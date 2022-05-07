using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioWorld.Configuration;

namespace Retro_ML.SuperMarioWorld.Game
{
    public class SMWDataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new SMWDataFetcher(emulator, config.NeuralConfig, (SMWPluginConfig)config.PluginConfig);
        }
    }
}
