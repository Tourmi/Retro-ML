using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class PowerUpScoreFactor : IScoreFactor
    {
        private const string MUSHROOM_MULT = "Mushroom Mult";
        private const string FLOWER_MULT = "Flower Mult";
        private const string STAR_MULT = "Star Mult";

        private double currScore;
        private byte prevPowerUp;
        private bool inited;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MushroomMult), "Mushroom Mult", double.MinValue, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(FlowerMult), "Flower Mult", double.MinValue, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(StarMult), "Star Mult", double.MinValue, double.MaxValue, 0.25),
        };

        public PowerUpScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MushroomMult) => MushroomMult,
                    nameof(FlowerMult) => FlowerMult,
                    nameof(StarMult) => StarMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MushroomMult): MushroomMult = (double)value; break;
                    case nameof(FlowerMult): FlowerMult = (double)value; break;
                    case nameof(StarMult): StarMult = (double)value; break;
                }
            }
        }

        public double MushroomMult { get; set; } = 1.0;
        public double FlowerMult { get; set; } = 2.0;
        public double StarMult { get; set; } = 3.0;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Power Up";

        public string Tooltip => "Reward given whenever the AI collects a power up it didn't have yet";

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

            if (currPowerUp > prevPowerUp)
            {
                currScore += ScoreMultiplier * currPowerUp switch
                {
                    1 => MushroomMult,
                    2 => FlowerMult,
                    3 => StarMult,
                    _ => 1.0
                };
            }

            prevPowerUp = currPowerUp;
        }

        public void LevelDone() => inited = false;

        public IScoreFactor Clone()
        {
            return new PowerUpScoreFactor() { 
                IsDisabled = IsDisabled, 
                ScoreMultiplier = ScoreMultiplier, 
                MushroomMult = MushroomMult,
                FlowerMult = FlowerMult,
                StarMult = StarMult
            };
        }
    }
}
