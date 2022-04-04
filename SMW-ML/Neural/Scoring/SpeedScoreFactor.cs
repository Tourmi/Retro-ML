using SMW_ML.Game.SuperMarioWorld;
using System;

namespace SMW_ML.Neural.Scoring
{
    internal class SpeedScoreFactor : IScoreFactor
    {
        private const string HORIZONTAL_MULT = "Horizontal Mult";
        private const string VERTICAL_MULT = "Vertical Mult";

        private uint initialX = 0;
        private uint initialY = 0;

        private uint maxX = 0;
        private uint minX = uint.MaxValue;
        private uint maxY = 0;
        private uint minY = uint.MaxValue;

        private int framesTaken = 0;
        private double currScore = 0;
        private bool inited = false;

        public SpeedScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(HORIZONTAL_MULT, 1.0),
                new ExtraField(VERTICAL_MULT, 0.25),
            };
        }

        public string Name => "Speed";
        public bool CanBeDisabled => true;
        public bool IsDisabled { get; set; }
        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(DataFetcher dataFetcher)
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

            double rightDistance = ((double)maxX) - initialX;
            double leftDistance = ((double)initialX) - minX;
            double upDistance = ((double)initialY) - minY;
            double downDistance = ((double)maxY) - initialY;

            if (rightDistance < 1.5 * 16) rightDistance = 0;
            if (leftDistance < 1.5 * 16) leftDistance = 0;
            if (upDistance < 1.5 * 16) upDistance = 0;
            if (downDistance < 1.5 * 16) downDistance = 0;

            subTotal += Math.Max(rightDistance, leftDistance) * ExtraField.GetValue(ExtraFields, HORIZONTAL_MULT);
            subTotal += Math.Max(upDistance, downDistance) * ExtraField.GetValue(ExtraFields, VERTICAL_MULT);

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

        public IScoreFactor Clone() => new SpeedScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
    }
}
