using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.SuperMario64.Neural.Train;

internal class SM64Trainer : SharpNeatTrainer
{
    public SM64Trainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        experimentFactory = new SM64ExperimentFactory(emulatorManager, appConfig, this);
    }
}
