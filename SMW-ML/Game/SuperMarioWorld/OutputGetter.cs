using SharpNeat.BlackBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class OutputGetter
    {
        private const bool USE_SHOULDERS = false;
        private const bool USE_START = false;
        private const bool USE_SELECT = false;

        private const bool ALLOW_OPPOSITE_DIRECTIONS = false;

        private const double ACTIVATION_THRESHOLD = 0.1;

        public OutputGetter()
        {
        }

        public Input GetControllerInput(IVector<double> outputs)
        {
            string controllerInputs = "";
            int currIndex = 0;
            int controllerIndex = 0;

            for(; currIndex < GetOutputCount() && controllerIndex < Input.BUTTON_COUNT; currIndex++, controllerIndex++)
            {
                if (!USE_SHOULDERS && (controllerIndex == 8 || controllerIndex == 9))
                {
                    currIndex--;
                    continue;
                }
                if (!USE_START && (controllerIndex == 10))
                {
                    currIndex--;
                    continue;
                }
                if (!USE_SELECT && (controllerIndex == 11))
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
            count += 4; //buttons
            count += 4; //Dpad
            if (USE_SHOULDERS) count += 2;
            if (USE_START) count += 1;
            if (USE_SELECT) count += 1;

            return count;
        }
    }
}
