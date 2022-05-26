using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Neural.Play;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;

namespace Retro_ML.Metroid.Neural.Play
{
    internal class MetroidPlayer : SharpNeatPlayer
    {
        private readonly MetroidPluginConfig pluginConfig;

        public MetroidPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            pluginConfig = (MetroidPluginConfig)appConfig.GamePluginConfig!;

        }

        protected override void DoFrame(IBlackBox<double> blackBox)
        {
            blackBox!.ResetState();
            inputSetter.SetInputs(blackBox.InputVector);
            blackBox.Activate();

            emulator.SendInput(outputGetter.GetControllerInput(blackBox.OutputVector));

            emulator.NextFrames(pluginConfig.FrameSkip + 1, true);
            dataFetcher.NextFrame();
            emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(blackBox.InputVector), SharpNeatUtils.VectorToArray(blackBox.OutputVector));
        }
    }
}
