using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class CheckpointReachedScoreFactor : IScoreFactor
    {
        private const string MAX_TIME_WITHOUT_PROGRESS = "Max time w/o progress";

        private bool init = false;
        private bool shouldStop = false;
        private int framesWithoutCheckpoint;
        private double currScore;
        private int maxCheckpoint = 0;
        private int currCheckpoint = 0;
        private int previousCheckpoint = 0;

        public CheckpointReachedScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(MAX_TIME_WITHOUT_PROGRESS, 10)
            };
        }

        public bool ShouldStop => shouldStop;
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
                framesWithoutCheckpoint = 0;
                maxCheckpoint = df.GetMaxCheckpoint();
                previousCheckpoint = df.GetCurrentCheckpoint() + df.GetCurrentLap() * maxCheckpoint;
            }
            currCheckpoint = df.GetCurrentCheckpoint() + df.GetCurrentLap() * maxCheckpoint;

            if (currCheckpoint > previousCheckpoint)
            {
                currScore += ScoreMultiplier * (currCheckpoint - previousCheckpoint);
                previousCheckpoint = currCheckpoint;
                framesWithoutCheckpoint = 0;
            }

            if (framesWithoutCheckpoint >= ExtraField.GetValue(ExtraFields, MAX_TIME_WITHOUT_PROGRESS) * 60)
            {
                shouldStop = true;
            }

            framesWithoutCheckpoint++;
        }

        public void LevelDone()
        {
            init = false;
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new CheckpointReachedScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
