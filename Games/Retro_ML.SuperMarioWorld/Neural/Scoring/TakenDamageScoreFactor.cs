using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class TakenDamageScoreFactor : IScoreFactor
    {
        private bool wasFlashing;
        private double currScore;

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

        public string Name => "Taken Damage";

        public string Tooltip => "Reward given whenever an AI takes damage, not counting dying";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

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
