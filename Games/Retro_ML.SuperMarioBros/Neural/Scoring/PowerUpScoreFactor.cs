using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class PowerUpScoreFactor : IScoreFactor
    {
        private const string MUSHROOM_MULT = "Mushroom Mult";
        private const string FLOWER_MULT = "Flower Mult";
        private const string CAPE_MULT = "Cape Mult";

        private double currScore;
        private byte prevPowerUp;
        private bool inited;

        public PowerUpScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(MUSHROOM_MULT, 1.0),
                new ExtraField(CAPE_MULT, 2.0),
                new ExtraField(FLOWER_MULT, 2.0),
            };
        }

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Power Up";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
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
                    currScore += ScoreMultiplier * currPowerUp switch
                    {
                        1 => ExtraField.GetValue(ExtraFields, MUSHROOM_MULT),
                        2 => ExtraField.GetValue(ExtraFields, CAPE_MULT),
                        3 => ExtraField.GetValue(ExtraFields, FLOWER_MULT),
                        _ => 1.0
                    };
                }
            }

            prevPowerUp = currPowerUp;
        }

        public void LevelDone() => inited = false;

        public IScoreFactor Clone()
        {
            return new PowerUpScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
