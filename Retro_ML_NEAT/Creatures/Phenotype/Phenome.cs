using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Utils;

namespace Retro_ML.NEAT.Creatures.Phenotype;
internal sealed class Phenome : IPhenome
{
    private readonly Connection[] connections;
    private readonly double[] nodeValues;
    private readonly int[] nodesDepth;
    private readonly int[] outputNodes;
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

        outputNodes = nodesDepth.Select((nd, i) => (nd, i)).Where((v) => v.nd == maximumDepth).Select(v => v.i).ToArray();

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

    public ((int input, int output, double weight)[][], int[] outputIds) GetConnectionLayers()
    {
        var layers = connections.GroupBy(c => c.Depth).ToList();
        (int, int, double)[][] resultLayers = new (int, int, double)[layers.Count][];
        for (int i = 0; i < layers.Count; i++)
        {
            var layerList = layers[i].ToList();
            (int, int, double)[] resultLayer = new (int, int, double)[layerList.Count];

            for (int j = 0; j < layerList.Count; j++)
            {
                resultLayer[j] = (layerList[j].InputNode, layerList[j].OutputNode, layerList[j].Weight);
            }

            resultLayers[i] = resultLayer;
        }

        return (resultLayers, outputNodes);
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
