﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Game;


namespace Retro_ML.SuperMarioWorld.Neural.Scoring
{
    internal class OneUpsScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private int currOneUps;
        private double score;

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

        public string Name => "1-ups";

        public string Tooltip => "Reward to give the AI per 1-up collected from any source";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public bool ShouldStop => false;

        public double ScoreMultiplier { get; set; }

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMWDataFetcher)dataFetcher);
        }

        private void Update(SMWDataFetcher dataFetcher)
        {
            if (!isInit)
            {
                currOneUps = dataFetcher.GetLives();
                isInit = true;
                return;
            }

            int newOneUps = dataFetcher.GetLives();
            if (newOneUps > currOneUps)
            {
                score += (newOneUps - currOneUps) * ScoreMultiplier;
            }

            currOneUps = dataFetcher.GetLives();
        }

        public void LevelDone()
        {
            isInit = false;
        }

        public double GetFinalScore() => score;

        public IScoreFactor Clone()
        {
            return new OneUpsScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
        }
    }
}
