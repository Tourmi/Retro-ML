using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Play;
using Retro_ML.PokemonGen1.Configuration;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;

namespace Retro_ML.PokemonGen1.Neural.Play;

internal class PokemonPlayer : SharpNeatPlayer
{
    private readonly PokemonPluginConfig pluginConfig;

    public PokemonPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        pluginConfig = (PokemonPluginConfig)appConfig.GamePluginConfig!;
    }

    protected override void DoFrame(IBlackBox<double> blackBox)
    {
        blackBox!.ResetState();
        inputSetter.SetInputs(blackBox.InputVector);
        blackBox.Activate();

        emulator.SendInput(outputGetter.GetControllerInput(blackBox.OutputVector));

        //ToDO wait until next turn
        emulator.NextFrames(1, true);
        dataFetcher.NextFrame();
        emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(blackBox.InputVector), SharpNeatUtils.VectorToArray(blackBox.OutputVector));
    }
}
