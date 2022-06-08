using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class StoppedProgressingScoreFactor : IScoreFactor
    {
        public const string MAX_TIME_WITHOUT_PROGRESS = "Max time w/o progress";

        private bool init = false;
        private bool shouldStop = false;
        private int framesWithoutCheckpoint;
        private double currScore;
        private int maxCheckpoint = 0;
        private int currCheckpoint = 0;
        private int previousCheckpoint = 0;

        public FieldInfo[] Fields => new FieldInfo[]
        {
             new DoubleFieldInfo(nameof(MaxTimeWithoutProgress), "Max time w/o progress", 1.0, double.MaxValue, 0.5)
        };

        public StoppedProgressingScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new ExtraField(MAX_TIME_WITHOUT_PROGRESS, 4)
            };
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaxTimeWithoutProgress) => MaxTimeWithoutProgress,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaxTimeWithoutProgress): MaxTimeWithoutProgress = (int)value; break;
                }
            }
        }

        public double MaxTimeWithoutProgress { get; set; } = 4;

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Stopped progressing";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            var df = (SMKDataFetcher)dataFetcher;
            if (!init)
            {
                init = true;
                framesWithoutCheckpoint = 0;
                maxCheckpoint = df.GetMaxCheckpoint();
                previousCheckpoint = df.GetCurrentCheckpoint() + df.GetCurrentLap() * maxCheckpoint;
            }

            currCheckpoint = df.GetCurrentCheckpoint() + df.GetCurrentLap() * maxCheckpoint;

            if (currCheckpoint > previousCheckpoint)
            {
                framesWithoutCheckpoint = 0;
                previousCheckpoint = currCheckpoint;
            }
            else if (df.GetRaceStatus() == 0x06)
            {
                framesWithoutCheckpoint++;
            }

            if (framesWithoutCheckpoint >= MaxTimeWithoutProgress * 60)
            {
                currScore += ScoreMultiplier;
                shouldStop = true;
            }
        }

        public void LevelDone()
        {
            init = false;
            shouldStop = false;
        }

        public IScoreFactor Clone()
        {
            return new StoppedProgressingScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields, MaxTimeWithoutProgress = MaxTimeWithoutProgress };
        }
    }
}
