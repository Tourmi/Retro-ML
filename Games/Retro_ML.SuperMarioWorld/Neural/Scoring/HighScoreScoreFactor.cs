using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class HighScoreScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currScore;
        private double score;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public HighScoreScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

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

        public string Name => "High Score";

        public string Tooltip => "The reward to give the AI for its in-game high-score";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currScore = dataFetcher.GetScore();
                isInit = true;
                return;
            }

            int newScore = dataFetcher.GetScore();
            if (newScore > currScore)
            {
                score += (newScore - currScore) * ScoreMultiplier / 100;
            }

            currScore = dataFetcher.GetScore();
        }

        public void LevelDone()
        {
            isInit = false;
        }

        public double GetFinalScore() => score;

        public IScoreFactor Clone()
        {
            return new HighScoreScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
