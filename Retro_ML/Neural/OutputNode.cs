namespace Retro_ML.Neural;

/// <summary>
/// Configuration of a single output node
/// </summary>
public class OutputNode : INode
{
    public string Name { get; }
    public bool ShouldUse { get; }
    public bool IsMultipleNodes { get; }
    public int TotalWidth { get; }
    public int TotalHeight { get; }

    public OutputNode(string name, bool shouldUse)
    {
        Name = name;
        ShouldUse = shouldUse;
        IsMultipleNodes = false;
        TotalWidth = 1;
        TotalHeight = 1;
    }

    public OutputNode(string name, int width, int height)
    {
        Name = name;
        ShouldUse = width != 0 && height != 0;
        IsMultipleNodes = width * height > 1;
        TotalWidth = width;
        TotalHeight = height;
    }
}
