using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.StreetFighter2Turbo.Configuration;

namespace Retro_ML.StreetFighter2Turbo.Neural;
internal class SF2TEvaluator : BaseEvaluator
{
    public SF2TEvaluator(ApplicationConfig appConfig,
                                  IPhenomeWrapper phenome,
                                  IEnumerable<string> saveStates,
                                  EmulatorManager emulatorManager) : base(appConfig, phenome, saveStates, emulatorManager) { }

    protected override int FrameSkip => ((SF2TPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
