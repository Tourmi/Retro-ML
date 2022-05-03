namespace Retro_ML.Neural.Training.StopCondition
{
    internal class FitnessStopCondition : IStopCondition
    {
        public string Name => "Stop when fitness reached";

        public bool ShouldUse { get; set; }

        public bool HasParam => true;

        public string ParamName => "Target fitness";

        public int ParamValue { get; set; }

        public void Start() { }

        public bool CheckShouldStop(TrainingStatistics stats) => stats.GetStat(TrainingStatistics.BEST_GENOME_FITNESS) >= ParamValue;
    }
}
