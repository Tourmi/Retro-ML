using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SMW_ML.Emulator;
using SMW_ML.Game;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Neural.Scoring;
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
        private readonly IEmulatorAdapter emulator;
        private readonly DataGetter dataReader;
        private readonly InputSetter inputSetter;
        private readonly OutputGetter outputGetter;

        public SMWPhenomeEvaluator(IEmulatorAdapter emulator, DataGetter dataReader, InputSetter inputSetter, OutputGetter outputGetter)
        {
            this.emulator = emulator;
            this.dataReader = dataReader;
            this.inputSetter = inputSetter;
            this.outputGetter = outputGetter;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            Score score = new();

            emulator.Reserve();
            var saveStates = emulator.GetStates();
            foreach (var state in saveStates)
            {
                emulator.LoadState(state);
                emulator.NextFrame();
                dataReader.NextLevel();

                while (!score.ShouldStop)
                {
                    DoFrame(phenome);

                    score.Update(dataReader);
                    dataReader.NextFrame();
                }
                score.LevelDone();
            }
            emulator.Free();

            return new FitnessInfo(score.GetScore());
        }

        /// <summary>
        /// Runs a single frame on the emulator, using the given AI
        /// </summary>
        /// <param name="phenome"></param>
        private void DoFrame(IBlackBox<double> phenome)
        {
            phenome.ResetState();
            inputSetter.SetInputs(phenome.InputVector);

            phenome.Activate();

            emulator.SendInput(outputGetter.GetControllerInput(phenome.OutputVector));
            emulator.NextFrame();
        }
    }
}
