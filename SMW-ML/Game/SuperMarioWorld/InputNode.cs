﻿using System;

namespace SMW_ML.Game.SuperMarioWorld
{
    /// <summary>
    /// Configuration of a single neural network input node.
    /// </summary>
    internal class InputNode
    {
        private readonly Func<DataFetcher, bool>? getStateFunction;
        private readonly Func<DataFetcher, bool[,]>? getStatesFunction;

        /// <summary>
        /// Name of this input node.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Whether or not the neural network should use this input node.
        /// </summary>
        public bool ShouldUse { get; }
        /// <summary>
        /// Whether or not this input node is actually an array of input nodes
        /// </summary>
        public bool IsMultipleInputs { get; }
        /// <summary>
        /// The width of the input node array.
        /// </summary>
        public int TotalWidth { get; }
        /// <summary>
        /// Height of the input node array.
        /// </summary>
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

        /// <summary>
        /// Returns the state of this single input node.
        /// </summary>
        /// <param name="dataFetcher"></param>
        /// <returns></returns>
        public bool GetState(DataFetcher dataFetcher) => getStateFunction!.Invoke(dataFetcher);

        /// <summary>
        /// Returns the states of the input node array.
        /// </summary>
        /// <param name="dataFetcher"></param>
        /// <returns></returns>
        public bool[,] GetStates(DataFetcher dataFetcher) => getStatesFunction!.Invoke(dataFetcher);
    }
}
