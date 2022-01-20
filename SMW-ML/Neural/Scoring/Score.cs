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
        private const int MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED = 4 * 60;
        private const int MAX_TRAINING_FRAMES = 60 * 60;

        private double score;

        private double levelScore;
        private double bestLevel;
        private uint previousXPosition;
        private uint maxXPosition;
        private uint minYPosition;
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
            score += levelScore + (1000 * levelScore / levelFrames) + minYPosition;

            bestLevel = Math.Max(maxXPosition, bestLevel);

            shouldStop = false;
            previousXPosition = 0;
            maxXPosition = 0;
            immobileFrames = 0;
            levelFrames = 0;
            moved = -1;
            minYPosition = uint.MaxValue;
            levelScore = 0;
        }

        public void Update(DataGetter dataReader)
        {
            uint newPosX = dataReader.GetPositionX();

            var newPosY = dataReader.GetPositionY();

            if (newPosX <= maxXPosition && newPosY >= minYPosition && dataReader.CanAct())
            {
                immobileFrames++;
                if (immobileFrames >= MAX_IMMOBILE_FRAMES && moved < 1 || immobileFrames >= MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED)
                {
                    shouldStop = true;
                    levelFrames = MAX_TRAINING_FRAMES * 2;
                    return;
                }
            }
            else
            {
                immobileFrames = 0;
            }
            minYPosition = Math.Min(minYPosition, newPosY);

            if (newPosX > maxXPosition)
            {
                levelScore += newPosX - previousXPosition;
                maxXPosition = newPosX;
                moved++;
            }

            if (dataReader.IsDead())
            {
                shouldStop = true;
                return;
            } else if (dataReader.WonLevel())
            {
                shouldStop = true;
                levelScore *= 10;
                return;
            }

            levelFrames++;
            if (levelFrames >= MAX_TRAINING_FRAMES)
            {
                shouldStop = true;
            }
            previousXPosition = newPosX;
        }

        public double GetFinalScore()
        {
            return score + bestLevel * 10;
        }

        public bool ShouldStop => shouldStop;
    }
}
