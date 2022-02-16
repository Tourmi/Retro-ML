using SharpNeat.EvolutionAlgorithm;
using SMW_ML.Neural.Training.SharpNeat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training
{
    /// <summary>
    /// Adapter class that allows training a neural network
    /// </summary>
    public interface INeuralTrainer
    {
        event Action<TrainingStatistics>? OnStatisticsUpdated;
        bool IsTraining { get; }
        void StartTraining(string configPath);
        void StopTraining();

        void LoadPopulation(string path);
        void SavePopulation(string path);
    }
}
