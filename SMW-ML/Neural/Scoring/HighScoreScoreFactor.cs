using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class HighScoreScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currScore;
        private double score;

        public string Name => "High Score";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public void Update(DataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currScore = dataFetcher.GetScore();
            }

            int newScore = dataFetcher.GetScore();
            if (newScore > currScore)
            {
                score += (newScore - currScore) * ScoreMultiplier / 100;
            }

            currScore = dataFetcher.GetScore();
        }

        public void LevelDone()
        {
            isInit = false;
        }

        public double GetFinalScore() => score;

        public IScoreFactor Clone()
        {
            return new HighScoreScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
