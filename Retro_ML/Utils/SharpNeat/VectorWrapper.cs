using Retro_ML.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.Utils.SharpNeat;
public class VectorWrapper : INeuralWrapper
{
    private readonly IVector<double> vector;

    public VectorWrapper(IVector<double> vector)
    {
        this.vector = vector;
    }

    public double this[int index]
    {
        get => vector[index];
        set => vector[index] = value;
    }

    public int Length => vector.Length;

    public double[] ToArray() => SharpNeatUtils.VectorToArray(vector);
}
