using System;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class InternalClock
    {
        private int tickLength;
        private int internalClockTimer;
        private bool[] states;

        public InternalClock(int tickLength, int inputCount)
        {
            this.tickLength = tickLength;

            states = new bool[inputCount];
            internalClockTimer = tickLength;
        }

        public void NextFrame()
        {
            if (internalClockTimer <= 0)
            {
                internalClockTimer = tickLength;

                int currStateIndex = 0;
                while (currStateIndex < states.Length && states[currStateIndex])
                {
                    states[currStateIndex] = !states[currStateIndex];
                    currStateIndex++;
                }
                if (currStateIndex < states.Length)
                {
                    states[currStateIndex] = !states[currStateIndex];
                }
            }
            internalClockTimer--;
        }

        public void Reset()
        {
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = false;
            }

            internalClockTimer = tickLength;
        }

        public bool[,] GetStates()
        {
            bool[,] result = new bool[Math.Max(1, states.Length / 8), Math.Min(8, states.Length)];

            int currRow = 0;
            int currCol = 0;
            for (int i = 0; i < states.Length; i++)
            {
                result[currRow, currCol] = states[i];

                currCol++;
                if (currCol >= result.GetLength(1))
                {
                    currCol = 0;
                    currRow++;
                }
            }

            return result;
        }
    }
}
