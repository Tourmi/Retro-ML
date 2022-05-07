using Retro_ML.Configuration;
using Retro_ML.Emulator;

namespace Retro_ML.Game
{
    public interface IDataFetcherFactory
    {
        IDataFetcher GetDataFetcher(ApplicationConfig config, IEmulatorAdapter emulator);
    }
}
