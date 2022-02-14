using SMW_ML.Game.SuperMarioWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Scoring
{
    internal class CoinsScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currCoins;
        private double score;

        public string Name => "Coins";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public void Update(DataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currCoins = dataFetcher.GetCoins();
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

        public object Clone()
        {
            return new CoinsScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
