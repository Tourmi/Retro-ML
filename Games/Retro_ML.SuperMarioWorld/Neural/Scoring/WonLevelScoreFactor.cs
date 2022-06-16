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
             new DoubleFieldInfo(nameof(GoalMult), "Goal Multiplier", double.MinValue, double.MaxValue, 0.25, "Set to 0 or a negative value if you want to discourage finishing a level through the regular level ending"),
             new DoubleFieldInfo(nameof(KeyMult), "Key Multiplier", double.MinValue, double.MaxValue, 0.25, "Set to 0 or a negative value if you want to discourage finishing a level with a key"),
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

        public string Tooltip => "Reward to give if the AI wins a level. Ideally, this should be a high value to encourage actually finishing levels";

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
                currScore += ScoreMultiplier * (dataFetcher.WonViaGoal() ? GoalMult : KeyMult);
                return;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new WonLevelScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                ExtraFields = ExtraFields,
                GoalMult = GoalMult,
                KeyMult = KeyMult
            };
        }
    }
}
