using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class IsCloseToEnemyScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool isInited = false;
        private uint p1CurrentHP = 0;
        private uint p2CurrentHP = 0;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public IsCloseToEnemyScoreFactor()
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

        public string Name => "Is close to the enemy";

        public string Tooltip => "Penalize the ai if it does not stays in an hitting distance of the enemy, close to it. If the ai has more HP than the enemy, fleeing is a valid strategy therefore it wont lose points";

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
            var distance = dataFetcher.GetHorizontalDistanceBetweenPlayers();
            var p1HP = dataFetcher.GetPlayer1Hp();
            var p2HP = dataFetcher.GetPlayer2Hp();

            if (!isInited)
            {
                p1CurrentHP = p1HP;
                p2CurrentHP = p2HP;
                isInited = true;
            }

            //If the player has the same or less HP compared to the enemy, it should be encouraged to get close / in fighting distance
            if (p2CurrentHP >= p1CurrentHP)
            {
                if (distance <= 13000)
                {
                    currScore += ScoreMultiplier;
                }

                else
                {
                    currScore -= ScoreMultiplier;
                }
            }

            //Else fleeing is a valid strategy and it shouldnt lose points
            else
            {
                p1CurrentHP = p1HP;
                p2CurrentHP = p2HP;
            }
        }

        public void LevelDone()
        {
            isInited = false;
            p1CurrentHP = 0;
            p2CurrentHP = 0;
        }

        public IScoreFactor Clone()
        {
            return new IsCloseToEnemyScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
            };
        }
    }
}
