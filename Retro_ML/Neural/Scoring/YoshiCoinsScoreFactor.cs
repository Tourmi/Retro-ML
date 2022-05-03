using Retro_ML.Game.SuperMarioWorld;
using System;

namespace Retro_ML.Neural.Scoring
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

        public void Update(DataFetcher dataFetcher)
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
