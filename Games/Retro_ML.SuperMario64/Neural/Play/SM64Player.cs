using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Play;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperMario64.Neural.Play;

internal class SM64Player : SharpNeatPlayer
{
    private readonly SM64PluginConfig pluginConfig;

    public SM64Player(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        pluginConfig = (SM64PluginConfig)appConfig.GamePluginConfig!;
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
