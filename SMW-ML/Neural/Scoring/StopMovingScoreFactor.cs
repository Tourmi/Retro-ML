using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class StopMovingScoreFactor : IScoreFactor
    {
        private const int MAX_IMMOBILE_FRAMES = 15;
        private const int MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED = 4 * 60;
        private int immobileFrames = 0;

        private uint maxXPosition = 0;
        private bool shouldStop = false;
        private int moved = -2;

        private double currScore = 0;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Stopped moving";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(DataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetPositionX();

            //TODO : Do something about entering sub areas

            if (newPosX <= maxXPosition && dataFetcher.CanAct())
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
                maxXPosition = newPosX;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
            moved = -2;
            maxXPosition = 0;
            immobileFrames = 0;
        }

        public object Clone()
        {
            return new DiedScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
