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
            RomHeaderName = "SUPER MARIOWORLD";
            ArduinoCommunicationPort = "COM3";
            ScoreFactors = new List<IScoreFactor>();
            SaveStates = new List<string>();
            StopConditions = new List<IStopCondition>() { new FitnessStopCondition(), new GenerationCountStopCondition(), new PlateauStopCondition(), new TimeStopCondition() };

            NeuralConfig = new NeuralConfig();
        }

        /// <summary>
        /// The path for the ROM file to use.
        /// </summary>
        public string RomPath { get; set; }
        /// <summary>
        /// The name of the game in the ROM's header file
        /// </summary>
        public string RomHeaderName { get; set; }

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
        /// The score factors used to determine the score of the training.
        /// </summary>
        public List<IScoreFactor> ScoreFactors { get; set; }
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
        public IPluginConfig? PluginConfig { get; set; }

        /// <summary>
        /// Returns a clone of the configured score factors, should be used if the state of the score factors will be used.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IScoreFactor> GetScoreFactorClones()
        {
            foreach (var factor in ScoreFactors)
            {
                if (factor.IsDisabled) continue;

                yield return factor.Clone();
            }
        }

        /// <summary>
        /// Returns the game plugin associated with the configuration.
        /// </summary>
        /// <returns></returns>
        public IGamePlugin GetGamePlugin() => PluginUtils.GetGamePlugin(RomHeaderName);

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_PASCAL_CASE_CONFIG);

        public static ApplicationConfig Deserialize(string json)
        {
            ApplicationConfig cfg = JsonConvert.DeserializeObject<ApplicationConfig>(json, SerializationUtils.JSON_PASCAL_CASE_CONFIG)!;
            var gamePlugin = cfg.GetGamePlugin();
            cfg.PluginConfig = gamePlugin.GetPluginConfig();
            if (File.Exists(gamePlugin.PluginConfigPath))
            {
                cfg.PluginConfig.Deserialize(File.ReadAllText(gamePlugin.PluginConfigPath));
            }
            cfg.PluginConfig.InitNeuralConfig(cfg.NeuralConfig);

            return cfg;
        }
    }
}
