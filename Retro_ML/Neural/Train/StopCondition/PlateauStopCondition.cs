namespace Retro_ML.Neural.Train.StopCondition
{
    internal class PlateauStopCondition : IStopCondition
    {
        private double highestFitness = 0;
        private int plateauLength = 0;

        public string Name => "Stop when plateau reached";

        public string Tooltip => "Will stop the training session once an AI has stopped improving over the specified amount of generations";

        public bool ShouldUse { get; set; }

        public bool HasParam => true;

        public string ParamName => "Maximum plateau length";

        public int ParamValue { get; set; }

        public void Start()
        {
            highestFitness = 0;
            plateauLength = 0;
        }

        public bool CheckShouldStop(TrainingStatistics stats)
        {
            double currFitness = stats.GetStat(TrainingStatistics.BEST_GENOME_FITNESS);
            if (currFitness > highestFitness)
            {
                plateauLength = 0;
                highestFitness = currFitness;
                return false;
            }

            plateauLength++;
            return plateauLength >= ParamValue;
        }
    }
}
