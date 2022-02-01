using SharpNeat.BlackBox;
using SMW_ML.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld
{
    public class OutputGetter
    {
        private readonly List<OutputNode> outputNodes;

        private const bool ALLOW_OPPOSITE_DIRECTIONS = false;

        private const double ACTIVATION_THRESHOLD = 0;

        public OutputGetter(NeuralConfig config)
        {
            outputNodes = config.OutputNodes;
        }

        public Input GetControllerInput(IVector<double> outputs)
        {
            string controllerInputs = "";
            int currIndex = 0;
            int controllerIndex = 0;

            int outputCount = GetOutputCount();

            for(; currIndex < outputCount && controllerIndex < Input.BUTTON_COUNT; currIndex++, controllerIndex++)
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

        public int GetOutputCount()
        {
            int count = 0;
            foreach (var output in  outputNodes)
            {
                if (output.ShouldUse) count++;
            }

            return count;
        }
    }
}
