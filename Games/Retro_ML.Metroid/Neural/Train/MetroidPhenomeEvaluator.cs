using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Neural.Train;

namespace Retro_ML.Metroid.Neural.Train;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class MetroidPhenomeEvaluator : DefaultPhenomeEvaluator
{
    public MetroidPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, MetroidTrainer trainer) : base(emulatorManager, appConfig, trainer) { }

    protected override int FrameSkip => ((MetroidPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
