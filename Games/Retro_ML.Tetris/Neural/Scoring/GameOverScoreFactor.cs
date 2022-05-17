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
    internal class GameOverScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public GameOverScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public string Name => "Game Over";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void LevelDone()
        {
            shouldStop = false;
        }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((TetrisDataFetcher)dataFetcher);
        }

        private void Update(TetrisDataFetcher dataFetcher)
        {
            if (dataFetcher.IsGameOver())
            {
                shouldStop = true;
                currScore += ScoreMultiplier;
                return;
            }
        }

        public IScoreFactor Clone()
        {
            return new GameOverScoreFactor() { ScoreMultiplier = ScoreMultiplier };
        }
    }
}
