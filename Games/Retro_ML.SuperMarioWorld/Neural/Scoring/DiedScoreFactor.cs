using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class DiedScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public DiedScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Died";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (dataFetcher.IsDead())
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
            return new DiedScoreFactor() { ScoreMultiplier = ScoreMultiplier };
        }
    }
}
