using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class PowerUpScoreFactor : IScoreFactor
    {
        private const string MUSHROOM_MULT = "Mushroom Mult";
        private const string FLOWER_MULT = "Flower Mult";
        private const string CAPE_MULT = "Cape Mult";

        private double currScore;
        private byte prevPowerUp;
        private bool inited;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MushroomMult), "Mushroom Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI gets a mushroom"),
             new DoubleFieldInfo(nameof(FlowerMult), "Flower Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI gets a flower"),
             new DoubleFieldInfo(nameof(CapeMult), "Cape Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI gets a cape"),
        };

        public PowerUpScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(MUSHROOM_MULT, 1.0),
                new ExtraField(CAPE_MULT, 2.0),
                new ExtraField(FLOWER_MULT, 2.0),
            };
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MushroomMult) => MushroomMult,
                    nameof(FlowerMult) => FlowerMult,
                    nameof(CapeMult) => CapeMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MushroomMult): MushroomMult = (double)value; break;
                    case nameof(FlowerMult): FlowerMult = (double)value; break;
                    case nameof(CapeMult): CapeMult = (double)value; break;
                }
            }
        }

        public double MushroomMult { get; set; } = 1.0;
        public double FlowerMult { get; set; } = 2.0;
        public double CapeMult { get; set; } = 2.0;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Power Up";

        public string Tooltip => "Points given whenever the AI collects a power up it didn't have yet";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
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
                        1 => MushroomMult,
                        2 => CapeMult,
                        3 => FlowerMult,
                        _ => 1.0
                    };
                }
            }

            prevPowerUp = currPowerUp;
        }

        public void LevelDone() => inited = false;

        public IScoreFactor Clone()
        {
            return new PowerUpScoreFactor()
            {
                IsDisabled = IsDisabled,
                ScoreMultiplier = ScoreMultiplier,
                ExtraFields = ExtraFields,
                MushroomMult = MushroomMult,
                CapeMult = CapeMult,
                FlowerMult = FlowerMult
            };
        }
    }
}
