using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.SuperBomberman3.Configuration;
using Retro_ML.SuperBomberman3.Game;
using Retro_ML.SuperBomberman3.Neural.Train;
using Retro_ML.SuperBomberMan3.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperBomberman3;

public class SB3Plugin : IGamePlugin
{
    public string PluginName => "SB3";
    public string PluginGameName => "Super Bomberman 3";
    public string PluginROMHeaderName => "";
    public string ConsolePluginName => "SNES";
    public string PluginConfigPath => "config/plugins/sb3-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new SB3DataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SB3Trainer(emulatorManager, appConfig);
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, object phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) => new SB3Evaluator(appConfig, (IBlackBox<double>)phenome, saveStates, emulator);
    public IPluginConfig GetPluginConfig() => new SB3PluginConfig();
}
