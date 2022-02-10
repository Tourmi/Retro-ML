using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class WonLevelScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public bool ShouldStop => shouldStop;
        public double ScoreFactor { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            if (dataFetcher.WonLevel())
            {
                shouldStop = true;
                currScore += ScoreFactor;
                return;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }
    }
}
