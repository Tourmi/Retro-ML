using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.Metroid.Neural;
internal class MetroidEvaluator : DefaultEvaluator
{
    public MetroidEvaluator(ApplicationConfig appConfig,
                                  IBlackBox<double> phenome,
                                  IEnumerable<string> saveStates,
                                  IEmulatorAdapter emulator) : base(appConfig, phenome, saveStates, emulator) { }

    protected override int FrameSkip => ((MetroidPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
