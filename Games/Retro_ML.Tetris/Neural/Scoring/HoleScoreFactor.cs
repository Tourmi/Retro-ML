using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Game;

namespace Retro_ML.Tetris.Neural.Scoring
{
    internal class HoleScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;
        private int nbHoles;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new IntegerFieldInfo(nameof(NumberOfHoles), "Number of holes permitted", 0, 50, 1, "Stops the current level if the AI creates X holes"),
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(NumberOfHoles) => NumberOfHoles,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(NumberOfHoles): NumberOfHoles = (int)value; break;
                }
            }
        }

        public int NumberOfHoles { get; set; } = 5;

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }

        public string Name => "Number of holes";

        public string Tooltip => "Reward applied whenever the AI creates a hole when placing blocks";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((TetrisDataFetcher)dataFetcher);
        }

        private void Update(TetrisDataFetcher dataFetcher)
        {
            nbHoles = dataFetcher.GetNumberOfHoles();

            if (nbHoles > NumberOfHoles)
            {
                shouldStop = true;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
            currScore += ScoreMultiplier * nbHoles;
        }

        public IScoreFactor Clone()
        {
            return new HoleScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, NumberOfHoles = NumberOfHoles };
        }
    }
}
