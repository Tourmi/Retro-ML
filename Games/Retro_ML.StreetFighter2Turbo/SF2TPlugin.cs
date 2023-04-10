using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.StreetFighter2Turbo.Configuration;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.StreetFighter2Turbo.Neural;
using Retro_ML.StreetFighter2Turbo.Neural.Train;

namespace Retro_ML.StreetFighter2Turbo;

public class SF2TPlugin : IGamePlugin
{
    public string PluginName => "SF2T";
    public string PluginGameName => "Street Fighter 2 Turbo";
    public string PluginROMHeaderName => "";
    public string ConsolePluginName => "SNES";
    public string PluginConfigPath => "config/plugins/sf2t-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new SF2TDataFetcherFactory();
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) => new SF2TEvaluator(appConfig, phenome, saveStates, emulatorManager);
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SF2TTrainer(emulatorManager, appConfig);
    public IPluginConfig GetPluginConfig() => new SF2TPluginConfig();
}
