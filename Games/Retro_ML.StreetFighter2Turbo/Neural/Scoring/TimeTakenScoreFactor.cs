using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.StreetFighter2Turbo.Game;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class TimeTakenScoreFactor : IScoreFactor
    {
        private double currScore;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public TimeTakenScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Time taken";

        public string Tooltip => "Penalize the AI for every seconds Taken to beat the enemy. Less time means less points loss";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SF2TDataFetcher)dataFetcher);
        }

        private void Update(SF2TDataFetcher dataFetcher)
        {
            //If the round is finished via timeout, reward ai if it ended with more HP than the enemy, or penalize it for the opposite.
            if ((dataFetcher.isPlayer1InEndRound() || dataFetcher.isPlayer2InEndRound()) && !dataFetcher.isPlayer1Dead() && !dataFetcher.isPlayer2Dead())
            {
                currScore += ScoreMultiplier * (dataFetcher.GetPlayer1Hp() - dataFetcher.GetPlayer2Hp());
            }
        }

        public void LevelDone()
        {
        }

        public IScoreFactor Clone()
        {
            return new TimeTakenScoreFactor() { ScoreMultiplier = ScoreMultiplier };
        }
    }
}
