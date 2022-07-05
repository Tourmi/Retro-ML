using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.SuperMarioBros.Configuration;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML.SuperMarioBros.Neural;
using Retro_ML.SuperMarioBros.Neural.Train;
using SharpNeat.BlackBox;

namespace Retro_ML.SuperMarioBros;

public class SMBPlugin : IGamePlugin
{
    public string PluginName => "SMB";
    public string PluginGameName => "Super Mario Bros";
    public string PluginROMHeaderName => "";
    public string ConsolePluginName => "NES";
    public string PluginConfigPath => "config/plugins/smb-config.json";

    public IDataFetcherFactory GetDataFetcherFactory() => new SMBDataFetcherFactory();
    public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);
    public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SMBTrainer(emulatorManager, appConfig);
    public IEvaluator GetEvaluator(ApplicationConfig appConfig, object phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) => new SMBEvaluator(appConfig, (IBlackBox<double>)phenome, saveStates, emulator);
    public IPluginConfig GetPluginConfig() => new SMBPluginConfig();
}
