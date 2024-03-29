﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class WonLevelScoreFactor : IScoreFactor
    {
        private bool shouldStop = false;
        private double currScore;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Won level";

        public string Tooltip => "Reward to give if the AI wins a level. Ideally, this should be a high value to encourage actually finishing levels";

        public bool CanBeDisabled => false;

        public bool IsDisabled { get => false; set { } }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
        {
            if (dataFetcher.WonLevel())
            {
                shouldStop = true;
                currScore += ScoreMultiplier;
            }
        }

        public void LevelDone()
        {
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new WonLevelScoreFactor() { ScoreMultiplier = ScoreMultiplier };
        }
    }
}
