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
    internal class ColumnHeightScoreFactor : IScoreFactor
    {
        internal const string HEIGHT_DIFFERENCE = "Height Difference";

        private bool shouldStop = false;
        private double currScore;
        private bool inited;
        private int prevDiff;
        private int[] prevColumnHeights;

        public ColumnHeightScoreFactor()
        {
            prevColumnHeights = new int[10];
            ExtraFields = new ExtraField[]
            {
                new(HEIGHT_DIFFERENCE, 6)
            };
        }

        public string Name => "Column Height";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            shouldStop = false;
            inited = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((TetrisDataFetcher)dataFetcher);
        }

        private void Update(TetrisDataFetcher dataFetcher)
        {
            if (!inited)
            {
                prevColumnHeights = dataFetcher.GetColumnHeights();
                prevDiff = prevColumnHeights.Max() - prevColumnHeights.Min();
            }

            var currColumnHeights = dataFetcher.GetColumnHeights();
            var currDiff = currColumnHeights.Max() - currColumnHeights.Min();

            if (currDiff > prevDiff && currDiff >= ExtraField.GetValue(ExtraFields, HEIGHT_DIFFERENCE))
            {
                currScore += ScoreMultiplier;
            }

            prevColumnHeights = dataFetcher.GetColumnHeights();
            prevDiff = prevColumnHeights.Max() - prevColumnHeights.Min();
        }

        public IScoreFactor Clone()
        {
            return new ColumnHeightScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
