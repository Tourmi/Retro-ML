using SMW_ML.Game.SuperMarioWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Scoring
{
    internal class Score
    {
        private const int MAX_IMMOBILE_FRAMES = 10;
        private const int MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED = 180;
        private const int MAX_TRAINING_FRAMES = 60 * 60;

        private double score;

        private double levelScore;
        private uint previousXPosition;
        private uint maxXPosition;
        private bool shouldStop = false;
        private int moved = -1;

        private int immobileFrames = 0;
        private int levelFrames = 0;

        public Score()
        {
            score = 1;
        }

        public void LevelDone()
        {
            score += levelScore;

            shouldStop = false;
            previousXPosition = 0;
            maxXPosition = 0;
            immobileFrames = 0;
            levelFrames = 0;
            moved = -1;
        }

        public void Update(DataGetter dataReader)
        {
            uint newPosX = dataReader.GetPositionX();
            if (newPosX > maxXPosition)
            {
                levelScore += newPosX - previousXPosition;
                maxXPosition = newPosX;
                moved++;
            }

            if (newPosX == previousXPosition && dataReader.CanAct())
            {
                immobileFrames++;
                if (immobileFrames >= MAX_IMMOBILE_FRAMES && moved < 1 || immobileFrames >= MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED)
                {
                    shouldStop = true;
                }
            } else
            {
                immobileFrames = 0;
            }

            if (dataReader.IsDead())
            {
                shouldStop = true;
            }

            levelFrames++;
            if (levelFrames >= MAX_TRAINING_FRAMES)
            {
                shouldStop = true;
            }
            previousXPosition = newPosX;
        }

        public double GetScore() => score;

        public bool ShouldStop => shouldStop;
    }
}
