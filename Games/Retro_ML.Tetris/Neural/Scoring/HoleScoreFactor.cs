using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Game;
using Retro_ML.Configuration.FieldInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Tetris.Neural.Scoring
{
    internal class HoleScoreFactor : IScoreFactor
    {
        public const string NUMBER_OF_HOLES = "Number of holes permitted";

        private bool shouldStop = false;
        private double currScore;
        private int nbHoles;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new IntegerFieldInfo(nameof(NumberOfHoles), "Number of holes permitted", 0, 50, 1),
        };

        public HoleScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new(NUMBER_OF_HOLES, 3)
            };
        }

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

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

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
            return new HoleScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields, NumberOfHoles = NumberOfHoles };
        }
    }
}
