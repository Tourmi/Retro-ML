using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.SuperBomberman3.Game;

namespace Retro_ML.SuperBomberMan3.Neural.Scoring
{
    internal class EndRoundScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool shouldStop = false;

        public FieldInfo[] Fields => new FieldInfo[]
       {
            new DoubleFieldInfo(nameof(DefeatMultiplier), "Defeat Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier in case of a defeat"),
            new DoubleFieldInfo(nameof(VictoryMultiplier), "Victory Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier in case of a victory"),
            new DoubleFieldInfo(nameof(DrawMultiplier), "Draw Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier in case of a draw"),
       };

        public EndRoundScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(DefeatMultiplier) => DefeatMultiplier,
                    nameof(VictoryMultiplier) => VictoryMultiplier,
                    nameof(DrawMultiplier) => DrawMultiplier,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(DefeatMultiplier): DefeatMultiplier = (double)value; break;
                    case nameof(VictoryMultiplier): VictoryMultiplier = (double)value; break;
                    case nameof(DrawMultiplier): DrawMultiplier = (double)value; break;
                }
            }
        }

        public string Tooltip => "The reward to give an AI at the end of the round depending on the result.";
        public string Name => "End of Round";
        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }
        public double VictoryMultiplier { get; set; } = 1;
        public double DefeatMultiplier { get; set; } = -0.1;
        public double DrawMultiplier { get; set; } = 0.1;

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SB3DataFetcher)dataFetcher);
        }

        private void Update(SB3DataFetcher dataFetcher)
        {
            //If the player lost the round
            if (dataFetcher.IsPlayerDead())
            {
                currScore += ScoreMultiplier * DefeatMultiplier;
                shouldStop = true;
            }

            //If the player won the round
            if (dataFetcher.GetNumbersOfPlayersAlive() == 1 && !dataFetcher.IsPlayerDead())
            {
                currScore += ScoreMultiplier * VictoryMultiplier;
                shouldStop = true;
            }

            //If the round is a draw
            if (dataFetcher.GetNumbersOfPlayersAlive() == 0)
            {
                currScore += ScoreMultiplier * DrawMultiplier;
                shouldStop = true;
            }

        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new EndRoundScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                DefeatMultiplier = DefeatMultiplier,
                VictoryMultiplier = VictoryMultiplier,
                DrawMultiplier = DrawMultiplier
            };
        }
    }
}
