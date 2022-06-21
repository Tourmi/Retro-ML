using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class IsInFightingDistanceScoreFactor : IScoreFactor
    {
        private double currScore;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public IsInFightingDistanceScoreFactor()
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

        public string Name => "IsInFightingDistance";

        public string Tooltip => "Reward the ai if it stays in an hitting distance of the enemy, close to it";

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
            //If distance between AI and player is equal or less than 13245 (approx 33BD)
            if (dataFetcher.GetHorizontalDistanceBetweenPlayers() <= 13245)
            {
               currScore += ScoreMultiplier;
            }
        }

        public void LevelDone()
        {
     
        }

        public IScoreFactor Clone()
        {
            return new IsInFightingDistanceScoreFactor()
            {
                IsDisabled = IsDisabled,
                ScoreMultiplier = ScoreMultiplier,
                ExtraFields = ExtraFields
            };
        }
    }
}
