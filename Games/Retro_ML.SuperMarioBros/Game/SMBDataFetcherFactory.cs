using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioBros.Configuration;

namespace Retro_ML.SuperMarioBros.Game
{
    public class SMBDataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new SMBDataFetcher(emulator, config.NeuralConfig, (SMBPluginConfig)config.GamePluginConfig!);
        }
    }
}
