using SMW_ML.Neural.Training.SharpNeat;
using System;

namespace SMW_ML.Neural.Training
{
    /// <summary>
    /// Adapter class that allows training a neural network
    /// </summary>
    public interface INeuralTrainer
    {
        /// <summary>
        /// Event that's called whenever new stats are available
        /// </summary>
        event Action<TrainingStatistics>? OnStatisticsUpdated;
        /// <summary>
        /// Whether or not the trainer is currently running
        /// </summary>
        bool IsTraining { get; }
        /// <summary>
        /// Whether or not the user wants to force stop the training
        /// </summary>
        bool ForceStop { get; set; }
        /// <summary>
        /// Starts training the AIs.
        /// </summary>
        /// <param name="configPath"></param>
        void StartTraining(string configPath);
        /// <summary>
        /// Stops the training of AIs.
        /// </summary>
        void StopTraining();

        /// <summary>
        /// Loads the population which will be used as a training starting point.
        /// </summary>
        /// <param name="path"></param>
        void LoadPopulation(string path);
        /// <summary>
        /// Saves the trained population to the given path.
        /// </summary>
        /// <param name="path"></param>
        void SavePopulation(string path);
    }
}
