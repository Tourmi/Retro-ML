using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioKart.Configuration;

namespace Retro_ML.SuperMarioKart.Game
{
    public class SMKDataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new SMKDataFetcher(emulator, config.NeuralConfig, (SMKPluginConfig)config.GamePluginConfig!);
        }
    }
}
