using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class WonLevelScoreFactor : IScoreFactor
    {
        private const string GOAL_MULT = "Goal Mult";
        private const string KEY_MULT = "Key Mult";

        private bool shouldStop = false;
        private double currScore;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(GoalMult), "Goal Multiplier", 0, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(KeyMult), "Key Multiplier", 0, double.MaxValue, 0.25),
        };

        public WonLevelScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(GOAL_MULT, 1.0),
                new ExtraField(KEY_MULT, 1.0),
            };
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(GoalMult) => GoalMult,
                    nameof(KeyMult) => KeyMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(GoalMult): GoalMult = (double)value; break;
                    case nameof(KeyMult): KeyMult = (double)value; break;
                }
            }
        }

        public double GoalMult { get; set; } = 1.0;
        public double KeyMult { get; set; } = 1.0;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Won level";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (dataFetcher.WonLevel())
            {
                shouldStop = true;
                currScore += ScoreMultiplier * (dataFetcher.WonViaGoal() ? ExtraField.GetValue(ExtraFields, GOAL_MULT) : ExtraField.GetValue(ExtraFields, KEY_MULT));
                return;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new WonLevelScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
