using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.SuperMarioKart.Configuration;
using Retro_ML.SuperMarioKart.Game;
using Retro_ML.SuperMarioKart.Neural.Scoring;
using Retro_ML.SuperMarioKart.Neural.Train;

namespace Retro_ML.SuperMarioKart
{
    public class SMKPlugin : IGamePlugin
    {
        public string PluginName => "SMK";
        public string PluginGameName => "Super Mario Kart";
        public string PluginROMHeaderName => "SUPER MARIO KART";
        public string ConsolePluginName => "SNES";
        public string PluginConfigPath => "config/plugins/smk-config.json";

        public IDataFetcherFactory GetDataFetcherFactory() => new SMKDataFetcherFactory();
        public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig, new FinishedRaceScoreFactor());
        public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SMKTrainer(emulatorManager, appConfig);
        public IPluginConfig GetPluginConfig() => new SMKPluginConfig();
    }
}