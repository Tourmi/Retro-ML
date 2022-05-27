using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace Retro_ML.Tetris.Neural.Play
{
    public class TetrisPlayer : SharpNeatPlayer
    {
        private readonly ApplicationConfig appConfig;

        public TetrisPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig):base(emulatorManager, appConfig)
        {
           this.appConfig = appConfig;
        }

        protected override void DoSaveState(string saveState, IBlackBox<double> blackBox)
        {
            emulator.LoadState(saveState);
            WaitThenStart();
            emulator.NextFrame();
            dataFetcher.NextState();

            Score score = new(scoreFactors.Select(s => s.Clone()));

            while (!shouldStop && !score.ShouldStop)
            {
                DoFrame(blackBox);
                score.Update(dataFetcher);
            }
        }

        protected override void DoFrame(IBlackBox<double> blackBox)
        {
            blackBox!.ResetState();
            inputSetter.SetInputs(blackBox.InputVector);
            blackBox.Activate();

            emulator.SendInput(outputGetter.GetControllerInput(blackBox.OutputVector));

            emulator.NextFrame();
            emulator.NextFrame();
            dataFetcher.NextFrame();
            emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(blackBox.InputVector), SharpNeatUtils.VectorToArray(blackBox.OutputVector));
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

            for (int i = 0; i < 8; i++)
            {
                emulator!.NextFrame();
            }
        }
    }
}
