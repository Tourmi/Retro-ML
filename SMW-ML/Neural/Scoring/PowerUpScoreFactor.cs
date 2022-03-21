using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class PowerUpScoreFactor : IScoreFactor
    {
        private double currScore;
        private byte prevPowerUp;
        private bool inited;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Power Up";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            var currPowerUp = dataFetcher.GetPowerUp();
            if (!inited)
            {
                prevPowerUp = currPowerUp;
                inited = true;
            }

            if (currPowerUp != 0)
            {
                if (prevPowerUp < currPowerUp && prevPowerUp <= 1)
                {
                    currScore += currPowerUp == 1 ? ScoreMultiplier : ScoreMultiplier * 2;
                }
            }

            prevPowerUp = currPowerUp;
        }

        public void LevelDone() => inited = false;

        public IScoreFactor Clone()
        {
            return new PowerUpScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
