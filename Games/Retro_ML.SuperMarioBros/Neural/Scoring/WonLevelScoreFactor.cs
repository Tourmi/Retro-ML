using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class WonLevelScoreFactor : IScoreFactor
    {
        private const string GOAL_MULT = "Goal Mult";
        private const string KEY_MULT = "Key Mult";

        private bool shouldStop = false;
        private double currScore;

        public WonLevelScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(GOAL_MULT, 1.0),
                new ExtraField(KEY_MULT, 1.0),
            };
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Won level";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
        {
            if (dataFetcher.WonLevel())
            {
                shouldStop = true;
                currScore += ScoreMultiplier;
                return;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new WonLevelScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
