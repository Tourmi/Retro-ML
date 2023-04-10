using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Utils;

namespace Retro_ML.NEAT.Creatures.Phenotype;
internal sealed class Phenome : IPhenome
{
    private readonly Connection[] connections;
    private readonly double[] nodeValues;
    private readonly int[] nodesDepth;
    private readonly int maximumDepth;

    private readonly Func<double, double> inputFn;
    private readonly Func<double, double> hiddenFn;
    private readonly Func<double, double> outputFn;

    public Phenome(Genome genome)
    {
        inputFn = ActivationFunctions.GetActivationFunction(genome.InputActivationFunction);
        hiddenFn = ActivationFunctions.GetActivationFunction(genome.HiddenActivationFunction);
        outputFn = ActivationFunctions.GetActivationFunction(genome.OutputActivationFunction);

        nodeValues = new double[genome.TotalNodeCount];
        InputCount = genome.InputNodeCount;
        OutputCount = genome.OutputNodeCount;

        (nodesDepth, maximumDepth) = genome.GetNodesDepth();

        List<Connection> activeConnections = new();
        foreach (var c in genome.ConnectionGenes)
        {
            if (!c.Enabled) continue;

            activeConnections.Add(new() { Depth = nodesDepth[c.InputNode], InputNode = c.InputNode, OutputNode = c.OutputNode, Weight = c.Weight });
        }

        connections = activeConnections.OrderBy(ac => ac.Depth).ThenBy(ac => ac.InputNode).ToArray();
    }

    public int InputCount { get; }
    public int OutputCount { get; }

    public ArraySegment<double> Inputs => new(nodeValues, 0, InputCount);

    public ArraySegment<double> Outputs => new(nodeValues, InputCount, OutputCount);

    public void Activate()
    {
        var currLayer = -1;
        for (int i = 0; i < connections.Length; i++)
        {
            var curr = connections[i];
            if (currLayer != curr.Depth)
            {
                currLayer = curr.Depth;
                ActivateLayer(currLayer, i);
            }

            nodeValues[curr.OutputNode] += nodeValues[curr.InputNode] * curr.Weight;
        }

        ActivateLayer(maximumDepth, InputCount);
    }

    private void ActivateLayer(int depth, int connectionIndex)
    {
        var activationFunction = depth == 0 ? inputFn : depth == maximumDepth ? outputFn : hiddenFn;

        while (connectionIndex < connections.Length && nodesDepth[connections[connectionIndex].InputNode] == depth)
        {
            nodeValues[connections[connectionIndex].InputNode] = activationFunction(nodeValues[connections[connectionIndex].InputNode]);

            connectionIndex++;
        }
    }
}
