using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class DistanceScoreFactor : IScoreFactor
    {
        private double currScore;
        private uint maxXPosition = 0;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Distance travelled";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetPositionX();

            //TODO : Do something about entering sub-areas

            if (newPosX > maxXPosition)
            {
                currScore += (newPosX - maxXPosition) / 16.0 * ScoreMultiplier;
                maxXPosition = newPosX;
            }
        }

        public void LevelDone()
        {
            maxXPosition = 0;
        }

        public object Clone()
        {
            return new DistanceScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
