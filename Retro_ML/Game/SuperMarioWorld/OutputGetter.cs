using SharpNeat.BlackBox;
using Retro_ML.Models.Config;
using System.Collections.Generic;

namespace Retro_ML.Game.SuperMarioWorld
{
    /// <summary>
    /// Gets the state of the output neurons of the neural network, and parses it to a controller input.
    /// </summary>
    internal class OutputGetter
    {
        private readonly List<OutputNode> outputNodes;
        private readonly NeuralConfig neuralConfig;

        private const bool ALLOW_OPPOSITE_DIRECTIONS = false;

        private const double ACTIVATION_THRESHOLD = 0;

        public OutputGetter(NeuralConfig config)
        {
            outputNodes = config.OutputNodes;
            neuralConfig = config;
        }
        /// <summary>
        /// Gets the state of the output neurons of the neural network, and parses it to a controller input.
        /// </summary>
        /// <param name="outputs"></param>
        /// <returns></returns>
        public Input GetControllerInput(IVector<double> outputs)
        {
            string controllerInputs = "";
            int currIndex = 0;
            int controllerIndex = 0;

            int outputCount = neuralConfig.GetOutputCount();

            for (; currIndex < outputCount && controllerIndex < Input.BUTTON_COUNT; currIndex++, controllerIndex++)
            {
                if (!outputNodes[controllerIndex].ShouldUse)
                {
                    currIndex--;
                    continue;
                }

                if (outputs[currIndex] > ACTIVATION_THRESHOLD)
                {
                    controllerInputs += (char)Input.IndexToButton(controllerIndex);
                }
            }

            Input controllerInput = new(controllerInputs);

            if (!ALLOW_OPPOSITE_DIRECTIONS)
            {
                if (controllerInput.GetButtonState(Input.Buttons.Left) && controllerInput.GetButtonState(Input.Buttons.Right))
                {
                    controllerInput.SetButtonState(Input.Buttons.Left, false);
                    controllerInput.SetButtonState(Input.Buttons.Right, false);
                }
                if (controllerInput.GetButtonState(Input.Buttons.Up) && controllerInput.GetButtonState(Input.Buttons.Down))
                {
                    controllerInput.SetButtonState(Input.Buttons.Up, false);
                    controllerInput.SetButtonState(Input.Buttons.Down, false);
                }
            }

            return controllerInput;
        }
    }
}
