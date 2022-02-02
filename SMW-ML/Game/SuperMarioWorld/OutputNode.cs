namespace SMW_ML.Game.SuperMarioWorld
{
    public class OutputNode
    {
        public string Name { get; }
        public bool ShouldUse { get; }

        public OutputNode(string name, bool shouldUse)
        {
            Name = name;
            ShouldUse = shouldUse;
        }


    }
}
