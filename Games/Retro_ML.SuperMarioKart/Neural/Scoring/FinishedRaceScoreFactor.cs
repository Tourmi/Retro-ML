using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class FinishedRaceScoreFactor : IScoreFactor
    {
        public const string FINAL_RANKING = "Ranking multiplier";

        private double currScore;

        public string Name => "Finished race";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        private bool shouldStop = false;
        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(RankingMult), "Ranking multiplier", double.MinValue, double.MaxValue, 0.25),
        };

        public FinishedRaceScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(FINAL_RANKING, 0.5)
            };
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(RankingMult) => RankingMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(RankingMult): RankingMult = (double)value; break;
                }
            }
        }


        public double RankingMult { get; set; } = 0.5;       

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            shouldStop = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            var df = (SMKDataFetcher)dataFetcher;
            if (df.GetCurrentLap() >= 5)
            {
                shouldStop = true;
                double rankingMultiplier = 0;
                if (df.IsRace())
                {
                    rankingMultiplier = (8 - df.GetRanking()) * ExtraField.GetValue(ExtraFields, FINAL_RANKING);
                }
                currScore += ScoreMultiplier * (1 + rankingMultiplier);
            }
        }

        public IScoreFactor Clone()
        {
            return new FinishedRaceScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
