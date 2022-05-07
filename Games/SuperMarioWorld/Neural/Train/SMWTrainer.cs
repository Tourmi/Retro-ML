using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.SuperMarioWorld.Neural.Train
{
    internal class SMWTrainer : SharpNeatTrainer
    {
        public SMWTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            experimentFactory = new SMWExperimentFactory(emulatorManager, appConfig, this);
        }
    }
}
