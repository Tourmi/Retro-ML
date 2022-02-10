using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class DistanceScoreFactor : IScoreFactor
    {
        private double currScore;
        private uint maxXPosition = 0;

        public bool ShouldStop => false;
        public double ScoreFactor { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetPositionX();

            //TODO : Do something about entering sub-areas

            if (newPosX > maxXPosition)
            {
                currScore += (newPosX - maxXPosition) / 16.0 * ScoreFactor;
                maxXPosition = newPosX;
            }
        }

        public void LevelDone()
        {
            maxXPosition = 0;
        }
    }
}
