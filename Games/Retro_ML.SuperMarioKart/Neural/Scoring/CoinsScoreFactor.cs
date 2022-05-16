using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class CoinsScoreFactor : IScoreFactor
    {
        private const string LOSING_COINS_MULT = "Losing coins mult.";

        private double currScore;
        private bool isInit;
        private byte currCoins;

        public string Name => "Coins";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public CoinsScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(LOSING_COINS_MULT, -0.5)
            };
        }

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            isInit = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            var df = (SMKDataFetcher)dataFetcher;
            if (!isInit)
            {
                isInit = true;
                currCoins = df.GetCoins();
            }

            var newCoins = df.GetCoins();
            if (newCoins > currCoins)
            {
                currScore += (newCoins - currCoins) * ScoreMultiplier;
                currCoins = newCoins;
            }
            else if (newCoins < currCoins)
            {
                currScore += (currCoins - newCoins) * ScoreMultiplier * ExtraField.GetValue(ExtraFields, LOSING_COINS_MULT);
                currCoins = newCoins;
            }
        }

        public IScoreFactor Clone() => new CoinsScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields, IsDisabled = IsDisabled };
    }
}
