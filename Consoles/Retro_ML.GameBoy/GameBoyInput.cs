using Retro_ML.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.GameBoy
{
    internal class GameBoyInput : IInput
    {
        public const int BUTTON_COUNT = 10;

        private static readonly Buttons[] buttons = new Buttons[] {
            Buttons.A,
            Buttons.B,
            Buttons.Left,
            Buttons.Right,
            Buttons.Up,
            Buttons.Down,
            Buttons.LeftShoulder,
            Buttons.RightShoulder,
            Buttons.Start,
            Buttons.Select
        };

        public enum Buttons
        {
            A = 'A',
            B = 'B',
            Left = 'l',
            Right = 'r',
            Up = 'u',
            Down = 'd',
            LeftShoulder = 'L',
            RightShoulder = 'R',
            Start = 'S',
            Select = 's'
        }

        private readonly bool[] inputMask = new bool[BUTTON_COUNT];

        public int ButtonCount => BUTTON_COUNT;

        /// <summary>
        /// Sets the state of a single button on the controller.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="state"></param>
        public void SetButtonState(Buttons button, bool state) => inputMask[Array.IndexOf(buttons, button)] = state;
        /// <summary>
        /// Returns whether or not a button on a controller is pressed.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool GetButtonState(Buttons button) => inputMask[Array.IndexOf(buttons, button)];

        public byte[] ToArduinoBytes()
        {
            byte byte1 = 0;

            for (int i = 0; i < 8; i++)
            {
                if (i == 2 || i == 3) continue;
                byte1 |= (byte)((inputMask[i] ? 1 : 0) << i);
            }

            byte byte2 = 0;

            for (int i = 0; i < 4; i++)
            {
                byte2 |= (byte)((inputMask[i + 8] ? 1 : 0) << i);
            }

            return new byte[] { byte1, byte2 };
        }

        public void FromString(string value)
        {
            for (int i = 0; i < inputMask.Length; i++)
            {
                inputMask[i] = false;
            }

            foreach (var chara in value)
            {
                SetButtonState((Buttons)chara, true);
            }
        }

        public string GetString()
        {
            string output = "";
            for (int i = 0; i < inputMask.Length; i++)
            {
                if (inputMask[i])
                {
                    output += (char)buttons[i];
                }
            }

            return output;
        }

        public void SetButton(int index, double value)
        {
            inputMask[index] = value > 0;
        }

        public void ValidateButtons()
        {
            if (GetButtonState(Buttons.Left) && GetButtonState(Buttons.Right))
            {
                SetButtonState(Buttons.Left, false);
                SetButtonState(Buttons.Right, false);
            }
            if (GetButtonState(Buttons.Up) && GetButtonState(Buttons.Down))
            {
                SetButtonState(Buttons.Up, false);
                SetButtonState(Buttons.Down, false);
            }
        }
    }
}
