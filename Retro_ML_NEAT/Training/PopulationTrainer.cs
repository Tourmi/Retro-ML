using Newtonsoft.Json;
using Retro_ML.NEAT.Configuration;
using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Populations;
using Retro_ML.NEAT.Utils;

namespace Retro_ML.NEAT.Training;
internal class PopulationTrainer : IPopulationTrainer
{
    private readonly Random random;
    private readonly NEATConfiguration config;
    private readonly ExperimentSettings experiment;
    private readonly GenomeReproductor reproductor;
    private Population population;
    private Func<IEvaluator>? evaluatorGenerator;

    public PopulationTrainer(NEATConfiguration configuration, ExperimentSettings experiment, Random random)
    {
        config = configuration;
        this.experiment = experiment;
        this.random = random;
        this.reproductor = new GenomeReproductor(configuration, experiment, random);

        population = new() { PopulationExperiment = experiment };
        IsInitialized = false;
    }

    public bool IsInitialized { get; set; }

    #region Serialization
    public void LoadPopulation(string filepath)
    {
        population = JsonConvert.DeserializeObject<Population>(File.ReadAllText(filepath))!;
        if (population.PopulationExperiment != experiment) throw new Exception("The population that was loaded was not compatible with the current experiment settings");
    }

    public void SavePopulation(string filepath)
    {
        File.WriteAllText(filepath, JsonConvert.SerializeObject(population));
    }
    #endregion

    #region Algo
    public void Initialize(Func<IEvaluator> evaluatorGenerator)
    {
        this.evaluatorGenerator = evaluatorGenerator;
        if (population.AllGenomes.Any()) return;

        var genomes = reproductor.Initialize();
        AssignSpeciesAndAddToPopulation(genomes);

        EvaluatePopulation();
        AdjustFitnesses();
        SortSpeciesAndGenomes();
    }

    public void RunOneGeneration()
    {
        reproductor.NewGeneration();

        PruneStaleSpecies();
        SelectSpeciesRepresentatives();

        EliminateLowPerforming();

        var children = ReproduceAndMutate();

        PrunePopulation();

        AssignSpeciesAndAddToPopulation(children);
        PruneEmptySpecies();

        EvaluatePopulation();
        AdjustFitnesses();
        SortSpeciesAndGenomes();
    }

    private void SelectSpeciesRepresentatives()
    {
        foreach (Species s in population.Species)
        {
            s.Representative = random.RandomFromList(s.Genomes);
        }
    }

    private void EliminateLowPerforming()
    {
        foreach (var species in population.Species)
        {
            int removeCount = (int)Math.Floor(species.Genomes.Count * config.ReproductionConfig.RemoveRatio);
            if (removeCount < 1 || species.Genomes.Count == 1) continue;

            for (int i = removeCount; i < species.Genomes.Count;)
            {
                var g = species.Genomes[^i];
                population.AllGenomes.Remove(g);
                species.Genomes.RemoveAt(species.Genomes.Count - i);
            }
        }
    }

    private List<Genome> ReproduceAndMutate()
    {
        int targetCount = config.ReproductionConfig.TargetPopulation - config.ReproductionConfig.EliteSpeciesCount * config.ReproductionConfig.EliteGenomeCount;

        var genomes = new List<Genome>();
        var adjustedFitnesses = population.Species.Select(s => s.AdjustedFitnessSum.Score).ToList();
        var totalAdjustedFitnesses = adjustedFitnesses.Sum();
        while (targetCount > 0)
        {
            var speciesToReproduce = random.PickRandomFromWeightedList(population.Species, adjustedFitnesses, totalAdjustedFitnesses);
            var genomeToReproduce = random.PickRandomFromWeightedList(speciesToReproduce.Genomes, speciesToReproduce.Genomes.Select(g => g.AdjustedFitness.Score).ToList(), adjustedFitnesses[population.Species.IndexOf(speciesToReproduce)]);

            if (random.NextDouble() < config.ReproductionConfig.CrossoverOdds)
            {
                var otherSpeciesToReproduce = random.PickRandomFromWeightedList(population.Species, adjustedFitnesses, totalAdjustedFitnesses);
                var otherGenomeToReproduce = random.PickRandomFromWeightedList(otherSpeciesToReproduce.Genomes, otherSpeciesToReproduce.Genomes.Select(g => g.AdjustedFitness.Score).ToList(), adjustedFitnesses[population.Species.IndexOf(otherSpeciesToReproduce)]);

                var (bestGenome, otherGenome) = genomeToReproduce.AdjustedFitness.CompareTo(otherGenomeToReproduce.AdjustedFitness) < 0 ? (otherGenomeToReproduce, genomeToReproduce) : (genomeToReproduce, otherGenomeToReproduce);
                genomes.Add(reproductor.Crossover(bestGenome, otherGenome));
            }
            else
            {
                genomes.Add(reproductor.Mutate(genomeToReproduce));
            }

            targetCount--;
        }
        return genomes;
    }

