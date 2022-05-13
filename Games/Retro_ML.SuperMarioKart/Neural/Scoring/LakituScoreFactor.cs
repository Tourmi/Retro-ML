using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class LakituScoreFactor : IScoreFactor
    {
        private const string STOP_AFTER_X_FALLS = "Stop after X falls";

        private double currScore;
        private bool isInit;
        private byte currStatus;
        private int fellCount;

        public string Name => "Lakitu";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        private bool shouldStop = false;
        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public LakituScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(STOP_AFTER_X_FALLS, 1)
            };
        }

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            isInit = false;
            fellCount = 0;
            shouldStop = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            SMKDataFetcher df = (SMKDataFetcher)dataFetcher;
            if (!isInit)
            {
                isInit = true;
                currStatus = df.GetKartStatus();
            }

            byte newStatus = df.GetKartStatus();
            if (newStatus != currStatus)
            {
                currStatus = newStatus;

                if (newStatus == 0x4 || newStatus == 0x6 || newStatus == 0x8)
                {
                    currScore += ScoreMultiplier;
                    fellCount++;

                    double stopAfter = ExtraField.GetValue(ExtraFields, STOP_AFTER_X_FALLS);

                    if (stopAfter >= 1 && fellCount >= stopAfter)
                    {
                        shouldStop = true;
                    }
                }
            }
        }

        public IScoreFactor Clone() => new LakituScoreFactor() { ScoreMultiplier = ScoreMultiplier, IsDisabled = IsDisabled, ExtraFields = ExtraFields };
    }
}
