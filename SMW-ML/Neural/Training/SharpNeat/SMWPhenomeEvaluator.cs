using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SMW_ML.Emulator;
using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training.SharpNeat
{
    internal class SMWPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        private const int INPUT_ROW_COUNT = 15;
        private const int INPUT_COLUMN_COUNT = 15;
        public const int INPUT_COUNT = INPUT_ROW_COUNT * INPUT_COLUMN_COUNT * 2 + 2; // 15x15 tile grid around Mario of solid tiles, as well as enemies. +1 for bias

        private const int FRAMES_TO_PROCESS = 1080;

        private readonly IEmulatorAdapter emulator;

        public SMWPhenomeEvaluator(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            double score = 0;

            emulator.Reserve();
            emulator.LoadState(emulator.GetStates()[0]);
            emulator.NextFrame();

            for (int i = 0; i < FRAMES_TO_PROCESS; i++)
            {
                score = DoFrame(phenome);
            }
            emulator.Free();

            return new FitnessInfo(score);
        }

        private double DoFrame(IBlackBox<double> phenome)
        {
            phenome.ResetState();

            int position = emulator.ReadMemory(0x000094) + (emulator.ReadMemory(0x000095) << 8);
            var inputs = phenome.InputVector;

            int currOffset = 0;
            inputs[currOffset++] = 1; //Bias
            inputs[currOffset++] = position; //Temporary

            for (int i = 0; i < INPUT_ROW_COUNT; i++)
            {
                for (int j = 0; j < INPUT_COLUMN_COUNT; j++)
                {
                    // TODO : read tile at position
                    inputs[currOffset++] = 0;
                }
            }

            for (int i = 0; i < INPUT_ROW_COUNT; i++)
            {
                for (int j = 0; j < INPUT_COLUMN_COUNT; j++)
                {
                    //TODO : check if enemy at position
                    inputs[currOffset++] = 0;
                }
            }

            phenome.Activate();

            var outputs = phenome.OutputVector;
            string controllerInputs = "";
            for (int i = 0; i < phenome.OutputCount; i++)
            {
                if (outputs[i] > 0.5)
                {
                    controllerInputs += (char)Input.IndexToButton(i);
                }
            }

            Input controllerInput = new(controllerInputs);

            emulator.SendInput(controllerInput);
            emulator.NextFrame();


            return position;
        }
    }
}
