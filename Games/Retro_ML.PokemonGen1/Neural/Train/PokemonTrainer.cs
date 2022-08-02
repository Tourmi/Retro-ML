using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;

namespace Retro_ML.PokemonGen1.Neural.Train;

internal class PokemonTrainer : SharpNeatTrainer
{
    public PokemonTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        experimentFactory = new PokemonExperimentFactory(emulatorManager, appConfig, this);
    }
}
