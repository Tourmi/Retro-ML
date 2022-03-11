using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class TimeTakenScoreFactor : IScoreFactor
    {
        private const int MAX_TRAINING_FRAMES = 120 * 60;

        private bool shouldStop = false;
        private double currScore;
        private int levelFrames = 0;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Time taken";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            if (levelFrames >= MAX_TRAINING_FRAMES)
            {
                shouldStop = true;
            }
            levelFrames++;
        }

        public void LevelDone()
        {
            currScore += (MAX_TRAINING_FRAMES - levelFrames) / 60.0 * ScoreMultiplier;
            shouldStop = false;
            levelFrames = 0;
        }

        public IScoreFactor Clone()
        {
            return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
