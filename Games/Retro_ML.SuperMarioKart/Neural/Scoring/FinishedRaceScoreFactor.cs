using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class FinishedRaceScoreFactor : IScoreFactor
    {
        private double currScore;

        public string Name => "Finished race";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        private bool shouldStop = false;
        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public FinishedRaceScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            shouldStop = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            if (!shouldStop && ((SMKDataFetcher)dataFetcher).GetCurrentLap() >= 5)
            {
                shouldStop = true;
                currScore += ScoreMultiplier;
            }
        }

        public IScoreFactor Clone()
        {
            return new FinishedRaceScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
