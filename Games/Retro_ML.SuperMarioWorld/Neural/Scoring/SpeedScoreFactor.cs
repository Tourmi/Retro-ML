using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class SpeedScoreFactor : IScoreFactor
    {
        private uint initialX = 0;
        private uint initialY = 0;

        private uint maxX = 0;
        private uint minX = uint.MaxValue;
        private uint maxY = 0;
        private uint minY = uint.MaxValue;

        private int framesTaken = 0;
        private double currScore = 0;
        private bool inited = false;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(HorizontalMult), "Horizontal Multiplier", double.MinValue, double.MaxValue, 0.25),
             new DoubleFieldInfo(nameof(VerticalMult), "Vertical Multiplier", double.MinValue, double.MaxValue, 0.25),
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(HorizontalMult) => HorizontalMult,
                    nameof(VerticalMult) => VerticalMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(HorizontalMult): HorizontalMult = (double)value; break;
                    case nameof(VerticalMult): VerticalMult = (double)value; break;
                }
            }
        }

        public double HorizontalMult { get; set; } = 1.0;
        public double VerticalMult { get; set; } = 0.25;

        public string Name => "Speed";

        public string Tooltip => "Reward based on the speed of the AI";

        public bool CanBeDisabled => true;
        public bool IsDisabled { get; set; }
        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (!inited)
            {
                inited = true;
                initialX = dataFetcher.GetPositionX();
                initialY = dataFetcher.GetPositionY();
            }
            if (dataFetcher.IsOnGround() || dataFetcher.IsInWater())
            {
                maxX = Math.Max(maxX, dataFetcher.GetPositionX());
                minX = Math.Min(minX, dataFetcher.GetPositionX());
                maxY = Math.Max(maxY, dataFetcher.GetPositionY());
                minY = Math.Min(minY, dataFetcher.GetPositionY());
            }

            framesTaken++;
        }

        public void LevelDone()
        {
            double subTotal = 0.0;

            double rightDistance = (double)maxX - initialX;
            double leftDistance = (double)initialX - minX;
            double upDistance = (double)initialY - minY;
            double downDistance = (double)maxY - initialY;

            if (rightDistance < 1.5 * 16) rightDistance = 0;
            if (leftDistance < 1.5 * 16) leftDistance = 0;
            if (upDistance < 1.5 * 16) upDistance = 0;
            if (downDistance < 1.5 * 16) downDistance = 0;

            subTotal += Math.Max(rightDistance, leftDistance) * HorizontalMult;
            subTotal += Math.Max(upDistance, downDistance) * VerticalMult;

            currScore += subTotal * ScoreMultiplier / (Math.Max(framesTaken, 1) / 60.0) / 16.0;

            framesTaken = 0;
            initialX = 0;
            initialY = 0;

            maxX = 0;
            minX = uint.MaxValue;
            maxY = 0;
            minY = uint.MaxValue;

            inited = false;
        }

        public double GetFinalScore() => currScore;

        public IScoreFactor Clone() => new SpeedScoreFactor()
        {
            IsDisabled = IsDisabled,
            ScoreMultiplier = ScoreMultiplier,
            HorizontalMult = HorizontalMult,
            VerticalMult = VerticalMult
        };
    }
}
