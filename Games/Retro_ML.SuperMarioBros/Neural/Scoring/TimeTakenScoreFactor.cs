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

        public TimeTakenScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }
        public FieldInfo[] Fields => Array.Empty<FieldInfo>();
        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                }
            }
        }

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
            if (levelFrames >= ExtraField.GetValue(ExtraFields, MAXIMUM_LEVEL_TIME) * 60)
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
            return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
