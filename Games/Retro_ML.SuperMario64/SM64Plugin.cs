using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.SuperMario64.Game;
using Retro_ML.SuperMario64.Neural.Play;
using Retro_ML.SuperMario64.Neural.Train;

namespace Retro_ML.SuperMario64;

internal class SM64Plugin : IGamePlugin
{
    public string PluginGameName => "Super Mario 64";
    public string PluginROMHeaderName => "";
    public string ConsolePluginName => "N64";
    public string PluginName => "SM64";
    public string PluginConfigPath => "config/plugins/sm64-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new SM64DataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SM64Player(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SM64Trainer(emulatorManager, appConfig);
    public IPluginConfig GetPluginConfig() => new SM64PluginConfig();
}
