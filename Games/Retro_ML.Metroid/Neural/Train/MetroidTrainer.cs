using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.Metroid.Neural.Train;

internal class MetroidTrainer : SharpNeatTrainer
{
    public MetroidTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        experimentFactory = new MetroidExperimentFactory(emulatorManager, appConfig, this);
    }
}
