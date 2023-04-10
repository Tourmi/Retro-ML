using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.SuperMario64.Configuration;

namespace Retro_ML.SuperMario64.Neural;
internal class SM64Evaluator : BaseEvaluator
{
    public SM64Evaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) : base(appConfig, phenome, saveStates, emulatorManager) { }

    protected override int FrameSkip => ((SM64PluginConfig)gamePluginConfig).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => true;
}
