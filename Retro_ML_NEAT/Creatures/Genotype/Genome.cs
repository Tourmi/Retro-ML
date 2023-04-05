using Retro_ML.NEAT.Creatures.Phenotype;
using Retro_ML.NEAT.Training;

namespace Retro_ML.NEAT.Creatures.Genotype;
internal class Genome : IGenome
{
    public Genome()
    {
        ConnectionGenes = Array.Empty<ConnectionGene>();

        Fitness = new(0);
        AdjustedFitness = new(0);
    }

    public int InputNodeCount { get; init; }
    public int OutputNodeCount { get; init; }
    public int TotalNodeCount { get; set; }

    public ConnectionGene[] ConnectionGenes { get; set; }

    public string? InputActivationFunction { get; init; }
    public string? HiddenActivationFunction { get; init; }
    public string? OutputActivationFunction { get; init; }

    public Fitness Fitness { get; set; }
    public Fitness AdjustedFitness { get; set; }

    public Phenome ToPhenome() => new(this);
    public IPhenome GetPhenome() => ToPhenome();

    /// <summary>
    /// Returns the depth in the network for each node, 0 meaning the node is in the input layer, and the final layer being the output layer
    /// </summary>
    /// <returns></returns>
    public (int[] depths, int maximumDepth) GetNodesDepth()
    {
        var depths = new int[TotalNodeCount];

        int maximumLayer = 0;

        bool updateNeeded = true;
        while (updateNeeded) //will endlessly loop if a cyclic connection exists
        {
            updateNeeded = false;
            for (int i = 0; i < ConnectionGenes.Length; i++)
            {
                if (!ConnectionGenes[i].Enabled) continue;

                var outputNode = ConnectionGenes[i].OutputNode;
                var inputNode = ConnectionGenes[i].InputNode;

                if (depths[outputNode] <= depths[inputNode])
                {
                    depths[outputNode] = depths[inputNode] + 1;
                    maximumLayer = depths[inputNode] + 1;
                    updateNeeded = true;
                }
            }
        }

        //update output nodes
        for (int i = InputNodeCount; i < InputNodeCount + OutputNodeCount; i++)
        {
            depths[i] = maximumLayer + 1;
        }

        return (depths, maximumLayer + 1);
    }

    public double GetDelta(Genome other, double excessMulti, double disjointMulti, double weightMulti, int minimumN)
    {
        int disjointCount = 0;
        int commonCount = 0;
        double weightDiffs = 0;

        int i = 0;
        int j = 0;
        while (i < ConnectionGenes.Length && j < other.ConnectionGenes.Length)
        {
            if (ConnectionGenes[i].InnovationNumber > other.ConnectionGenes[j].InnovationNumber)
            {
                disjointCount++;
                j++;
            }
            else if (ConnectionGenes[i].InnovationNumber < other.ConnectionGenes[j].InnovationNumber)
            {
                disjointCount++;
                i++;
            }
            else
            {
                weightDiffs += Math.Abs(ConnectionGenes[i].Weight - other.ConnectionGenes[j].Weight);
                commonCount++;
                i++;
                j++;
            }
        }

        int excessCount = ConnectionGenes.Length - i + other.ConnectionGenes.Length - j;

        int totalCount = Math.Max(ConnectionGenes.Length, other.ConnectionGenes.Length);
        totalCount = totalCount >= minimumN ? totalCount : 1; //if N is not reached, each extra gene are way more important
        return (excessCount * excessMulti + disjointCount * disjointMulti) / totalCount + weightDiffs / commonCount * weightMulti;
    }

    public Genome Copy() => new()
    {
        ConnectionGenes = ConnectionGenes.Select(g => g.Copy()).ToArray(),
        HiddenActivationFunction = HiddenActivationFunction,
        InputActivationFunction = InputActivationFunction,
        InputNodeCount = InputNodeCount,
        OutputActivationFunction = OutputActivationFunction,
        OutputNodeCount = OutputNodeCount,
        TotalNodeCount = TotalNodeCount
    };

    public Genome WithGenes(IEnumerable<ConnectionGene> genes) => new()
    {
        ConnectionGenes = genes.ToArray(),
        HiddenActivationFunction = HiddenActivationFunction,
        InputActivationFunction = InputActivationFunction,
        InputNodeCount = InputNodeCount,
        OutputActivationFunction = OutputActivationFunction,
        OutputNodeCount = OutputNodeCount,
        TotalNodeCount = TotalNodeCount
    };
}
