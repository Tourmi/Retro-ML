namespace Retro_ML.Neural.Train.StopCondition
{
    internal class GenerationCountStopCondition : IStopCondition
    {
        public string Name => "Stop after X generations";

        public string Tooltip => "Will stop the training session once the specified amount of generations is reached";

        public bool ShouldUse { get; set; }

        public bool HasParam => true;

        public string ParamName => "Generations";

        public int ParamValue { get; set; }

        public void Start() { }

        public bool CheckShouldStop(TrainingStatistics stats) => stats.GetStat(TrainingStatistics.CURRENT_GEN) >= ParamValue;
    }
}