    private void PruneStaleSpecies()
    {
        List<Species> toRemove = new();
        for (int i = config.ReproductionConfig.EliteSpeciesCount; i < population.Species.Count; i++)
        {
            if (population.Species[i].GensSinceLastProgress > config.SpeciesConfig.PruneAfterXGenerationsWithoutProgress)
            {
                toRemove.Add(population.Species[i]);
                foreach (var genome in population.Species[i].Genomes)
                {
                    population.AllGenomes.Remove(genome);
                }
            }
        }

        foreach (var species in toRemove)
        {
            population.Species.Remove(species);
        }
    }

    private void PrunePopulation()
    {
        for (int i = 0; i < population.Species.Count; i++)
        {
            int eliteCount = 0;
            for (int j = 0; j < population.Species[i].Genomes.Count; j++)
            {
                if (i < config.ReproductionConfig.EliteSpeciesCount && eliteCount < config.ReproductionConfig.EliteGenomeCount)
                {
                    eliteCount++;
                    continue;
                }

                population.AllGenomes.Remove(population.Species[i].Genomes[j]);
                population.Species[i].Genomes.RemoveAt(j);
                j--;
            }
        }
    }

    private void PruneEmptySpecies() => population.Species.RemoveAll(s => !s.Genomes.Any());

    private void AssignSpeciesAndAddToPopulation(IEnumerable<Genome> newGenomes)
    {
        double excessWeight = config.SpeciesConfig.DeltaExcessGenesWeight;
        double disjointWeight = config.SpeciesConfig.DeltaDisjointGenesWeight;
        double weightWeight = config.SpeciesConfig.DeltaAverageWeightDifferenceWeight;
        int minimumN = config.SpeciesConfig.MinimumGeneCountToNormalizeExcessDisjoint;
        double maxDelta = config.SpeciesConfig.SpeciesMaxDelta;

        foreach (var genome in newGenomes)
        {
            bool addedToSpecies = false;
            foreach (var species in population.Species)
            {
                if (species.Representative.GetDelta(genome, excessWeight, disjointWeight, weightWeight, minimumN) <= maxDelta)
                {
                    species.Genomes.Add(genome);
                    addedToSpecies = true;
                }
            }
            if (!addedToSpecies)
            {
                var newSpecies = new Species() { Representative = genome };
                newSpecies.Genomes.Add(genome);
                population.Species.Add(newSpecies);
            }

            population.AllGenomes.Add(genome);
        }
    }

    private void EvaluatePopulation()
    {
        _ = Parallel.For(0, population.AllGenomes.Count, (i) =>
        {
            var currGenome = population.AllGenomes[i];

            var evaluator = evaluatorGenerator!();
            var fitness = evaluator.Evaluate(currGenome.ToPhenome());
            currGenome.Fitness = fitness;
        });
    }

    private void AdjustFitnesses()
    {
        foreach (var species in population.Species)
        {
            species.GensSinceLastProgress++;
            double adjustedFitnessTotal = 0;
            var genMaxFitness = new Fitness(double.MinValue);
            foreach (var genome in species.Genomes)
            {
                genMaxFitness = Fitness.Max(genMaxFitness, genome.Fitness);
                genome.AdjustedFitness = new Fitness(genome.Fitness.Score / species.Genomes.Count);
                adjustedFitnessTotal += genome.AdjustedFitness.Score;
            }

            if (genMaxFitness.CompareTo(species.BestFitness) > 0)
            {
                species.GensSinceLastProgress = 0;
                species.BestFitness = genMaxFitness;
            }

            species.AdjustedFitnessSum = new Fitness(adjustedFitnessTotal);
        }
    }

    private void SortSpeciesAndGenomes()
    {
        population.Species = population.Species.OrderByDescending(s => s.BestFitness).ToList();
        foreach (var species in population.Species)
        {
            species.SortGenomes();
        }
        population.AllGenomes = population.AllGenomes.OrderByDescending(g => g.Fitness).ToList();
    }
    #endregion
}
