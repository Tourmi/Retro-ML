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

namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    internal class SMWPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        private readonly EmulatorManager emulatorManager;
        private IEmulatorAdapter? emulator;
        private DataFetcher? dataFetcher;
        private InputSetter? inputSetter;
        private OutputGetter? outputGetter;

        public SMWPhenomeEvaluator(EmulatorManager emulatorManager)
        {
            this.emulatorManager = emulatorManager;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            Score score = new();

            emulator = emulatorManager.WaitOne();
            dataFetcher = emulator.GetDataFetcher();
            inputSetter = emulator.GetInputSetter();
            outputGetter = emulator.GetOutputGetter();

            var saveStates = emulator.GetStates();
            foreach (var state in saveStates)
            {
                if (!state.Contains("yoshi-island")) continue;
                emulator.LoadState(state);
                emulator.NextFrame();
                dataFetcher.NextLevel();

                while (!score.ShouldStop)
                {
                    DoFrame(phenome);

                    score.Update(dataFetcher);
                    dataFetcher.NextFrame();
                    emulator.NetworkUpdated(phenome);
                }
                score.LevelDone();
            }

            dataFetcher = null;
            inputSetter = null;
            outputGetter = null;
            emulatorManager.FreeOne(emulator);
            emulator = null;

            return new FitnessInfo(score.GetFinalScore());
        }

        /// <summary>
        /// Runs a single frame on the emulator, using the given AI
        /// </summary>
        /// <param name="phenome"></param>
        private void DoFrame(IBlackBox<double> phenome)
        {
            phenome.ResetState();
            inputSetter!.SetInputs(phenome.InputVector);

            phenome.Activate();

            emulator!.SendInput(outputGetter!.GetControllerInput(phenome.OutputVector));
            emulator!.NextFrame();
        }
    }
}
