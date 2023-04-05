namespace Retro_ML.NEAT.Configuration;
public class ExperimentSettings : IEquatable<ExperimentSettings>
{
    public int NeuralInputCount { get; init; }
    public int NeuralOutputCount { get; init; }

    public bool Equals(ExperimentSettings? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;

        return NeuralOutputCount == other.NeuralOutputCount && NeuralInputCount == other.NeuralInputCount;
    }
}
