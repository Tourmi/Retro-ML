using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.Neural.Scoring
{
    internal class TakenDamageScoreFactor : IScoreFactor
    {
        private bool wasFlashing;
        private double currScore;

        public string Name => "Taken Damage";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public void Update(DataFetcher dataFetcher)
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
