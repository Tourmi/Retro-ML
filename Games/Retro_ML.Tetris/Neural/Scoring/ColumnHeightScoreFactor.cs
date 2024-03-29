﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Game;

namespace Retro_ML.Tetris.Neural.Scoring
{
    internal class ColumnHeightScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;
        private bool inited;
        private int prevDiff;
        private int prevMedian;
        private int[] prevColumnHeights;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new IntegerFieldInfo(nameof(HeightDifference), "Maximum height difference from median", 1, 17, 1, "The difference between the max and median")
        };

        public ColumnHeightScoreFactor() => prevColumnHeights = new int[10];

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(HeightDifference) => HeightDifference,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(HeightDifference): HeightDifference = (int)value; break;
                }
            }
        }

        public int HeightDifference { get; set; } = 5;

        public string Name => "Column Height";

        public string Tooltip => "Reward applied when the AI creates a big gap between the maximum height and the median";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }

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
                prevMedian = prevColumnHeights.OrderBy(i => i).ToArray()[4];
                prevDiff = prevColumnHeights.Max() - prevMedian;
            }

            var currColumnHeights = dataFetcher.GetColumnHeights();
            var currMedian = currColumnHeights.OrderBy(i => i).ToArray()[4];
            var currDiff = currColumnHeights.Max() - currMedian;

            if (currDiff > prevDiff && currDiff >= HeightDifference)
            {
                currScore += ScoreMultiplier;
            }

            prevColumnHeights = currColumnHeights;
            prevMedian = currMedian;
            prevDiff = currDiff;
        }

        public IScoreFactor Clone()
        {
            return new ColumnHeightScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, HeightDifference = HeightDifference };
        }
    }
}
