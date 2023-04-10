namespace Retro_ML.Neural;

/// <summary>
/// Interface that allows reading and writing to a neuron group
/// </summary>
public interface INeuralWrapper
{
    double this[int index] { get; set; }
    int Length { get; }
    double[] ToArray();
}
