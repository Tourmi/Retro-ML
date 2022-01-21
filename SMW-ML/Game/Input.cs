using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game
{
    public class Input
    {
        public const int BUTTON_COUNT = 12;

        private readonly Buttons[] buttons = new Buttons[] {
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

        public void SetButtonState(Buttons button, bool state) => inputMask[Array.IndexOf(buttons, button)] = state;
        public bool GetButtonState(Buttons button) => inputMask[Array.IndexOf(buttons, button)];

        /// <summary>
        /// Returns two bitmaps based on the input, the first one containing, in order, the status of the A B X Y left right up down key. The second one contains the status of LShoulder RShoulder Start Select.
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

        public static Buttons IndexToButton(int index) => Enum.GetValues<Buttons>()[index];
    }
}
