using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class EndRoundScoreFactor : IScoreFactor
    {
        private const uint MAX_HP = 176;
        private bool shouldStop = false;
        private double currScore;
        private ulong maxTime = 0;
        private bool isInited = false;

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new DoubleFieldInfo(nameof(DefeatMultiplier), "Defeat Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier in case of a defeat"),
            new DoubleFieldInfo(nameof(VictoryMultiplier), "Victory Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier in case of a victory"),
            new DoubleFieldInfo(nameof(DrawMultiplier), "Draw Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier in case of a draw"),
        };

        public EndRoundScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(DefeatMultiplier) => DefeatMultiplier,
                    nameof(VictoryMultiplier) => VictoryMultiplier,
                    nameof(DrawMultiplier) => DrawMultiplier,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(DefeatMultiplier): DefeatMultiplier = (double)value; break;
                    case nameof(VictoryMultiplier): VictoryMultiplier = (double)value; break;
                    case nameof(DrawMultiplier): DrawMultiplier = (double)value; break;
                }
            }
        }

        public string Tooltip => "The reward to give an AI at the end of the round depending on the result.";

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }
        public double VictoryMultiplier { get; set; } = 1;
        public double DefeatMultiplier { get; set; } = -0.1;
        public double DrawMultiplier { get; set; } = 0;

        public string Name => "End of Round";

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
            if (dataFetcher.isRoundOver())
            {
                shouldStop = true;
                //If the round is finished via Player 1 K.O, penalize the AI depending on the time he survived.
                if (dataFetcher.hasPlayerLost())
                {
                    currScore += ScoreMultiplier * DefeatMultiplier;
                }

                //If the round is finished via Player 2 K.O, reward ai for killing the enemy depending on time used
                if (!dataFetcher.hasPlayerWon())
                {
                    currScore += ScoreMultiplier * VictoryMultiplier;
                }

                //If the round is finished via Draw, reward ai for killing the enemy depending on time used
                if (dataFetcher.isRoundDraw())
                {
                    currScore += ScoreMultiplier * DrawMultiplier;
                }
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
            return new EndRoundScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                DefeatMultiplier = DefeatMultiplier,
                VictoryMultiplier = VictoryMultiplier,
                DrawMultiplier = DrawMultiplier
            };
        }
    }
}
