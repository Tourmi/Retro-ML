﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class StopMovingScoreFactor : IScoreFactor
    {
        private const int MAX_IMMOBILE_FRAMES = 15;
        private const int MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED = 4 * 60;

        private int immobileFrames = 0;

        private uint minXPosition = uint.MaxValue;
        private uint maxXPosition = 0;
        private uint minYPosition = uint.MaxValue;
        private uint maxYPosition = 0;
        private bool shouldStop = false;
        private int moved = -2;

        private uint levelUID;

        private double currScore = 0;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                }
            }
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Stopped moving";

        public string Tooltip => "Stops the current level if the AI has stopped progressing through the level. This is based on the maximum distance reached so far, not the current position";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetPositionX();
            uint newPosY = dataFetcher.GetPositionY();

            uint luid = dataFetcher.GetLevelUID();
            if (levelUID != luid)
            {
                levelUID = luid;

                minXPosition = uint.MaxValue;
                maxXPosition = 0;
                minYPosition = uint.MaxValue;
                maxYPosition = 0;

                immobileFrames = 0;
            }

            if (dataFetcher.CanAct() && newPosX <= maxXPosition && newPosX >= minXPosition && newPosY <= maxYPosition && newPosY >= minYPosition)
            {
                immobileFrames++;
                if (immobileFrames >= MAX_IMMOBILE_FRAMES && moved < 1 || immobileFrames >= MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED)
                {
                    shouldStop = true;
                    currScore += ScoreMultiplier;
                }
            }
            else
            {
                immobileFrames = 0;
                moved++;
            }

            maxXPosition = Math.Max(newPosX, maxXPosition);
            minXPosition = Math.Min(newPosX, minXPosition);
            maxYPosition = Math.Max(newPosY, maxYPosition);
            minYPosition = Math.Min(newPosY, minYPosition);
        }

        public void LevelDone()
        {
            shouldStop = false;
            moved = -2;

            minXPosition = uint.MaxValue;
            maxXPosition = 0;
            minYPosition = uint.MaxValue;
            maxYPosition = 0;

            immobileFrames = 0;
        }

        public IScoreFactor Clone()
        {
            return new StopMovingScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
