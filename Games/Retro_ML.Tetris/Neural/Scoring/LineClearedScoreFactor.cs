using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
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

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(SingleScoreMult), "Single Multiplier", double.MinValue, double.MaxValue, 0.25, "Multiplier for clearing a single line"),
             new DoubleFieldInfo(nameof(DoubleScoreMult), "Double Multiplier", double.MinValue, double.MaxValue, 0.25, "Multiplier when clearing 2 lines at the same time"),
             new DoubleFieldInfo(nameof(TripleScoreMult), "Triple Multiplier", double.MinValue, double.MaxValue, 0.25, "Multiplier when clearing 3 lines at the same time"),
             new DoubleFieldInfo(nameof(TetrisScoreMult), "Tetris Multiplier", double.MinValue, double.MaxValue, 0.25, "Multiplier when clearing 4 lines at the same time")
        };

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

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(SingleScoreMult) => SingleScoreMult,
                    nameof(DoubleScoreMult) => DoubleScoreMult,
                    nameof(TripleScoreMult) => TripleScoreMult,
                    nameof(TetrisScoreMult) => TetrisScoreMult,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(SingleScoreMult): SingleScoreMult = (double)value; break;
                    case nameof(DoubleScoreMult): DoubleScoreMult = (double)value; break;
                    case nameof(TripleScoreMult): TripleScoreMult = (double)value; break;
                    case nameof(TetrisScoreMult): TetrisScoreMult = (double)value; break;
                }
            }
        }

        public double SingleScoreMult { get; set; } = 1.0;
        public double DoubleScoreMult { get; set; } = 2.0;
        public double TripleScoreMult { get; set; } = 4.0;
        public double TetrisScoreMult { get; set; } = 16.0;

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Line Cleared";

        public string Tooltip => "Reward applied when the AI clears one or multiple lines";

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
                currScore += ScoreMultiplier * TetrisScoreMult;
            }

            if (dataFetcher.GetTriples() > prevTriple)
            {
                prevTriple = dataFetcher.GetTriples();
                currScore += ScoreMultiplier * TripleScoreMult;
            }

            if (dataFetcher.GetDoubles() > prevDouble)
            {
                prevDouble = dataFetcher.GetDoubles();
                currScore += ScoreMultiplier * DoubleScoreMult;
            }

            if (dataFetcher.GetSingles() > prevSingle)
            {
                prevSingle = dataFetcher.GetSingles();
                currScore += ScoreMultiplier * SingleScoreMult;
            }
        }

        public void LevelDone() => inited = false;

        public IScoreFactor Clone()
        {
            return new LineClearedScoreFactor()
            {
                IsDisabled = IsDisabled,
                ScoreMultiplier = ScoreMultiplier,
                ExtraFields = ExtraFields,
                SingleScoreMult = SingleScoreMult,
                DoubleScoreMult = DoubleScoreMult,
                TripleScoreMult = TripleScoreMult,
                TetrisScoreMult = TetrisScoreMult 
            };
        }
    }
}
