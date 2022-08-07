using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class StopFightingScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool shouldStop = false;
        private bool isInited = false;
        private uint p2CurrentHP = 0;
        private uint innactiveFrames = 0;

        public FieldInfo[] Fields => new FieldInfo[]
         {
             new IntegerFieldInfo(nameof(MaxInnactiveFrames), "Maximum inactive frames", 1, int.MaxValue, 1, "The maximum amount of consecutive frames where the AI isn't landing blows before stopping the current training."),
         };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaxInnactiveFrames) => MaxInnactiveFrames,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaxInnactiveFrames): MaxInnactiveFrames = (int)value; break;
                }
            }
        }
        public int MaxInnactiveFrames { get; set; } = 300;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Stop Fighting";

        public string Tooltip => "Penalize the ai if it doesnt hit the enemy for a defined amount of frames";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SF2TDataFetcher)dataFetcher);
        }

        private void Update(SF2TDataFetcher dataFetcher)
        {
            var p2HP = dataFetcher.GetPlayer2Hp();

            if (!isInited)
            {
                p2CurrentHP = p2HP;
                innactiveFrames = 0;
                isInited = true;
            }

            if (!dataFetcher.IsPlayer1Staggered())
            {
                //If the player can attack and has not attacked in a certain time frame.
                if (p2CurrentHP == p2HP)
                {
                    innactiveFrames++;
                    if (innactiveFrames >= MaxInnactiveFrames)
                    {
                        shouldStop = true;
                        currScore += ScoreMultiplier;
                    }
                }

                else
                {
                    innactiveFrames = 0;
                }
            }

            p2CurrentHP = p2HP;
        }

        public void LevelDone()
        {
            isInited = false;
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new StopFightingScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                MaxInnactiveFrames = MaxInnactiveFrames
            };
        }
    }
}
