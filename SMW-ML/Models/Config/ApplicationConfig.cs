using Newtonsoft.Json;
using SMW_ML.Neural.Scoring;
using System.Collections.Generic;

namespace SMW_ML.Models.Config
{
    /// <summary>
    /// Genneric configuration for the application
    /// </summary>
    public class ApplicationConfig
    {
        public ApplicationConfig()
        {
            StopTrainingCondition = "";
            ArduinoCommunicationPort = "COM3";
            ScoreFactors = new List<IScoreFactor>();
            SaveStates = new List<string>();

            NeuralConfig = new NeuralConfig();
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
        /// Unimplemented
        /// </summary>
        public string StopTrainingCondition { get; set; }
        /// <summary>
        /// Unimplemented
        /// </summary>
        public int? StopTrainingConditionValue { get; set; }
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
        internal NeuralConfig NeuralConfig { get; set; }

        /// <summary>
        /// Returns a clone of the configured score factors, should be used if the state of the score factors will be used.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IScoreFactor> GetScoreFactorClones()
        {
            foreach (var factor in ScoreFactors)
            {
                if (factor.IsDisabled) continue;

                yield return (IScoreFactor)factor.Clone();
            }
        }
    }
}
