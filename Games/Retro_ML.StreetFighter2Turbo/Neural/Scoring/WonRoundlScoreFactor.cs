using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class WonRoundlScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public WonRoundlScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Won level";

        public string Tooltip => "Reward to give if the AI wins a round.";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SF2TDataFetcher)dataFetcher);
        }

        private void Update(SF2TDataFetcher dataFetcher)
        {
            if (dataFetcher.isPlayer2Dead())
            {
                shouldStop = true;
                currScore += ScoreMultiplier;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new WonRoundlScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
