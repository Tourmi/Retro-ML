using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Play;
using Retro_ML.SuperMarioKart.Configuration;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperMarioKart.Neural.Play
{
    internal class SMKPlayer : SharpNeatPlayer
    {
        private readonly SMKPluginConfig pluginConfig;

        public SMKPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            pluginConfig = (SMKPluginConfig)appConfig.GamePluginConfig!;

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
