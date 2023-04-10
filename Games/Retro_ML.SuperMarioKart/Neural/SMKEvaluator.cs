using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.SuperMarioKart.Configuration;

namespace Retro_ML.SuperMarioKart.Neural;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class SMKEvaluator : BaseEvaluator
{
    public SMKEvaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) : base(appConfig, phenome, saveStates, emulatorManager) { }

    protected override int FrameSkip => ((SMKPluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
