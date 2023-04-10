using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.PokemonGen1.Configuration;
using Retro_ML.PokemonGen1.Game;
using Retro_ML.PokemonGen1.Neural;
using Retro_ML.PokemonGen1.Neural.Train;

namespace Retro_ML.PokemonGen1;

internal class PokemonPlugin : IGamePlugin
{
    public string PluginGameName => "Pokemon Blue/Red/Yellow";
    public string PluginROMHeaderName => "";
    public string ConsolePluginName => "GameBoy";
    public string PluginName => "PokemonGen1";
    public string PluginConfigPath => "config/plugins/pokemon-gen1-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new PokemonDataFetcherFactory();
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) => new PokemonEvaluator(appConfig, phenome, saveStates, emulatorManager);
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new PokemonTrainer(emulatorManager, appConfig);
    public IPluginConfig GetPluginConfig() => new PokemonPluginConfig();
}
