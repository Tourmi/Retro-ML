using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class OffRoadScoreFactor : IScoreFactor
    {
        private const string STOP_AFTER = "Stop after";

        private bool shouldStop = false;
        private double currScore;
        private int framesOffroad;

        public string Name => "Offroad";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public OffRoadScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(STOP_AFTER, 5)
            };
        }

        public double GetFinalScore() => currScore;

        public void LevelDone() => shouldStop = false;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMKDataFetcher)dataFetcher);
        }

        private void Update(SMKDataFetcher dataFetcher)
        {
            if (dataFetcher.IsOffroad())
            {
                currScore += ScoreMultiplier / 60.0;
                framesOffroad++;
            }
            else
            {
                framesOffroad = 0;
            }

            if (framesOffroad >= ExtraField.GetValue(ExtraFields, STOP_AFTER))
            {
                shouldStop = true;
            }
        }

        public IScoreFactor Clone()
        {
            return new OffRoadScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
