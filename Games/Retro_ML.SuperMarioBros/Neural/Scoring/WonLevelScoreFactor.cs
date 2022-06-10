using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class WonLevelScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public WonLevelScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

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

        public string Name => "Won level";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
        {
            if (dataFetcher.WonLevel())
            {
                shouldStop = true;
                currScore += ScoreMultiplier;
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
