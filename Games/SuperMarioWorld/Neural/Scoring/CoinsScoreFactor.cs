using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
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

        public void Update(IDataFetcher dataFetcher)
        {
            Update((DataFetcher)dataFetcher);
        }

        private void Update(DataFetcher dataFetcher)
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
