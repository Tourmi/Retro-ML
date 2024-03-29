﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class DistanceScoreFactor : IScoreFactor
    {
        private double currScore;
        private uint maxXPosition = 0;
        private bool inited = false;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Distance travelled";

        public string Tooltip => "Reward for each tile the AI traverses. This is based on the maximum distance, so going back and forth will not increase the reward";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetMarioPositionX();

            if (!inited)
            {
                inited = true;

                maxXPosition = newPosX;
            }

            if (dataFetcher.IsOnGround() || dataFetcher.IsInWater())
            {
                if (newPosX > maxXPosition)
                {
                    currScore += (newPosX - maxXPosition) / 16.0 * ScoreMultiplier;
                    maxXPosition = newPosX;
                }
            }
        }

        public void LevelDone()
        {
            maxXPosition = 0;
            inited = false;
        }

        public IScoreFactor Clone()
        {
            return new DistanceScoreFactor()
            {
                IsDisabled = IsDisabled,
                ScoreMultiplier = ScoreMultiplier
            };
        }
    }
}
