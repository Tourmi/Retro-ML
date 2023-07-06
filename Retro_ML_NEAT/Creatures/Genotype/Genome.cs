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

    internal Phenome ToPhenome() => new(this);
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
                var inputNode = ConnectionGenes[i].InputNode;
                var outputNode = ConnectionGenes[i].OutputNode;

                if (depths[outputNode] <= depths[inputNode])
                {
                    depths[outputNode] = depths[inputNode] + 1;
                    maximumLayer = Math.Max(depths[inputNode] + 1, maximumLayer);
                    updateNeeded = true;
                }
            }
        }

        //update output nodes
        for (int i = InputNodeCount; i < InputNodeCount + OutputNodeCount; i++)
        {
            depths[i] = maximumLayer;
        }

        return (depths, maximumLayer);
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

    public bool HasCycle()
    {
        for (int i = 0; i < InputNodeCount; i++)
        {
            if (CheckCycle(new(), i)) return true;
        }
        return false;
    }

    private bool CheckCycle(Stack<int> currentStack, int input)
    {
        if (currentStack.Contains(input)) return true;
        currentStack.Push(input);

        foreach (var connection in ConnectionGenes.Where(c => c.InputNode == input))
        {
            if (CheckCycle(currentStack, connection.OutputNode)) return true;
        }
        _ = currentStack.Pop();
        return false;
    }

    public Genome Copy() => new()
    {
        ConnectionGenes = ConnectionGenes.ToArray(),
        HiddenActivationFunction = HiddenActivationFunction,
        InputActivationFunction = InputActivationFunction,
        InputNodeCount = InputNodeCount,
        OutputActivationFunction = OutputActivationFunction,
        OutputNodeCount = OutputNodeCount,
        TotalNodeCount = TotalNodeCount,
        Fitness = Fitness,
        AdjustedFitness = AdjustedFitness,
    };

    public Genome WithGenes(IEnumerable<ConnectionGene> genes) => new()
    {
        ConnectionGenes = genes.ToArray(),
        HiddenActivationFunction = HiddenActivationFunction,
        InputActivationFunction = InputActivationFunction,
        InputNodeCount = InputNodeCount,
        OutputActivationFunction = OutputActivationFunction,
        OutputNodeCount = OutputNodeCount,
        TotalNodeCount = TotalNodeCount,
        Fitness = Fitness,
        AdjustedFitness = AdjustedFitness,
    };
}
