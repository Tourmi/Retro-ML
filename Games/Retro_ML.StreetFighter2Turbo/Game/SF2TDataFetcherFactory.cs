using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.StreetFighter2Turbo.Configuration;

namespace Retro_ML.StreetFighter2Turbo.Game
{
    public class SF2TDataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new SF2TDataFetcher(emulator, config.NeuralConfig, (SF2TPluginConfig)config.GamePluginConfig!);
        }
    }
}
