using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Metroid.Game;
using Retro_ML.Neural.Scoring;
using System.Diagnostics;

namespace Retro_ML.Metroid.Neural.Scoring
{
    internal class ProgressScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool isInit = false;
        private int previousX;
        private int previousY;
        private int framesWithoutProgress;
        private bool wasSamusFrozen;

        public string Name => "Progression";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        private bool shouldStop = false;
        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }
        public double MaximumTimeWithoutProgress { get; set; } = 10.0;
        public bool StopAfterObjectiveReached { get; set; } = false;
        public double RewardOnObjectiveReached { get; set; } = 100.0;

        public ExtraField[] ExtraFields { get; set; }

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new DoubleFieldInfo(nameof(MaximumTimeWithoutProgress), "Maximum time without progress", 0, double.MaxValue, 1.0),
            new BoolFieldInfo(nameof(StopAfterObjectiveReached), "Stop after objective reached"),
            new DoubleFieldInfo(nameof(RewardOnObjectiveReached), "Reward when objective reached", double.MinValue, double.MaxValue, 1.0)
        };

        public ProgressScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(MaximumTimeWithoutProgress) => MaximumTimeWithoutProgress,
                    nameof(StopAfterObjectiveReached) => StopAfterObjectiveReached,
                    nameof(RewardOnObjectiveReached) => RewardOnObjectiveReached,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(MaximumTimeWithoutProgress): MaximumTimeWithoutProgress = (double)value; break;
                    case nameof(StopAfterObjectiveReached): StopAfterObjectiveReached = (bool)value; break;
                    case nameof(RewardOnObjectiveReached): RewardOnObjectiveReached = (double)value; break;
                }
            }
        }

        public double GetFinalScore()
        {
            return currScore;
        }

        public void LevelDone()
        {
            shouldStop = false;
            isInit = false;
        }

        public void Update(IDataFetcher dataFetcher) => Update((MetroidDataFetcher)dataFetcher);

        private void Update(MetroidDataFetcher df)
        {
            byte currX = df.GetSamusXPosition();
            byte currY = df.GetSamusYPosition();

            if (!isInit)
            {
                previousX = currX;
                previousY = currY;

                framesWithoutProgress = 0;

                isInit = true;
            }

            double distX = currX - previousX;
            double distY = currY - previousY;
            //correct the distance if there was an overflow/underflow
            if (distX > 128) distX -= byte.MaxValue;
            if (distX < 128) distX += byte.MaxValue;
            if (distY > 128) distY -= byte.MaxValue;
            if (distY < 128) distY += byte.MaxValue;

            var navigationDirection = df.GetNavigationDirection();
            var scoreDelta = ScoreMultiplier / 16.0 * (distX * navigationDirection[0, 0] + distY * navigationDirection[0, 1]);
            currScore += scoreDelta;

            Debug.WriteLine(currScore);

            if (df.CanSamusAct())
            {
                if (scoreDelta <= 0)
                {
                    framesWithoutProgress++;
                }
                else
                {
                    framesWithoutProgress = 0;
                }

                wasSamusFrozen = false;
            }
            else
            {
                if (df.IsSamusFrozenAfterObjective() && !wasSamusFrozen)
                {
                    ScoreMultiplier += RewardOnObjectiveReached;
                    shouldStop = StopAfterObjectiveReached;
                    wasSamusFrozen = true;
                }
            }

            if (MaximumTimeWithoutProgress != 0 && framesWithoutProgress > MaximumTimeWithoutProgress * 60.0)
            {
                shouldStop = true;
            }

            previousX = currX;
            previousY = currY;
        }

        public IScoreFactor Clone()
        {
            return new ProgressScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                RewardOnObjectiveReached = RewardOnObjectiveReached,
                StopAfterObjectiveReached = StopAfterObjectiveReached,
                MaximumTimeWithoutProgress = MaximumTimeWithoutProgress
            };
        }
    }
}
