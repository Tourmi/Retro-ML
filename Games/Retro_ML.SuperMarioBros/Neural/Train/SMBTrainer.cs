using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using Retro_ML.SuperMarioBros.Neural.Train;

namespace Retro_ML.SuperMarioBros.Neural.Train
{
    internal class SMBTrainer : SharpNeatTrainer
    {
        public SMBTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            experimentFactory = new SMBExperimentFactory(emulatorManager, appConfig, this);
        }
    }
}
