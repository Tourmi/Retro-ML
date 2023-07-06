using Retro_ML.NEAT.Configuration;
using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Utils;

namespace Retro_ML.NEAT.Populations;
internal class GenomeReproductor
{
    private struct Innovation
    {
        public Innovation(int input, int output)
        {
            this.input = input;
            this.output = output;
        }

        public int input;
        public int output;
    }

    private readonly Random random;
    private readonly NEATConfiguration config;
    private readonly ExperimentSettings experiment;
    private long currentInovation;
    private Dictionary<Innovation, long> innovationIDs;

    public GenomeReproductor(NEATConfiguration neatConfiguration, ExperimentSettings experiment, Random random)
    {
        config = neatConfiguration;
        this.experiment = experiment;
        this.random = random;
        innovationIDs = new();
    }

    public IEnumerable<Genome> Initialize()
    {
        int count = 0;
        var baseGenome = new Genome()
        {
            InputActivationFunction = config.GenomeConfig.InputActivationFunction,
            HiddenActivationFunction = config.GenomeConfig.HiddenActivationFunction,
            OutputActivationFunction = config.GenomeConfig.OutputActivationFunction,
            InputNodeCount = experiment.NeuralInputCount,
            OutputNodeCount = experiment.NeuralOutputCount,
            TotalNodeCount = experiment.NeuralInputCount + experiment.NeuralOutputCount,
            ConnectionGenes = Array.Empty<ConnectionGene>(),
        };

        while (count < config.ReproductionConfig.TargetPopulation)
        {
            yield return Mutate(baseGenome);
            count++;
        }
    }

    public void NewGeneration() => innovationIDs.Clear();

    public Genome Mutate(Genome toMutate)
    {
        for (int i = 0; i < config.ReproductionConfig.MutationIterations; i++)
        {
            toMutate = MutateWeights(toMutate);
            toMutate = MutateAddConnection(toMutate);
            toMutate = MutateAddNode(toMutate);
        }
        return toMutate;
    }

    internal Genome MutateWeights(Genome toMutate)
    {
        if (toMutate.ConnectionGenes.Length == 0) return toMutate;
        if (random.NextDouble() >= config.ReproductionConfig.AdjustWeightsOdds) return toMutate;

        var modifiedGenes = new List<ConnectionGene>();
        foreach (var connection in toMutate.ConnectionGenes)
        {
            if (random.NextDouble() < config.ReproductionConfig.WeightPerturbationOdds)
            {
                modifiedGenes.Add(connection.WithWeight(PertubedWeight(connection.Weight)));
            }
            else if (random.NextDouble() < config.ReproductionConfig.WeightShuffleOdds)
            {
                modifiedGenes.Add(connection.WithWeight(RandomWeight()));
            }
            else
            {
                modifiedGenes.Add(connection);
            }
        }

        return toMutate.WithGenes(modifiedGenes);
    }

    internal Genome MutateAddConnection(Genome toMutate)
    {
        if (random.NextDouble() >= config.ReproductionConfig.MutationAddConnectionOdds && toMutate.ConnectionGenes.Any()) return toMutate;
        var copiedGenes = toMutate.ConnectionGenes.ToList();
        var (nodeDepths, maximumDepth) = toMutate.GetNodesDepth();
        int randomInput = 0;
        int randomOutput = 0;

        int tries = 0;
        while (randomInput == randomOutput || copiedGenes.Where(g => g.InputNode == randomInput && g.OutputNode == randomOutput).Any())
        {
            if (tries > 100)
            {
                return toMutate; //too many connections
            }
            tries++;

            randomInput = random.RandomNode(nodeDepths, 0, maximumDepth - 1);
            randomOutput = random.RandomNode(nodeDepths, Math.Max(nodeDepths[randomInput], 1), maximumDepth);
        }

        var result = toMutate.WithGenes(copiedGenes.Append(new ConnectionGene()
        {
            Enabled = true,
            InnovationNumber = GetInnovationNumber(randomInput, randomOutput),
            InputNode = randomInput,
            OutputNode = randomOutput,
            Weight = RandomWeight()
        }));

        if (result.HasCycle())
        {
            return toMutate;
        }

        return result;
    }

    internal Genome MutateAddNode(Genome toMutate)
    {
        if (random.NextDouble() >= config.ReproductionConfig.MutationAddNodeOdds || toMutate.ConnectionGenes.Length <= 1) return toMutate;
        var copiedGenes = toMutate.ConnectionGenes.ToList();
        var chosenGeneToSplit = random.RandomFromList(copiedGenes.Where(g => g.Enabled).ToList());
        chosenGeneToSplit.Enabled = false;

        int nodeCount = toMutate.TotalNodeCount + 1;
        ConnectionGene splitInput = new() { InnovationNumber = GetInnovationNumber(chosenGeneToSplit.InputNode, nodeCount - 1), Enabled = true, InputNode = chosenGeneToSplit.InputNode, OutputNode = nodeCount - 1, Weight = chosenGeneToSplit.Weight };
        ConnectionGene splitOutput = new() { InnovationNumber = GetInnovationNumber(nodeCount - 1, chosenGeneToSplit.OutputNode), Enabled = true, InputNode = nodeCount - 1, OutputNode = chosenGeneToSplit.OutputNode, Weight = 1 };

        var result = toMutate.WithGenes(copiedGenes.Append(splitInput).Append(splitOutput));
        result.TotalNodeCount = nodeCount;
        return result;
    }

    public Genome Crossover(Genome best, Genome other)
    {
        var newGenes = new List<ConnectionGene>();

        var bestGenes = best.ConnectionGenes.ToList();
        var otherGenes = other.ConnectionGenes.ToList();

        for (int i = 0, j = 0; i < bestGenes.Count; i++, j++)
        {
            if (j >= otherGenes.Count)
            {
                newGenes.Add(bestGenes[i]);
                continue;
            }
            if (bestGenes[i].InnovationNumber < otherGenes[j].InnovationNumber)
            {
                newGenes.Add(bestGenes[i]);
                j--;
                continue;
            }
            if (bestGenes[i].InnovationNumber > otherGenes[j].InnovationNumber)
            {
                i--;
                continue;
            }

            var chosenGene = (random.Next(2) == 0 ? bestGenes[i] : otherGenes[j]);
            if (!bestGenes[i].Enabled || !otherGenes[j].Enabled) chosenGene.Enabled = random.NextDouble() >= config.ReproductionConfig.GeneRemainsDisabledOdds;
            newGenes.Add(chosenGene);
        }

        return best.WithGenes(newGenes);
    }

    internal long GetInnovationNumber(int input, int output) => GetInnovationNumber(new Innovation(input, output));

    private long GetInnovationNumber(Innovation innovation)
    {
        if (!innovationIDs.TryGetValue(innovation, out long innovationNumber))
        {
            innovationNumber = currentInovation++;
            innovationIDs[innovation] = innovationNumber;
        }
        return innovationNumber;
    }

    private double PertubedWeight(double weight) => (1 + ((random.NextDouble() * 2) - 1) * config.ReproductionConfig.WeightPerturbationPercentRange) * weight;
    private double RandomWeight() => ((random.NextDouble() * 2) - 1) * config.ReproductionConfig.MaximumWeightAmplitude;
}
