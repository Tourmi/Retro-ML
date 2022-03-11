using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SMW_ML.Emulator;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Scoring;
using SMW_ML.Utils;
using SMW_ML.Utils.SharpNeat;
using System;
using System.IO;
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
        private INeuralTrainer trainer;
        private DataFetcher? dataFetcher;
        private InputSetter? inputSetter;
        private OutputGetter? outputGetter;
        private ApplicationConfig appConfig;

        public SMWPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, INeuralTrainer trainer)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            try
            {
                Score score = new(appConfig);

                emulator = emulatorManager.WaitOne();
                int[] outputMap = new int[phenome.OutputCount];
                Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);
                emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(phenome), outputMap);
                dataFetcher = emulator.GetDataFetcher();
                inputSetter = emulator.GetInputSetter();
                outputGetter = emulator.GetOutputGetter();

                var saveStates = appConfig.SaveStates;
                foreach (var state in saveStates)
                {
                    if (trainer.ForceStop)
                    {
                        break;
                    }
                    emulator.LoadState(Path.GetFullPath(state));
                    emulator.NextFrame();
                    dataFetcher.NextLevel();

                    while (!score.ShouldStop)
                    {
                        if (trainer.ForceStop)
                        {
                            break;
                        }
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
            catch (Exception ex)
            {
                Exceptions.QueueException(new Exception($"Error occured during training. Was an emulator closed?\n{ex.Message}\n{ex.StackTrace}"));

                return FitnessInfo.DefaultFitnessInfo;
            }
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
