using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.SuperMarioWorld.Configuration;
using Retro_ML.SuperMarioWorld.Game;
using Retro_ML.SuperMarioWorld.Neural;
using Retro_ML.SuperMarioWorld.Neural.Train;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperMarioWorld;

public class SMWPlugin : IGamePlugin
{
    public string PluginName => "SMW";
    public string PluginGameName => "Super Mario World";
    public string PluginROMHeaderName => "SUPER MARIOWORLD";
    public string ConsolePluginName => "SNES";
    public string PluginConfigPath => "config/plugins/smw-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new SMWDataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SMWTrainer(emulatorManager, appConfig);
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, object phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) => new SMWEvaluator(appConfig, (IBlackBox<double>)phenome, saveStates, emulator);
    public IPluginConfig GetPluginConfig() => new SMWPluginConfig();
}
