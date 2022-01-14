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

        public void SetButtonState(Buttons button, bool state) => inputMask[Array.IndexOf(Enum.GetValues<Buttons>(), button)] = state;
        public bool GetButtonState(Buttons button) => inputMask[Array.IndexOf(Enum.GetValues<Buttons>(), button)];

        public static Buttons IndexToButton(int index) => Enum.GetValues<Buttons>()[index];
    }
}
