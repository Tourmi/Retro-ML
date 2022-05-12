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
    internal class LineClearedScoreFactor : IScoreFactor
    {
        internal const string SINGLE_MULT = "Single";
        internal const string DOUBLE_MULT = "Double";
        internal const string TRIPLE_MULT = "Triple";
        internal const string TETRIS_MULT = "Tetris";

        private double currScore;
        private bool inited;
        private uint prevSingle;
        private uint prevDouble;
        private uint prevTriple;
        private uint prevTetris;

        public LineClearedScoreFactor()
        {
            ExtraFields = new ExtraField[]
          {
                new ExtraField(SINGLE_MULT, 1.0),
                new ExtraField(DOUBLE_MULT, 2.0),
                new ExtraField(TRIPLE_MULT, 4.0),
                new ExtraField(TETRIS_MULT, 16.0)
          };
        }

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Line Cleared";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((TetrisDataFetcher)dataFetcher);
        }

        private void Update(TetrisDataFetcher dataFetcher)
        {
            if (!inited)
            {
                inited = true;
                prevSingle = dataFetcher.GetSingles();
                prevDouble = dataFetcher.GetDoubles();
                prevTriple = dataFetcher.GetTriples();
                prevTetris = dataFetcher.GetTetrises();
            }

            if (dataFetcher.GetTetrises() > prevTetris)
            {
                prevTetris = dataFetcher.GetTetrises();
                currScore += ScoreMultiplier * ExtraField.GetValue(ExtraFields, TETRIS_MULT);
            }

            if (dataFetcher.GetTriples() > prevTriple)
            {
                prevTriple = dataFetcher.GetTriples();
                currScore += ScoreMultiplier * ExtraField.GetValue(ExtraFields, TRIPLE_MULT);
            }

            if (dataFetcher.GetDoubles() > prevDouble)
            {
                prevDouble = dataFetcher.GetDoubles();
                currScore += ScoreMultiplier * ExtraField.GetValue(ExtraFields, DOUBLE_MULT);
            }

            if (dataFetcher.GetSingles() > prevSingle)
            {
                prevSingle = dataFetcher.GetSingles();
                currScore += ScoreMultiplier * ExtraField.GetValue(ExtraFields, SINGLE_MULT);
            }
        }

        public void LevelDone() => inited = false;

        public IScoreFactor Clone()
        {
            return new LineClearedScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
