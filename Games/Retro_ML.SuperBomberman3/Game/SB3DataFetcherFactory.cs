using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperBomberman3.Configuration;

namespace Retro_ML.SuperBomberman3.Game
{
    public class SB3DataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new SB3DataFetcher(emulator, config.NeuralConfig, (SB3PluginConfig)config.GamePluginConfig!);
        }
    }
}
