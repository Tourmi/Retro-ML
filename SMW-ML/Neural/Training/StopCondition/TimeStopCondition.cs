using System;

namespace SMW_ML.Neural.Training.StopCondition
{
    internal class TimeStopCondition : IStopCondition
    {
        private DateTime? endTime;

        public string Name => "Stop after X minutes";

        public bool ShouldUse { get; set; }

        public bool HasParam => true;

        public string ParamName => "Minutes";

        public int ParamValue { get; set; }

        public void Start()
        {
            endTime = DateTime.Now + TimeSpan.FromMinutes(ParamValue);
        }

        public bool CheckShouldStop(TrainingStatistics stats) => DateTime.Now >= endTime;
    }
}
