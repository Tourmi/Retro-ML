using SharpNeat.EvolutionAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training.SharpNeat
{
    public class TrainingStatistics
    {
        public struct Stat
        {
            public string Name;
            public double value;
        }

        private List<Stat> stats;

        /// <summary>
        /// SharpNeat statistics using the SharpNEAT library
        /// </summary>
        public TrainingStatistics()
        {
            stats = new List<Stat>();
        }

        public void AddStat(string name, double value)
        {
            stats.Add(new Stat()
            {
                Name = name,
                value = value
            });
        }
    }
}
