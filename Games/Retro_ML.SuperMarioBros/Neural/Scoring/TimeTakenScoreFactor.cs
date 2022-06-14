using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class TimeTakenScoreFactor : IScoreFactor
    {
        public const string MAXIMUM_LEVEL_TIME = "Maximum Level Time";

        private bool shouldStop = false;
        private double currScore;
        private int levelFrames = 0;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MaximumLevelTime), "Maximum Level Time", 60.0, double.MaxValue, 1.0)
        };

        public TimeTakenScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new(MAXIMUM_LEVEL_TIME, 240)
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

        public double MaximumLevelTime { get; set; } = 240;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Time taken";

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
            return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier,  MaximumLevelTime = MaximumLevelTime };
        }
    }
}
