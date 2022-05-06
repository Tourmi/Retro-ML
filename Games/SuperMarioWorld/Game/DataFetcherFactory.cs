using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioWorld.Configuration;

namespace Retro_ML.SuperMarioWorld.Game
{
    public class DataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new DataFetcher(emulator, (SMWNeuralConfig)config.NeuralConfig);
        }
    }
}
