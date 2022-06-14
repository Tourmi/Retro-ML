using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.SuperMarioBros.Neural.Scoring
{
    internal class StopMovingScoreFactor : IScoreFactor
    {
        private const string MAX_IMMOBILE_FRAMES = "Max immobile frames";
        private const string MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED = "Max immobiles frames when moved";

        private int immobileFrames = 0;
        private uint maxXPosition = 0;
        private bool shouldStop = false;
        private int moved = -2;

        private double currScore = 0;

        public FieldInfo[] Fields => new FieldInfo[]
         {
             new IntegerFieldInfo(nameof(MaxImmobileFrames), "Max immobile frames", 1, int.MaxValue, 1),
             new IntegerFieldInfo(nameof(MaxImmobileFramesIfMoved), "Max immobiles frames when moved", 1, int.MaxValue, 1),
         };

        public StopMovingScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(MAX_IMMOBILE_FRAMES, 15),
                new ExtraField(MAX_IMMOBILE_FRAMES_IF_ALREADY_MOVED, 240)
            };
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaxImmobileFrames) => MaxImmobileFrames,
                    nameof(MaxImmobileFramesIfMoved) => MaxImmobileFramesIfMoved,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaxImmobileFrames): MaxImmobileFrames = (int)value; break;
                    case nameof(MaxImmobileFramesIfMoved): MaxImmobileFramesIfMoved = (int)value; break;
                }
            }
        }
        public int MaxImmobileFrames { get; set; } = 15;
        public int MaxImmobileFramesIfMoved { get; set; } = 240;
        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Stopped moving";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SMBDataFetcher)dataFetcher);
        }

        private void Update(SMBDataFetcher dataFetcher)
        {
            uint newPosX = dataFetcher.GetMarioPositionX();

            if (dataFetcher.CanAct() && newPosX <= maxXPosition)
            {
                immobileFrames++;
                if (immobileFrames >= MaxImmobileFrames && moved < 1 || immobileFrames >= MaxImmobileFramesIfMoved)
                {
                    shouldStop = true;
                    currScore += ScoreMultiplier;
                }
            }
            else
            {
                immobileFrames = 0;
                moved++;
            }

            maxXPosition = Math.Max(newPosX, maxXPosition);
        }

        public void LevelDone()
        {
            shouldStop = false;
            moved = -2;
            maxXPosition = 0;
            immobileFrames = 0;
        }

        public IScoreFactor Clone()
        {
            return new StopMovingScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields, MaxImmobileFrames = MaxImmobileFrames, MaxImmobileFramesIfMoved = MaxImmobileFramesIfMoved };
        }
    }
}
