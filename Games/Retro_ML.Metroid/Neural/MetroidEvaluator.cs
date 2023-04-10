using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Neural;

namespace Retro_ML.Metroid.Neural;
internal class MetroidEvaluator : BaseEvaluator
{
    public MetroidEvaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) : base(appConfig, phenome, saveStates, emulatorManager) { }

    protected override int FrameSkip => ((MetroidPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
