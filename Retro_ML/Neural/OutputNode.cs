namespace Retro_ML.Neural
{
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
        public bool UsesActivationThreshold { get; }
        public bool IsHalfActivationThreshold { get; }

        public OutputNode(string name, bool shouldUse, bool usesActivationThreshold = true, bool isHalfActivationThreshold = false)
        {
            Name = name;
            ShouldUse = shouldUse;
            IsMultipleNodes = false;
            TotalWidth = 1;
            TotalHeight = 1;
            UsesActivationThreshold = usesActivationThreshold;
            IsHalfActivationThreshold = isHalfActivationThreshold;
        }

        public OutputNode(string name, int width, int height, bool usesActivationThreshold = true, bool isHalfActivationThreshold = false)
        {
            Name = name;
            ShouldUse = width != 0 && height != 0;
            IsMultipleNodes = width * height > 1;
            TotalWidth = width;
            TotalHeight = height;
            UsesActivationThreshold = usesActivationThreshold;
            IsHalfActivationThreshold = isHalfActivationThreshold;
        }
    }
}