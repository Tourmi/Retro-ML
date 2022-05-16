using Retro_ML.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.SuperMarioKart.Neural.Scoring
{
    internal class TimeTakenScoreFactor : IScoreFactor
    {
        public const string MAXIMUM_RACE_TIME = "Maximum Race Time";

        private bool shouldStop = false;
        private double currScore;
        private int levelFrames = 0;

        public TimeTakenScoreFactor()
        {
            ExtraFields = new ExtraField[]
            {
                new(MAXIMUM_RACE_TIME, 240)
            };
        }

        public bool ShouldStop => shouldStop;
        public double ScoreMultiplier { get; set; }

        public string Name => "Time taken";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            if (levelFrames >= ExtraField.GetValue(ExtraFields, MAXIMUM_RACE_TIME) * 60)
            {
                shouldStop = true;
            }
            levelFrames++;
            currScore += ScoreMultiplier / 60.0;
        }

        public void LevelDone()
        {
            shouldStop = false;
            levelFrames = 0;
        }

        public IScoreFactor Clone()
        {
            return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
        }
    }
}
