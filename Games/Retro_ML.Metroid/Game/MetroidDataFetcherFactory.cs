using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Metroid.Configuration;

namespace Retro_ML.Metroid.Game;

public class MetroidDataFetcherFactory : IDataFetcherFactory
{
    public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
    {
        return new MetroidDataFetcher(emulator, config.NeuralConfig, (MetroidPluginConfig)config.GamePluginConfig!);
    }
}
