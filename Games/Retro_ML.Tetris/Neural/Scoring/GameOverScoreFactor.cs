using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Game;

namespace Retro_ML.Tetris.Neural.Scoring
{
    internal class GameOverScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                }
            }
        }

        public string Name => "Game Over";

        public string Tooltip => "Reward when the AI is game over";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }

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
