using Newtonsoft.Json;
using Retro_ML.Neural.Scoring;
using Retro_ML.Neural.Train.StopCondition;
using Retro_ML.Plugin;
using Retro_ML.Utils;

namespace Retro_ML.Configuration
{
    /// <summary>
    /// Genneric configuration for the application
    /// </summary>
    public class ApplicationConfig
    {
        public ApplicationConfig()
        {
            RomPath = "smw.sfc";
            gamePluginName = "SMW";
            ArduinoCommunicationPort = "COM3";
            SaveStates = new List<string>();
            StopConditions = new List<IStopCondition>() { new FitnessStopCondition(), new GenerationCountStopCondition(), new PlateauStopCondition(), new TimeStopCondition() };

            NeuralConfig = new NeuralConfig();
        }

        /// <summary>
        /// The path for the ROM file to use.
        /// </summary>
        public string RomPath { get; set; }
        /// <summary>
        /// The name of the current game plugin
        /// </summary>
        private string gamePluginName;
        public string GamePluginName
        {
            get => gamePluginName;
            set
            {
                gamePluginName = value;
                ResetGamePluginConfig();
            }
        }

        /// <summary>
        /// The amount of threads and emulator instances the application should use.
        /// </summary>
        public int Multithread { get; set; }
        /// <summary>
        /// The communication port to use to communnicate with an arduino
        /// </summary>
        public string ArduinoCommunicationPort { get; set; }
        /// <summary>
        /// The trainer's stop conditions
        /// </summary>
        [JsonProperty]
        public List<IStopCondition> StopConditions { get; set; }
        /// <summary>
        /// The save states to use for training.
        /// </summary>
        public List<string> SaveStates { get; set; }
        /// <summary>
        /// The application's Neural Configuration
        /// </summary>
        [JsonProperty]
        public NeuralConfig NeuralConfig { get; set; }
        [JsonIgnore]
        public IGamePluginConfig? GamePluginConfig { get; set; }

        /// <summary>
        /// Returns a clone of the configured score factors, should be used if the state of the score factors will be used.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IScoreFactor> GetScoreFactorClones()
        {
            foreach (var factor in GamePluginConfig!.ScoreFactors)
            {
                if (factor.IsDisabled) continue;

                yield return factor.Clone();
            }
        }

        /// <summary>
        /// Returns the game plugin associated with the configuration.
        /// </summary>
        public IGamePlugin GetGamePlugin() => PluginUtils.GetPlugin<IGamePlugin>(GamePluginName);
        /// <summary>
        /// Returns the console plugin associated with the current game plugin
        /// </summary>
        public IConsolePlugin GetConsolePlugin() => PluginUtils.GetPlugin<IConsolePlugin>(PluginUtils.GetPlugin<IGamePlugin>(GamePluginName).ConsolePluginName);

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_PASCAL_CASE_CONFIG);

        public static ApplicationConfig Deserialize(string json)
        {
            ApplicationConfig cfg = JsonConvert.DeserializeObject<ApplicationConfig>(json, SerializationUtils.JSON_PASCAL_CASE_CONFIG)!;
            cfg.ResetGamePluginConfig();

            return cfg;
        }

        private void ResetGamePluginConfig()
        {
            var gamePlugin = GetGamePlugin();
            GamePluginConfig = (IGamePluginConfig)gamePlugin.GetPluginConfig();
            if (File.Exists(gamePlugin.PluginConfigPath))
            {
                GamePluginConfig.Deserialize(File.ReadAllText(gamePlugin.PluginConfigPath));
            }
            GamePluginConfig.InitNeuralConfig(NeuralConfig);
        }
    }
}
