using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class FightStateScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool shouldStop = false;
        private bool isInited = false;
        private uint p1CurrentPos = 0;
        private uint p1CurrentHP = 0;
        private uint p2CurrentHP = 0;
        private uint immobileFrames = 0;
        private uint innactiveFrames = 0;

        public FieldInfo[] Fields => new FieldInfo[]
         {
             new IntegerFieldInfo(nameof(MaxImmobileFrames), "Max immobile frames when the ai is not moving", 1, int.MaxValue, 1),
             new IntegerFieldInfo(nameof(MaxInnactiveFrames), "Max innactive frames when the ai is not landing blows", 1, int.MaxValue, 1),
         };

        public FightStateScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaxImmobileFrames) => MaxImmobileFrames,
                    nameof(MaxInnactiveFrames) => MaxInnactiveFrames,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaxImmobileFrames): MaxImmobileFrames = (int)value; break;
                    case nameof(MaxInnactiveFrames): MaxInnactiveFrames = (int)value; break;
                }
            }
        }
        public int MaxImmobileFrames { get; set; } = 300;
        public int MaxInnactiveFrames { get; set; } = 450;

        public bool ShouldStop => shouldStop;
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
            var p1Pos = dataFetcher.GetPlayer1XPos();
            var p1HP = dataFetcher.GetPlayer1Hp();
            var p2HP = dataFetcher.GetPlayer2Hp();

            if (!isInited)
            {
                p1CurrentPos = p1Pos;
                p1CurrentHP = p1HP;
                p2CurrentHP = p2HP;
                immobileFrames = 0;
                innactiveFrames = 0;
            }

            //If the player can move and has not moved yet
            if (!dataFetcher.isPlayer1Staggered() && p1CurrentPos == p1Pos)
            {
                immobileFrames++;
                if (immobileFrames >= MaxImmobileFrames)
                {
                    shouldStop = true;
                    currScore += ScoreMultiplier;
                }

                else
                {
                    immobileFrames = 0;
                    p1CurrentPos = p1Pos;
                }
            }

            //If the player can attack and has not attacked in a certain time frame
            if (!dataFetcher.isPlayer1Staggered() && p2CurrentHP == p2HP)
            {
                innactiveFrames++;
                if (innactiveFrames >= MaxInnactiveFrames)
                {
                    shouldStop = true;
                    currScore += ScoreMultiplier;
                }

                else
                {
                    innactiveFrames = 0;
                    p2CurrentHP = p2HP;
                }
            }
        }

        public void LevelDone()
        {
            isInited = false;
            p1CurrentPos = 0;
            p1CurrentHP = 0;
            p2CurrentHP = 0;
            immobileFrames = 0;
            innactiveFrames = 0;
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new FightStateScoreFactor()
            {
                IsDisabled = IsDisabled,
                ScoreMultiplier = ScoreMultiplier,
                MaxImmobileFrames = MaxImmobileFrames,
                MaxInnactiveFrames = MaxInnactiveFrames
            };
        }
    }
}
