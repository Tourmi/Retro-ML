using System.Collections.Generic;

namespace SMW_ML.Neural.Training
{
    /// <summary>
    /// Collection of stats for the training session.
    /// </summary>
    public class TrainingStatistics
    {
        public const string CURRENT_GEN = "Current generation";
        public const string BEST_GENOME_FITNESS = "Best genome's fitness";
        public const string BEST_GENOME_COMPLEXITY = "Best genome's complexity";
        public const string MEAN_FITNESS = "Average fitness";
        public const string MEAN_COMPLEXITY = "Average complexity";
        public const string MAX_COMPLEXITY = "Maximum complexity";
        public const string EVALS_PER_MINUTE = "Evaluations per minute";
        public const string TOTAL_EVALUATIONS = "Total evaluations so far";

        public struct Stat
        {
            public string Name { get; set; }
            public double Value { get; set; }
        }

        private List<Stat> stats;

        public TrainingStatistics()
        {
            stats = new List<Stat>();
        }

        /// <summary>
        /// Adds the given stat to the stats collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddStat(string name, double value)
        {
            stats.Add(new Stat()
            {
                Name = name,
                Value = value
            });
        }

        /// <summary>
        /// Returns all of the collected stats.
        /// </summary>
        /// <returns></returns>
        public List<Stat> GetStats()
        {
            return stats;
        }
    }
}
