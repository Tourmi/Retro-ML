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
    private readonly Statistics stats;
    private Population population;
    private Func<IEvaluator>? evaluatorGenerator;

    public PopulationTrainer(NEATConfiguration configuration, ExperimentSettings experiment, Random random)
    {
        config = configuration;
        this.experiment = experiment;
        this.random = random;
        this.reproductor = new GenomeReproductor(configuration, experiment, random);
        this.stats = new();

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

    public void SaveBestGenome(string filepath)
    {
        File.WriteAllText(filepath, JsonConvert.SerializeObject(population.AllGenomes.First()));
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

        stats.GenerationCount++;
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
            int removeCount = (int)Math.Floor(species.Genomes.Count * config.ReproductionConfig.PreReproductionRemoveRatio);
            if (removeCount < 1 || species.Genomes.Count == 1) continue;

            for (int i = 0; i < removeCount; i++)
            {
                var g = species.Genomes.Last();
                population.AllGenomes.Remove(g);
                species.Genomes.Remove(g);
            }
        }
    }

    private List<Genome> ReproduceAndMutate()
    {
        int targetCount = config.ReproductionConfig.TargetPopulation - config.ReproductionConfig.EliteSpeciesCount * config.ReproductionConfig.EliteGenomeCount;

        var genomes = new List<Genome>();
        var adjustedFitnesses = population.Species.Select(s => s.AdjustedFitnessSum.Score).ToList();
        var totalAdjustedFitnesses = adjustedFitnesses.Sum();

        var countPerSpecies = adjustedFitnesses.Select(f => (int)Math.Floor(f / totalAdjustedFitnesses * targetCount)).ToList();
        targetCount -= countPerSpecies.Sum();

        //Since we always round down the count per species, we need to pick some randomly to increase back to the target number
        while (targetCount > 0)
        {
            int indexSpecies = random.PickRandomFromWeightedList(population.Species.Select((s, i) => i).ToList(), adjustedFitnesses, totalAdjustedFitnesses);
            countPerSpecies[indexSpecies]++;

            targetCount--;
        }

        var genomesToReproduce = countPerSpecies.SelectMany((s, i)
            => Enumerable.Repeat(0, s)
                         .Select(_ => random.PickRandomFromWeightedList(
                             population.Species[i].Genomes,
                             population.Species[i].Genomes.Select(g => g.AdjustedFitness.Score).ToList(),
                             adjustedFitnesses[i])));

        foreach (var genome in genomesToReproduce)
        {
            if (random.NextDouble() < config.ReproductionConfig.CrossoverOdds)
            {
                var otherSpeciesToReproduce = random.PickRandomFromWeightedList(population.Species, adjustedFitnesses, totalAdjustedFitnesses);
                var otherGenomeToReproduce = random.PickRandomFromWeightedList(otherSpeciesToReproduce.Genomes, otherSpeciesToReproduce.Genomes.Select(g => g.AdjustedFitness.Score).ToList(), otherSpeciesToReproduce.AdjustedFitnessSum.Score);

                var (bestGenome, otherGenome) = genome.AdjustedFitness.CompareTo(otherGenomeToReproduce.AdjustedFitness) < 0 ? (otherGenomeToReproduce, genome) : (genome, otherGenomeToReproduce);
                genomes.Add(reproductor.Crossover(bestGenome, otherGenome));
            }
            else
            {
                genomes.Add(reproductor.Mutate(genome));
            }
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
                    break;
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

            using var evaluator = evaluatorGenerator!();
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
            var genMaxFitness = new Fitness(0);
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

    #region Stats
    public Statistics GetStatistics()
    {
        stats.BestGenomeFitness = population.AllGenomes.FirstOrDefault()?.Fitness?.Score ?? 0;
        stats.BestSpeciesAverageFitness = population.Species.FirstOrDefault()?.Genomes?.Select(g => g.Fitness.Score)?.DefaultIfEmpty(0)?.Average() ?? 0;
        stats.AverageFitness = population.AllGenomes.Select(g => g.Fitness.Score).DefaultIfEmpty(0).Average();

        stats.BestGenomeComplexity = population.AllGenomes.FirstOrDefault()?.ConnectionGenes?.Length ?? 0;
        stats.AverageGenomeComplexity = population.AllGenomes.Select(g => g.ConnectionGenes.Length).DefaultIfEmpty(0).Average();
        stats.MaximumGenomeComplexity = population.AllGenomes.Select(g => g.ConnectionGenes.Length).DefaultIfEmpty(0).Max();

        stats.TotalSpecies = population.Species.Count;
        stats.BestSpeciesPopulation = population.Species.FirstOrDefault()?.Genomes?.Count ?? 0;
        stats.AverageSpeciesPopulation = population.Species.Select(s => s.Genomes.Count).DefaultIfEmpty(0).Average();

        return stats;
    }
    #endregion
}
