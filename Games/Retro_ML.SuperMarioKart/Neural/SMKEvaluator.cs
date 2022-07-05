using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.SuperMarioKart.Configuration;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperMarioKart.Neural;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class SMKEvaluator : DefaultEvaluator
{
    public SMKEvaluator(ApplicationConfig appConfig, IBlackBox<double> phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) : base(appConfig, phenome, saveStates, emulator) { }

    protected override int FrameSkip => ((SMKPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
