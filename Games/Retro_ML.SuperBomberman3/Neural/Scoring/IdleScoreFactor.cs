using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.SuperBomberman3.Game;

namespace Retro_ML.SuperBomberMan3.Neural.Scoring
{
    internal class IdleScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool shouldStop = false;
        private bool isInit = false;
        private int idleFrames = 0;
        private byte previousXPos = 0;
        private byte previousYPos = 0;

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new DoubleFieldInfo(nameof(MaxIdleFrames), "Max idle frames", double.MinValue, double.MaxValue, 1, "Maximum amount of frames where the player can stay idle"),
        };

        public IdleScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaxIdleFrames) => MaxIdleFrames,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaxIdleFrames): MaxIdleFrames = (double)value; break;
                }
            }
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public double MaxIdleFrames { get; set; } = 600;

        public string Name => "Max idle frames";

        public string Tooltip => "Penalize the ai if it stays idle (do not move) for a certain amount of frames";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SB3DataFetcher)dataFetcher);
        }

        private void Update(SB3DataFetcher dataFetcher)
        {
            var xPos = dataFetcher.GetPlayersXPos()[0];
            var yPos = dataFetcher.GetPlayersYPos()[0];

            if (!isInit)
            {
                isInit = true;
                idleFrames = 0;
                previousXPos = xPos;
                previousYPos = yPos;
            }

            //If player has not moved compared to the last frame
            if (previousXPos == xPos && previousYPos == yPos)
            {
                //Check if the max idle frame count is reached
                if (idleFrames >= MaxIdleFrames)
                {
                    currScore += ScoreMultiplier;
                    shouldStop = true;
                }

                //If not, increment the idle frames count
                idleFrames++;
            }

            //Else reset the idle frames count
            else
            {
                idleFrames = 0;
            }

            previousXPos = xPos;
            previousYPos = yPos;
        }

        public void LevelDone()
        {
            shouldStop = false;
            isInit = false;
            idleFrames = 0;
            previousXPos = 0;
            previousYPos = 0;
        }

        public IScoreFactor Clone()
        {
            return new IdleScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                MaxIdleFrames = MaxIdleFrames,
            };
        }
    }
}
