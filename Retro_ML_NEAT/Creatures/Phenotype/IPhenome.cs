namespace Retro_ML.NEAT.Creatures.Phenotype;
public interface IPhenome
{
    /// <summary>
    /// Amount of inputs the phenome has
    /// </summary>
    public int InputCount { get; }
    /// <summary>
    /// Amount of outputs the phenome has
    /// </summary>
    public int OutputCount { get; }

    /// <summary>
    /// Input span of the phenome
    /// </summary>
    public ArraySegment<double> Inputs { get; }
    /// <summary>
    /// Output span of the phenome
    /// </summary>
    public ArraySegment<double> Outputs { get; }

    /// <summary>
    /// Takes the given <see cref="Inputs"/> values and activates the phenome, storing the result in the <see cref="Outputs"/>
    /// </summary>
    public void Activate();
}
