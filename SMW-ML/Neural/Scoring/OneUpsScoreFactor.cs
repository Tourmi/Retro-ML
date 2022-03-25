using SMW_ML.Game.SuperMarioWorld;
using System;

namespace SMW_ML.Neural.Scoring
{
    internal class OneUpsScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currOneUps;
        private double score;

        public OneUpsScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public string Name => "1-ups";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(DataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currOneUps = dataFetcher.GetLives();
                isInit = true;
                return;
            }

            int newOneUps = dataFetcher.GetLives();
            if (newOneUps > currOneUps)
            {
                score += (newOneUps - currOneUps) * ScoreMultiplier;
            }

            currOneUps = dataFetcher.GetLives();
        }

        public void LevelDone()
        {
            isInit = false;
        }

        public double GetFinalScore() => score;

        public IScoreFactor Clone()
        {
            return new OneUpsScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
