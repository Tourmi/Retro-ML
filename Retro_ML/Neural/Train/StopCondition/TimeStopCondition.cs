﻿namespace Retro_ML.Neural.Train.StopCondition
{
    internal class TimeStopCondition : IStopCondition
    {
        private DateTime? endTime;

        public string Name => "Stop after X minutes";

        public string Tooltip => "Will stop the training session after the specified amount of minutes";

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
