using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class LostFightScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;
        private ulong maxTime = 0;
        private bool isInited = false;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public LostFightScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }

        public string Tooltip => "The reward to give an AI that lost a round / fight";

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Died";

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
            if (!isInited)
            {
                maxTime = dataFetcher.GetRoundTimer();
                isInited = true;
            }

            //If the round is finished via Player 1 K.O, penalize the AI depending on the time he survived.
            if (dataFetcher.isPlayer1Dead() && !dataFetcher.isPlayer2Dead())
            {
                currScore += ScoreMultiplier + (maxTime - dataFetcher.GetRoundTimer());
                shouldStop = true;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
            maxTime = 0;
            isInited = false;
        }

        public IScoreFactor Clone()
        {
            return new LostFightScoreFactor() { ScoreMultiplier = ScoreMultiplier };
        }
    }
}
