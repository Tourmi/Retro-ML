using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.SuperMarioKart.Neural.Train
{
    internal class SMKTrainer : SharpNeatTrainer
    {
        public SMKTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            experimentFactory = new SMKExperimentFactory(emulatorManager, appConfig, this);
        }
    }
}
