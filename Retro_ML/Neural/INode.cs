namespace Retro_ML.Neural;
/// <summary>
/// Interface that represents a neural network input node
/// </summary>
public interface INode
{
    /// <summary>
    /// Name of the node
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Whether or not the node should be used by the neural network.
    /// </summary>
    bool ShouldUse { get; }
    /// <summary>
    /// Whether or not this node is actually an array of nodes
    /// </summary>
    public bool IsMultipleNodes { get; }
    /// <summary>
    /// The width of the node array.
    /// </summary>
    public int TotalWidth { get; }
    /// <summary>
    /// Height of the node array.
    /// </summary>
    public int TotalHeight { get; }
}
