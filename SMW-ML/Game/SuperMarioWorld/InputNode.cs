using System;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class InputNode
    {
        private readonly Func<DataFetcher, bool>? getStateFunction;
        private readonly Func<DataFetcher, bool[,]>? getStatesFunction;

        public string Name { get; }
        public bool ShouldUse { get; }
        public bool IsMultipleInputs { get; }
        public int TotalWidth { get; }
        public int TotalHeight { get; }

        public InputNode(string name, bool shouldUse, Func<DataFetcher, bool> getStateFunc) : this(name, shouldUse, false)
        {
            getStateFunction = getStateFunc;
        }

        public InputNode(string name, bool shouldUse, Func<DataFetcher, bool[,]> getStatesFunc, int totalWidth, int totalHeight) : this(name, shouldUse, true)
        {
            getStatesFunction = getStatesFunc;
            TotalWidth = totalWidth;
            TotalHeight = totalHeight;
        }

        private InputNode(string name, bool shouldUse, bool isMultiple)
        {
            Name = name;
            ShouldUse = shouldUse;
            IsMultipleInputs = isMultiple;
            TotalWidth = 1;
            TotalHeight = 1;
        }

        public bool GetState(DataFetcher dataFetcher) => getStateFunction!.Invoke(dataFetcher);
        
        public bool[,] GetStates(DataFetcher dataFetcher) => getStatesFunction!.Invoke(dataFetcher);
    }
}
