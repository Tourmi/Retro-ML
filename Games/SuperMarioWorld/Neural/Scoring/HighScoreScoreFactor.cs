using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class HighScoreScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currScore;
        private double score;

        public HighScoreScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public string Name => "High Score";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((DataFetcher)dataFetcher);
        }

        private void Update(DataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currScore = dataFetcher.GetScore();
                isInit = true;
                return;
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
