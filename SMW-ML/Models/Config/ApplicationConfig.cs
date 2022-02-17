using Newtonsoft.Json;
using SMW_ML.Neural.Scoring;
using System.Collections.Generic;

namespace SMW_ML.Models.Config
{
    public class ApplicationConfig
    {
        public ApplicationConfig()
        {
            StopTrainingCondition = "";
            ArduinoCommunicationPort = "COM3";
            ScoreFactors = new List<IScoreFactor>()
            {
                new DiedScoreFactor() { ScoreMultiplier = -10 },
                new DistanceScoreFactor() { ScoreMultiplier = 3 },
                new StopMovingScoreFactor() { ScoreMultiplier = -10 },
                new TimeTakenScoreFactor() { ScoreMultiplier = 1 },
                new WonLevelScoreFactor() { ScoreMultiplier = 100 },
                new CoinsScoreFactor() { ScoreMultiplier = 5 },
                new YoshiCoinsScoreFactor() { ScoreMultiplier = 15 },
                new OneUpsScoreFactor() { ScoreMultiplier = 30 },
                new HighScoreScoreFactor() { ScoreMultiplier = 1 }
            };
            SaveStates = new List<string>();

            NeuralConfig = new NeuralConfig();
        }

        public int Multithread { get; set; }
        public string ArduinoCommunicationPort { get; set; }
        public string StopTrainingCondition { get; set; }
        public int? StopTrainingConditionValue { get; set; }
        public List<IScoreFactor> ScoreFactors { get; set; }
        public List<string> SaveStates { get; set; }
        [JsonProperty]
        internal NeuralConfig NeuralConfig { get; set; }

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
