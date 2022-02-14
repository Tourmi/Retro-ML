using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SharpNeat.Graphs;
using SharpNeat.Graphs.Acyclic;
using SMW_ML.Emulator;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Scoring;
using SMW_ML.Utils.SharpNeat;
using System;
using static SMW_ML.Utils.ReflectionTool;

namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    /// <summary>
    /// This class takes care of the evaluation of a single AI.
    /// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
    /// </summary>
    internal class SMWPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        private readonly EmulatorManager emulatorManager;
        private IEmulatorAdapter? emulator;
        private DataFetcher? dataFetcher;
        private InputSetter? inputSetter;
        private OutputGetter? outputGetter;
        private ApplicationConfig appConfig;

        public SMWPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            Score score = new(appConfig);

            emulator = emulatorManager.WaitOne();
            int[] outputMap = new int[phenome.OutputCount];
            Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);
            emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(phenome), outputMap);
            dataFetcher = emulator.GetDataFetcher();
            inputSetter = emulator.GetInputSetter();
            outputGetter = emulator.GetOutputGetter();

            var saveStates = emulator.GetStates();
            foreach (var state in saveStates)
            {
                if (!state.Contains("yoshi-island-1")) continue;
                emulator.LoadState(state);
                emulator.NextFrame();
                dataFetcher.NextLevel();

                while (!score.ShouldStop)
                {
                    DoFrame(phenome);

                    score.Update(dataFetcher);
                    dataFetcher.NextFrame();
                    emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(phenome.InputVector), SharpNeatUtils.VectorToArray(phenome.OutputVector));
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
