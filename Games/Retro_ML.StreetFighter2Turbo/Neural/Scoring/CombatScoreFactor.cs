using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class CombatScoreFactor : IScoreFactor
    {
        private double currScore;
        private double currTimer;
        private double hpDelta;

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new DoubleFieldInfo(nameof(TimerInfluence), "Timer Influence", 0, 1, 0.01, "Percentage of the score attributed by the end round timer"),
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(TimerInfluence) => TimerInfluence,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(TimerInfluence): TimerInfluence = (double)value; break;
                }
            }
        }

        public double TimerInfluence { get; set; } = 0.5;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Combat";

        public string Tooltip => "Reward applied to the AI after a round is finished that represent its efficiency in combat based on HP delta between the player and the enemy and the time taken for combat";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SF2TDataFetcher)dataFetcher);
        }

        private void Update(SF2TDataFetcher dataFetcher)
        {
            currTimer = dataFetcher.GetRoundTimerNormalized();
            hpDelta = dataFetcher.GetPlayer1HpNormalized() - dataFetcher.GetPlayer2HpNormalized();
        }

        public void LevelDone()
        {
            currScore += (currTimer * TimerInfluence + (1.0 - TimerInfluence)) * ScoreMultiplier * hpDelta;
        }

        public IScoreFactor Clone() => new CombatScoreFactor()
        {
            ScoreMultiplier = ScoreMultiplier,
            IsDisabled = IsDisabled,
            TimerInfluence = TimerInfluence,
        };
    }
}
