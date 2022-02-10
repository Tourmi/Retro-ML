using SharpNeat.BlackBox;
using SMW_ML.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class InputSetter
    {
        private readonly List<InputNode> inputNodes;

        private DataFetcher dataReader;

        public InputSetter(DataFetcher dataReader, NeuralConfig config)
        {
            this.dataReader = dataReader;
            inputNodes = config.InputNodes;
        }

        public void SetInputs(IVector<double> inputs)
        {
            int currOffset = 0;

            foreach (var input in inputNodes)
            {
                if (!input.ShouldUse) continue;

                if (input.IsMultipleInputs)
                {
                    var inputStates = input.GetStates(dataReader);
                    for (int i = 0; i < inputStates.GetLength(0); i++)
                    {
                        for (int j = 0; j < inputStates.GetLength(1); j++)
                        {
                            inputs[currOffset++] = inputStates[i, j] ? 1 : 0;
                        }
                    }
                }
                else
                {
                    inputs[currOffset++] = input.GetState(dataReader) ? 1 : 0;
                }
            }
        }

        public int GetInputCount()
        {
            int count = 0;

            foreach(var input in inputNodes)
            {
                if (input.ShouldUse) count += input.TotalWidth * input.TotalHeight;
            }

            return count;
        }
    }
}
