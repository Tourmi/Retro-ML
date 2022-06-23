using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.StreetFighter2Turbo.Game;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class DrawFightScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool shouldStop = false;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public DrawFightScoreFactor()
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
            if (dataFetcher.isPlayer1InEndRound() && dataFetcher.isPlayer2InEndRound() && !dataFetcher.isPlayer1Dead() && !dataFetcher.isPlayer2Dead())
            {
                if (dataFetcher.GetPlayer1Hp() >= dataFetcher.GetPlayer2Hp())
                {
                    currScore += ScoreMultiplier * (dataFetcher.GetPlayer1Hp() - dataFetcher.GetPlayer2Hp());
                }
                else if (dataFetcher.GetPlayer2Hp() >= dataFetcher.GetPlayer1Hp())
                {
                    currScore += ScoreMultiplier * (dataFetcher.GetPlayer2Hp() - dataFetcher.GetPlayer1Hp());
                }
                else
                {
                    currScore += 0;
                }

                shouldStop = true;

            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new DrawFightScoreFactor() { ScoreMultiplier = ScoreMultiplier };
        }
    }
}
