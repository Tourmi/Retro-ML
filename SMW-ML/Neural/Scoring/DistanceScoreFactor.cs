﻿using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class DistanceScoreFactor : IScoreFactor
    {
        private double currScore;
        private uint maxXPosition = 0;
        private bool inited = false;
        private uint levelUID;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Distance travelled";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            if (inited && levelUID != dataFetcher.GetLevelUID()) return;

            uint newPosX = dataFetcher.GetPositionX();
            if (!inited)
            {
                inited = true;
                maxXPosition = newPosX;
                levelUID = dataFetcher.GetLevelUID();
            }

            if (newPosX > maxXPosition)
            {
                currScore += (newPosX - maxXPosition) / 16.0 * ScoreMultiplier;
                maxXPosition = newPosX;
            }
        }

        public void LevelDone()
        {
            maxXPosition = 0;
            inited = false;
        }

        public IScoreFactor Clone()
        {
            return new DistanceScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
