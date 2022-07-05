using Retro_ML.Game;

namespace Retro_ML.Neural
{
    /// <summary>
    /// Configuration of a single neural network input node.
    /// </summary>
    public class InputNode : INode
    {
        private readonly Func<IDataFetcher, double>? getStateFunction;
        private readonly Func<IDataFetcher, double[,]>? getStatesFunction;

        public string Name { get; }
        public bool ShouldUse { get; }
        public bool IsMultipleNodes { get; }
        public int TotalWidth { get; }
        public int TotalHeight { get; }

        public InputNode(string name, bool shouldUse, Func<IDataFetcher, bool> getStateFunc) : this(name, shouldUse, 1, 1)
        {
            getStateFunction = (dataFetcher) => getStateFunc(dataFetcher) ? 1 : 0;
        }

        public InputNode(string name, bool shouldUse, Func<IDataFetcher, bool[,]> getStatesFunc, int totalWidth, int totalHeight) : this(name, shouldUse, totalWidth, totalHeight)
        {
            getStatesFunction = (datafetcher) =>
            {
                bool[,] states = getStatesFunc(datafetcher);
                double[,] values = new double[states.GetLength(0), states.GetLength(1)];
                for (int i = 0; i < states.GetLength(0); i++)
                {
                    for (int j = 0; j < states.GetLength(1); j++)
                    {
                        values[i, j] = states[i, j] ? 1 : 0;
                    }
                }

                return values;
            };
        }

        public InputNode(string name, bool shouldUse, Func<IDataFetcher, double> getStateFunc) : this(name, shouldUse, 1, 1)
        {
            getStateFunction = getStateFunc;
        }

        public InputNode(string name, bool shouldUse, Func<IDataFetcher, double[,]> getStatesFunc, int totalWidth, int totalHeight) : this(name, shouldUse, totalWidth, totalHeight)
        {
            getStatesFunction = getStatesFunc;
        }

        public InputNode(string name, bool shouldUse, int width, int height)
        {
            Name = name;
            ShouldUse = shouldUse;
            IsMultipleNodes = width * height > 1;
            TotalWidth = width;
            TotalHeight = height;
        }

        /// <summary>
        /// Returns the state of this single input node.
        /// </summary>
        /// <param name="dataFetcher"></param>
        /// <returns></returns>
        public double GetState(IDataFetcher dataFetcher) => getStateFunction!.Invoke(dataFetcher);

        /// <summary>
        /// Returns the states of the input node array.
        /// </summary>
        /// <param name="dataFetcher"></param>
        /// <returns></returns>
        public double[,] GetStates(IDataFetcher dataFetcher) => getStatesFunction!.Invoke(dataFetcher);
    }
}