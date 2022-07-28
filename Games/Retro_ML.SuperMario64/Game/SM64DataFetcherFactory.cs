using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMario64.Configuration;

namespace Retro_ML.SuperMario64.Game;

public class SM64DataFetcherFactory : IDataFetcherFactory
{
    public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        => new SM64DataFetcher(emulator, config.NeuralConfig, (SM64PluginConfig)config.GamePluginConfig!);
}
