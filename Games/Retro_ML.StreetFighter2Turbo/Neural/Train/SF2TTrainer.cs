using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using Retro_ML.StreetFighter2Turbo.Neural.Train;

namespace Retro_ML.StreetFighter2Turbo.Neural.Train
{
    internal class SF2TTrainer : SharpNeatTrainer
    {
        public SF2TTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            experimentFactory = new SF2TExperimentFactory(emulatorManager, appConfig, this);
        }
    }
}
