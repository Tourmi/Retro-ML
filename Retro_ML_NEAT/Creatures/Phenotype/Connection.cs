namespace Retro_ML.NEAT.Creatures.Phenotype;
internal struct Connection
{
    /// <summary>
    /// Depth of the input node in the network
    /// </summary>
    public int Depth { get; init; }
    /// <summary>
    /// ID of this connection
    /// </summary>
    public int InputNode { get; init; }
    /// <summary>
    /// Target ID of this connection
    /// </summary>
    public int OutputNode { get; init; }
    /// <summary>
    /// Weight of the connection
    /// </summary>
    public double Weight { get; init; }
}
