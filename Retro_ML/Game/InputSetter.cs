using Retro_ML.Configuration;
using Retro_ML.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.Game
{
    /// <summary>
    /// Sets the inputs of the neural network
    /// </summary>
    public class InputSetter
    {
        private readonly List<InputNode> inputNodes;

        private IDataFetcher dataFetcher;

        public InputSetter(IDataFetcher dataReader, NeuralConfig config)
        {
            dataFetcher = dataReader;
            inputNodes = config.InputNodes;
        }

        /// <summary>
        /// Sets the states of the given input vector based on the input nodes.
        /// </summary>
        /// <param name="inputs"></param>
        public void SetInputs(IVector<double> inputs)
        {
            int currOffset = 0;

            foreach (var input in inputNodes)
            {
                if (!input.ShouldUse) continue;

                if (input.IsMultipleInputs)
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
        }
    }
}
