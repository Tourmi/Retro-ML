using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.PokemonGen1.Configuration;

namespace Retro_ML.PokemonGen1.Game;

public class PokemonDataFetcherFactory : IDataFetcherFactory
{
    public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
    {
        return new PokemonDataFetcher(emulator, config.NeuralConfig, (PokemonPluginConfig)config.GamePluginConfig!);
    }
}
