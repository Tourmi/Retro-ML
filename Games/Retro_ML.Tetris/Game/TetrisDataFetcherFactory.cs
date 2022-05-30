using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Tetris.Configuration;

namespace Retro_ML.Tetris.Game
{
    public class TetrisDataFetcherFactory : IDataFetcherFactory
    {
        public IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator)
        {
            return new TetrisDataFetcher(emulator, config.NeuralConfig, (TetrisPluginConfig)config.GamePluginConfig!);
        }
    }
}
