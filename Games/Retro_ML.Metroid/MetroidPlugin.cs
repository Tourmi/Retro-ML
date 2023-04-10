using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Metroid.Game;
using Retro_ML.Metroid.Neural;
using Retro_ML.Metroid.Neural.Train;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;

namespace Retro_ML.Metroid;

internal class MetroidPlugin : IGamePlugin
{
    public string PluginGameName => "Metroid";
    public string PluginROMHeaderName => "";
    public string ConsolePluginName => "NES";
    public string PluginName => "Metroid";
    public string PluginConfigPath => "config/plugins/metroid-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new MetroidDataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new MetroidTrainer(emulatorManager, appConfig);
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) => new MetroidEvaluator(appConfig, phenome, saveStates, emulatorManager);
    public IPluginConfig GetPluginConfig() => new MetroidPluginConfig();
}
