using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Game;
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

        public HoleScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new(NUMBER_OF_HOLES, 3)
            };
        }

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
            var nbHoles = dataFetcher.GetNumberOfHoles();

            currScore += ScoreMultiplier;
            if (nbHoles >= ExtraField.GetValue(ExtraFields, NUMBER_OF_HOLES))
            {
                shouldStop = true;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new HoleScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
