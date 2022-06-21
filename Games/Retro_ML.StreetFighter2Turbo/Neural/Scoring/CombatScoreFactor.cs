﻿using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Scoring
{
    internal class CombatScoreFactor : IScoreFactor
    {
        private bool isInit = false;
        private double currScore;
        private uint player1HP = 0;
        private uint player2HP = 0;

        public FieldInfo[] Fields => Array.Empty<FieldInfo>();

        public CombatScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get => 0;
            set { }
        }

        public bool ShouldStop => false;
        public double ScoreMultiplier { get; set; }

        public string Name => "Combat";

        public string Tooltip => "Reward the ai if it hits the enemy or block an enemy hit and penalize it if it gets hit";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SF2TDataFetcher)dataFetcher);
        }

        private void Update(SF2TDataFetcher dataFetcher)
        {
            var p1HP = dataFetcher.GetPlayer1Hp();
            var p2HP = dataFetcher.GetPlayer1Hp();

            if (!isInit)
            {
                player1HP = p1HP;
                player2HP = p2HP;
                isInit = true;
            }

            //Can be hit at the same time***

            //If player got hit and lost HP and is not considered K.O
            if (p1HP < player1HP && p1HP != 255)
            {
                currScore -= (player1HP - p1HP) * ScoreMultiplier;
                player1HP = p1HP;
                player2HP = p2HP;
            }

            //If player hit ai, and it didnt K.O
            else if (p2HP < player2HP && p2HP != 255)
            {
                currScore += (player2HP - p2HP) * 2 * ScoreMultiplier;
                player1HP = p1HP;
                player2HP = p2HP;
            }
        }

        public void LevelDone()
        {
            isInit = false;
            player1HP = 0;
            player2HP = 0;
        }

        public IScoreFactor Clone()
        {
            return new FightStateScoreFactor()
            {
                IsDisabled = IsDisabled,
                ScoreMultiplier = ScoreMultiplier,
                ExtraFields = ExtraFields
            };
        }
    }
}
