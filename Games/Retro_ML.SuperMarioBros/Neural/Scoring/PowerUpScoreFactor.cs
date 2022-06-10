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
        private const string ONEUP_MULT = "OneUp Mult";

        private double currScore;
        private byte prevPowerUp;
        private bool inited;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MushroomMult), "Mushroom Mult", double.MinValue, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(FlowerMult), "Flower Mult", double.MinValue, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(StarMult), "Star Mult", double.MinValue, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(OneUpMult), "OneUp Mult", double.MinValue, double.MaxValue, 0.25),
        };

        public PowerUpScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(MUSHROOM_MULT, 1.0),
                new ExtraField(FLOWER_MULT, 2.0),
                new ExtraField(STAR_MULT, 3.0),
                new ExtraField(ONEUP_MULT, 4.0),
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
                    nameof(StarMult) => FlowerMult,
                    nameof(OneUpMult) => FlowerMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MushroomMult): MushroomMult = (double)value; break;
                    case nameof(FlowerMult): FlowerMult = (double)value; break;
                    case nameof(StarMult): FlowerMult = (double)value; break;
                    case nameof(OneUpMult): FlowerMult = (double)value; break;
                }
            }
        }

        public double MushroomMult { get; set; } = 1.0;
        public double FlowerMult { get; set; } = 2.0;
        public double StarMult { get; set; } = 3.0;
        public double OneUpMult { get; set; } = 4.0;

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

            if (currPowerUp > prevPowerUp)
            {
                currScore += ScoreMultiplier * currPowerUp switch
                {
                    1 => ExtraField.GetValue(ExtraFields, MUSHROOM_MULT),
                    2 => ExtraField.GetValue(ExtraFields, FLOWER_MULT),
                    3 => ExtraField.GetValue(ExtraFields, STAR_MULT),
                    4 => ExtraField.GetValue(ExtraFields, ONEUP_MULT),
                    _ => 1.0
                };
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
