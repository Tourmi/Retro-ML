using Retro_ML.Configuration;
using Retro_ML.Neural;
using Retro_ML.Neural.Memory;
using SharpNeat.BlackBox;

namespace Retro_ML.Game
{
    /// <summary>
    /// Sets the inputs of the neural network
    /// </summary>
    public class InputSetter
    {
        private readonly List<InputNode> inputNodes;

        private readonly IDataFetcher dataFetcher;

        private readonly NeuralMemory neuralMemory;

        public InputSetter(IDataFetcher dataReader, NeuralConfig config, NeuralMemory neuralMemory)
        {
            dataFetcher = dataReader;
            inputNodes = config.InputNodes;

            this.neuralMemory = neuralMemory;
        }

        /// <summary>
        /// Sets the states of the given input vector based on the input nodes.
        /// </summary>
        public void SetInputs(IVector<double> inputs)
        {
            int currOffset = 0;

            foreach (var input in inputNodes)
            {
                if (!input.ShouldUse) continue;

                if (input.IsMultipleNodes)
                {
                    var inputStates = input.GetStates(dataFetcher);
                    for (int i = 0; i < inputStates.GetLength(0); i++)
                    {
                        for (int j = 0; j < inputStates.GetLength(1); j++)
                        {
                            inputs[currOffset++] = inputStates[i, j];
                        }
                    }
                }
                else
                {
                    inputs[currOffset++] = input.GetState(dataFetcher);
                }
            }

            neuralMemory.SetMemory(inputs, currOffset);
        }
    }
}