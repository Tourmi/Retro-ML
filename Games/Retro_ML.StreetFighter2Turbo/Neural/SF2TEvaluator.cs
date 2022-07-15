using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.StreetFighter2Turbo.Configuration;
using Retro_ML.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.StreetFighter2Turbo.Neural;
internal class SF2TEvaluator : DefaultEvaluator
{
    public SF2TEvaluator(ApplicationConfig appConfig,
                                  IBlackBox<double> phenome,
                                  IEnumerable<string> saveStates,
                                  IEmulatorAdapter emulator) : base(appConfig, phenome, saveStates, emulator) { }

    protected override int FrameSkip => ((SF2TPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
