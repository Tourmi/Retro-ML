using System;

namespace SMW_ML.Game
{
    /// <summary>
    /// Represents a controller input state, which buttons are pressed, and which are not.
    /// </summary>
    public class Input
    {
        public const int BUTTON_COUNT = 12;

        private static readonly Buttons[] buttons = new Buttons[] {
            Buttons.A,
            Buttons.B,
            Buttons.X,
            Buttons.Y,
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
            X = 'X',
            Y = 'Y',
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

        public Input(string inputs)
        {
            foreach (var chara in inputs)
            {
                SetButtonState((Buttons)chara, true);
            }
        }

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

        /// <summary>
        /// Returns two bitmaps based on the input, the first one containing, in order, the status of the A B X Y left right up down key. The second one contains the status of LShoulder RShoulder Start Select.
        /// Used for Serial port communications.
        /// </summary>
        /// <returns></returns>
        public byte[] GetButtonBytes()
        {
            byte byte1 = 0;

            for (int i = 0; i < 8; i++)
            {
                byte1 |= (byte)((inputMask[i] ? 1 : 0) << i);
            }

            byte byte2 = 0;

            for (int i = 0; i < 4; i++)
            {
                byte2 |= (byte)((inputMask[i + 8] ? 1 : 0) << i);
            }

            return new byte[] { byte1, byte2 };
        }

        public override string ToString()
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

        public static Buttons IndexToButton(int index) => buttons[index];
    }
}
