using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training.SharpNeat
{
    internal class SMWPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        private const int INPUT_ROW_COUNT = 15;
        private const int INPUT_COLUMN_COUNT = 15;
        public const int INPUT_COUNT = INPUT_ROW_COUNT * INPUT_COLUMN_COUNT * 2 + 1; // 15x15 tile grid around Mario of solid tiles, as well as enemies. +1 for bias

        private const int FRAMES_TO_PROCESS = 360;

        public SMWPhenomeEvaluator()
        {
            // TODO : Receive emulator manager here
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            double score = 0;

            for (int i = 0; i < FRAMES_TO_PROCESS; i++)
            {
                score = DoFrame(phenome);
            }

            return new FitnessInfo(score);
        }

        private double DoFrame(IBlackBox<double> phenome)
        {
            phenome.ResetState();

            var inputs = phenome.InputVector;

            int currOffset = 0;
            inputs[currOffset++] = 1; //Bias

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
                if (outputs[i] >= 0)
                {
                    controllerInputs += (char)Input.IndexToButton(i);
                }
            }

            Input controllerInput = new(controllerInputs);

            //TODO : Pass ControllerInput to emulator
            //TODO : emulator next frame

            return 0;
        }
    }
}
