using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.SuperMarioBros.Neural.Train;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class SMBPhenomeEvaluator : DefaultPhenomeEvaluator
{
    public SMBPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, SMBTrainer trainer) : base(emulatorManager, appConfig, trainer) { }
}
