using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.Tetris.Neural.Train
{
    internal class TetrisTrainer : SharpNeatTrainer
    {
        public TetrisTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
        {
            experimentFactory = new TetrisExperimentFactory(emulatorManager, appConfig, this);
        }
    }
}
