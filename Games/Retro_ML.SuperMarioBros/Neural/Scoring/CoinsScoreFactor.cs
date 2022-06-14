using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class CoinsScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currCoins;
        private double score;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public CoinsScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }
        public string Name => "Coins";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
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
            return new CoinsScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
