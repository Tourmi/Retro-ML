using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.Tetris.Neural.Scoring
{
    internal class TimeTakenScoreFactor : IScoreFactor
    {
        public const string MAXIMUM_LEVEL_TIME = "Maximum Level Time";

        private bool shouldStop = false;
        private double currScore;
        private int levelFrames = 0;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MaximumLevelTime), "Maximum Level Time", 1.0, double.MaxValue, 0.5, "Maximum time the current savestate can go on for, in seconds")
        };

        public TimeTakenScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new(MAXIMUM_LEVEL_TIME, 600)
            };
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaximumLevelTime) => MaximumLevelTime,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaximumLevelTime): MaximumLevelTime = (double)value; break;
                }
            }
        }

        public double MaximumLevelTime { get; set; } = 600;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Time taken";

        public string Tooltip => "Reward applied every second passed in the current save state";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            levelFrames++;
            currScore += ScoreMultiplier / 60.0;
            if (levelFrames >= MaximumLevelTime * 60)
            {
                shouldStop = true;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
            levelFrames = 0;
        }

        public IScoreFactor Clone()
        {
            return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields, MaximumLevelTime = MaximumLevelTime };
        }
    }
}
