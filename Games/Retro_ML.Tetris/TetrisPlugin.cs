using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.Tetris.Configuration;
using Retro_ML.Tetris.Game;
using Retro_ML.Tetris.Neural;
using Retro_ML.Tetris.Neural.Train;
using SharpNeat.BlackBox;

namespace Retro_ML.Tetris;

public class TetrisPlugin : IGamePlugin
{
    public string PluginName => "Tetris";
    public string PluginGameName => "Tetris";
    public string PluginROMHeaderName => "TETRIS";
    public string ConsolePluginName => "GameBoy";
    public string PluginConfigPath => "config/plugins/tetris-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new TetrisDataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new TetrisTrainer(emulatorManager, appConfig);
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, object phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) => new TetrisEvaluator(appConfig, (IBlackBox<double>)phenome, saveStates, emulator);
    public IPluginConfig GetPluginConfig() => new TetrisPluginConfig();
}
