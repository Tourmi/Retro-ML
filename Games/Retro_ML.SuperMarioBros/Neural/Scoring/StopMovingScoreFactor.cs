using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
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

        public StopMovingScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Stopped moving";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetPositionX();
            uint newPosY = dataFetcher.GetPositionY();

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
