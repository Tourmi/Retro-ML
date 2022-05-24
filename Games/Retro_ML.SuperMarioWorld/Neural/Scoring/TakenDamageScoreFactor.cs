using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class TakenDamageScoreFactor : IScoreFactor
    {
        private bool wasFlashing;
        private double currScore;

        public TakenDamageScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public string Name => "Taken Damage";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (dataFetcher.IsFlashing() && !wasFlashing)
            {
                currScore += ScoreMultiplier;
            }

            wasFlashing = dataFetcher.IsFlashing();
        }

        public double GetFinalScore() => currScore;

        public void LevelDone() => wasFlashing = false;

        public IScoreFactor Clone()
        {
            return new TakenDamageScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }

    }
}
