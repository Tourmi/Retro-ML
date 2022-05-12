using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class YoshiCoinsScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currYoshiCoins;
        private double score;

        public YoshiCoinsScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public string Name => "Yoshi Coins";

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
            if (!isInit)
            {
                currYoshiCoins = dataFetcher.GetYoshiCoins();
                isInit = true;
                return;
            }

            int newYoshiCoins = dataFetcher.GetYoshiCoins();
            if (newYoshiCoins > currYoshiCoins)
            {
                score += (newYoshiCoins - currYoshiCoins) * ScoreMultiplier;
            }

            currYoshiCoins = dataFetcher.GetYoshiCoins();
        }

        public void LevelDone()
        {
            isInit = false;
        }

        public double GetFinalScore() => score;

        public IScoreFactor Clone()
        {
            return new YoshiCoinsScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
