using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Neural.Train;
using Retro_ML.Tetris.Game;
//using Retro_ML.Tetris.Game;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.Tetris.Neural.Train
{
    /// <summary>
    /// This class takes care of the evaluation of a single AI.
    /// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
    /// </summary>
    internal class TetrisPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        private readonly EmulatorManager emulatorManager;
        private IEmulatorAdapter? emulator;
        private INeuralTrainer trainer;
        private TetrisDataFetcher? dataFetcher;
        private ApplicationConfig appConfig;
        private InputSetter? inputSetter;
        private OutputGetter? outputGetter;

        public TetrisPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, TetrisTrainer trainer)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            try
            {
                Score score = new(appConfig.GetScoreFactorClones());

                emulator = emulatorManager.WaitOne();
                int[] outputMap = new int[phenome.OutputCount];
                Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);
                emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(phenome), outputMap);
                dataFetcher = (TetrisDataFetcher)emulator.GetDataFetcher();
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
                    WaitThenStart();
                    emulator.NextFrame();
                    dataFetcher.NextState();

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

        private void WaitThenStart()
        {
            for (int i = Random.Shared.Next(120); i > 0; i--)
            {
                emulator!.NextFrame();
            }

            var input = appConfig.GetConsolePlugin().GetInput();
            input.FromString("S");
            emulator!.SendInput(input);

            for(int i = 0; i < 8; i++)
            {
                emulator!.NextFrame();
            }
        }
    }
}
