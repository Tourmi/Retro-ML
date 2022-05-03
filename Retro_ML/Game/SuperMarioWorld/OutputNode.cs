namespace Retro_ML.Game.SuperMarioWorld
{
    /// <summary>
    /// Configuration of a single output node
    /// </summary>
    internal class OutputNode
    {
        /// <summary>
        /// Name of the output node
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Whether or not the output node should be used by the neural network.
        /// </summary>
        public bool ShouldUse { get; }

        public OutputNode(string name, bool shouldUse)
        {
            Name = name;
            ShouldUse = shouldUse;
        }
    }
}
