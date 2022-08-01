using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.SuperMario64.Game;
using Retro_ML.SuperMario64.Neural;
using Retro_ML.SuperMario64.Neural.Train;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperMario64;

internal class SM64Plugin : IGamePlugin
{
    public string PluginGameName => "Super Mario 64";
    public string PluginROMHeaderName => "SUPER MARIO 64     ";
    public string ConsolePluginName => "N64";
    public string PluginName => "SM64";
    public string PluginConfigPath => "config/plugins/sm64-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new SM64DataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SM64Trainer(emulatorManager, appConfig);
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, object phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) => new SM64Evaluator(appConfig, (IBlackBox<double>)phenome, saveStates, emulator);
    public IPluginConfig GetPluginConfig() => new SM64PluginConfig();
}
