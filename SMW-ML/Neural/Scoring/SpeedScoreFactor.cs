using SMW_ML.Game.SuperMarioWorld;
using System;

namespace SMW_ML.Neural.Scoring
{
    internal class SpeedScoreFactor : IScoreFactor
    {
        private uint maxX = 0;
        private int framesTaken = 0;
        private double currScore = 0;

        public string Name => "Speed";
        public bool CanBeDisabled => true;
        public bool IsDisabled { get; set; }
        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public void Update(DataFetcher dataFetcher)
        {
            maxX = Math.Max(maxX, dataFetcher.GetPositionX());
            framesTaken++;
        }

        public void LevelDone()
        {
            currScore += (maxX / 16.0) / (Math.Max(framesTaken, 1) / 60.0) * ScoreMultiplier;
            framesTaken = 0;
            maxX = 0;
        }

        public double GetFinalScore() => currScore;

        public object Clone() => new SpeedScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
    }
}
