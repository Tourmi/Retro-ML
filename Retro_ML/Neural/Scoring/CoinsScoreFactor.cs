using Retro_ML.Game.SuperMarioWorld;
using System;

namespace Retro_ML.Neural.Scoring
{
    internal class CoinsScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currCoins;
        private double score;

        public CoinsScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public string Name => "Coins";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(DataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currCoins = dataFetcher.GetCoins();
                isInit = true;
                return;
            }

            int newCoins = dataFetcher.GetCoins();
            if (newCoins < currCoins) newCoins += 100;
            if (newCoins > currCoins)
            {
                score += (newCoins - currCoins) * ScoreMultiplier;
            }

            currCoins = dataFetcher.GetCoins();
        }

        public void LevelDone()
        {
            isInit = false;
        }

        public double GetFinalScore() => score;

        public IScoreFactor Clone()
        {
            return new CoinsScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
