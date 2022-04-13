using System.Collections.Generic;
using System.Linq;

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

        private readonly List<Stat> stats;

        public TrainingStatistics()
        {
            stats = new();
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
        /// Returns the specified stat.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetStat(string name) => stats.Where(s => s.Name == name).Single().Value;

        /// <summary>
        /// Returns all of the collected stats.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stat> GetStats()
        {
            return stats.AsEnumerable();
        }
    }
}
