using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class CollisionScoreFactor : IScoreFactor
    {
        public const string STOP_AFTER_X_COLLISIONS = "Maximum collisions";

        private double currScore;
        private bool isInit;
        private bool wasInCollision;
        private int collisionCount;

        public string Name => "Collisions";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        private bool shouldStop = false;
        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public CollisionScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(STOP_AFTER_X_COLLISIONS, 5)
            };
        }

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            isInit = false;
            collisionCount = 0;
            shouldStop = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            SMKDataFetcher df = (SMKDataFetcher)dataFetcher;
            if (!isInit)
            {
                isInit = true;
                wasInCollision = df.GetCollisionTimer() > 0;
            }

            bool isInCollision = df.GetCollisionTimer() > 0;
            if (isInCollision != wasInCollision)
            {
                wasInCollision = isInCollision;

                if (isInCollision)
                {
                    currScore += ScoreMultiplier;
                    collisionCount++;

                    double stopAfter = ExtraField.GetValue(ExtraFields, STOP_AFTER_X_COLLISIONS);

                    if (stopAfter >= 1 && collisionCount >= stopAfter)
                    {
                        shouldStop = true;
                    }
                }
            }
        }

        public IScoreFactor Clone() => new CollisionScoreFactor() { ScoreMultiplier = ScoreMultiplier, IsDisabled = IsDisabled, ExtraFields = ExtraFields };
    }
}
