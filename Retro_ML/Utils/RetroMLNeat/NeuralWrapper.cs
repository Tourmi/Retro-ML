using Retro_ML.Neural;

namespace Retro_ML.Utils.RetroMLNeat;
public class NeuralWrapper : INeuralWrapper
{
    private ArraySegment<double> nodes;

    public NeuralWrapper(ArraySegment<double> nodes)
    {
        this.nodes = nodes;
    }

    public double this[int index] { get => nodes[index]; set => nodes[index] = value; }

    public int Length => nodes.Count;

    public double[] ToArray() => nodes.ToArray();
}
