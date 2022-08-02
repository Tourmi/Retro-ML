using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class DistanceScoreFactor : IScoreFactor
    {
        private double currScore;
        private uint minYPosition = 0;
        private uint maxYPosition = 0;
        private uint minXPosition = 0;
        private uint maxXPosition = 0;
        private bool inited = false;
        private uint levelUID;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(EastDistance), "East Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI is moving east"),
             new DoubleFieldInfo(nameof(WestDistance), "West Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI is moving west"),
             new DoubleFieldInfo(nameof(UpDistance), "Up Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI is moving up"),
             new DoubleFieldInfo(nameof(DownDistance), "Down Mult", double.MinValue, double.MaxValue, 0.25, "Multiplier when the AI is moving down")
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(EastDistance) => EastDistance,
                    nameof(WestDistance) => WestDistance,
                    nameof(UpDistance) => UpDistance,
                    nameof(DownDistance) => DownDistance,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(EastDistance): EastDistance = (double)value; break;
                    case nameof(WestDistance): WestDistance = (double)value; break;
                    case nameof(UpDistance): UpDistance = (double)value; break;
                    case nameof(DownDistance): DownDistance = (double)value; break;
                }
            }
        }

        public double EastDistance { get; set; } = 1.0;
        public double WestDistance { get; set; } = 0.0;
        public double UpDistance { get; set; } = 0.5;
        public double DownDistance { get; set; } = 0.25;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Distance travelled";

        public string Tooltip => "Reward for each tile the AI traverses. This is based on the maximum distance, so going back and forth will not increase the reward";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (inited && levelUID != dataFetcher.GetLevelUID()) return; // Return if not in same area anymore

            uint newPosX = dataFetcher.GetPositionX();
            uint newPosY = dataFetcher.GetPositionY();

            if (!inited)
            {
                inited = true;

                minXPosition = newPosX;
                maxXPosition = newPosX;
                minYPosition = newPosY;
                maxYPosition = newPosY;

                levelUID = dataFetcher.GetLevelUID();
            }

            if (dataFetcher.IsOnGround() || dataFetcher.IsInWater())
            {
                //TODO : Do something about entering sub-areas

                double totalSubScore = 0;
                if (newPosX > maxXPosition)
                {
                    totalSubScore += (newPosX - maxXPosition) * EastDistance;
                    maxXPosition = newPosX;
                }
                if (newPosX < minXPosition)
                {
                    totalSubScore += (minXPosition - newPosX) * WestDistance;
                    minXPosition = newPosX;
                }
                if (newPosY > maxYPosition)
                {
                    totalSubScore += (newPosY - maxYPosition) * DownDistance;
                    maxYPosition = newPosY;
                }
                if (newPosY < minYPosition)
                {
                    totalSubScore += (minYPosition - newPosY) * UpDistance;
                    minYPosition = newPosY;
                }

                currScore += totalSubScore / 16.0 * ScoreMultiplier;
            }
        }

        public void LevelDone()
        {
            minXPosition = 0;
            maxXPosition = 0;
            minYPosition = 0;
            maxYPosition = 0;
            inited = false;
        }

        public IScoreFactor Clone() => new DistanceScoreFactor()
        {
            IsDisabled = IsDisabled,
            ScoreMultiplier = ScoreMultiplier,
            EastDistance = EastDistance,
            WestDistance = WestDistance,
            DownDistance = DownDistance,
            UpDistance = UpDistance
        };
    }
}
