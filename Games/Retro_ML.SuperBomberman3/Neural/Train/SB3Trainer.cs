using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using Retro_ML.SuperBomberman3.Neural.Train;

namespace Retro_ML.SuperBomberman3.Neural.Train
{
    internal class SB3Trainer : SharpNeatTrainer
    {
        public SB3Trainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            experimentFactory = new SB3ExperimentFactory(emulatorManager, appConfig, this);
        }
    }
}
