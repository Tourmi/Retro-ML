using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class CheckpointReachedScoreFactor : IScoreFactor
    {
        private bool init = false;
        private double currScore;
        private int maxCheckpoint = 0;
        private int currCheckpoint = 0;
        private int previousCheckpoint = 0;

        public FieldInfo[] Fields => new FieldInfo[]
        {
        };

        public CheckpointReachedScoreFactor()
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

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Checkpoint Reached";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            var df = (SMKDataFetcher)dataFetcher;
            if (!init)
            {
                init = true;
                maxCheckpoint = df.GetMaxCheckpoint();
                previousCheckpoint = df.GetCurrentCheckpoint() + df.GetCurrentLap() * maxCheckpoint;
            }
            currCheckpoint = df.GetCurrentCheckpoint() + df.GetCurrentLap() * maxCheckpoint;

            if (currCheckpoint > previousCheckpoint)
            {
                currScore += ScoreMultiplier * (currCheckpoint - previousCheckpoint);
                previousCheckpoint = currCheckpoint;
            }
        }

        public void LevelDone()
        {
            init = false;
        }

        public IScoreFactor Clone()
        {
            return new CheckpointReachedScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
