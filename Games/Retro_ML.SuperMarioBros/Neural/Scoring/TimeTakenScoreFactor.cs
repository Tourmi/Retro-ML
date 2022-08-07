using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class TimeTakenScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;
        private int levelFrames = 0;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MaximumLevelTime), "Maximum Level Time", 60.0, double.MaxValue, 1.0)
        };

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

        public string Tooltip => "Reward applied when the AI takes way too long to complete a level. Recommended to set to a negative value and leave enabled in case AIs decide to take way too much time on a level";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

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
            return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, MaximumLevelTime = MaximumLevelTime };
        }
    }
}
