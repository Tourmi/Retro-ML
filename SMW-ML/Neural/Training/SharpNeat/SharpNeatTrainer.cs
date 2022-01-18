using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SharpNeat.Experiments;
using SharpNeat.IO;
using SharpNeat.Neat;
using System.Threading;
using SharpNeat.Neat.EvolutionAlgorithm;
using SMW_ML.Emulator;

namespace SMW_ML.Neural.Training.SharpNeat
{
    internal class SharpNeatTrainer : INeuralTrainer
    {
        private readonly SMWExperimentFactory experimentFactory;
        private readonly Semaphore syncSemaphore;

        private INeatExperiment<double>? currentExperiment;
        private NeatEvolutionAlgorithm<double>? currentAlgo;
        private bool stopFlag = false;

        public bool IsTraining => currentExperiment != null || currentAlgo != null;

        /// <summary>
        /// Neural training using the SharpNEAT library
        /// </summary>
        public SharpNeatTrainer(IEmulatorAdapter emulator)
        {
            syncSemaphore = new Semaphore(1, 1);
            experimentFactory = new SMWExperimentFactory(emulator);
        }

        /// <summary>
        /// Starts the training of AIs based on the given config file
        /// </summary>
        /// <param name="configPath">configuration file for the experiment</param>
        /// <exception cref="InvalidOperationException">The experiment may not be started twice</exception>
        public void StartTraining(string configPath)
        {
            if (IsTraining)
            {
                throw new InvalidOperationException("An experiment is already ongoing.");
            }

            currentExperiment = experimentFactory.CreateExperiment(JsonUtils.LoadUtf8(configPath).RootElement);
            currentAlgo = NeatUtils.CreateNeatEvolutionAlgorithm(currentExperiment);

            stopFlag = false;

            currentAlgo.Initialise();
            new Thread(Training).Start();
        }

        public void StopTraining()
        {
            stopFlag = true;
            syncSemaphore.WaitOne();

            currentAlgo = null;
            currentExperiment = null;

            syncSemaphore.Release();
        }

        private void Training()
        {
            syncSemaphore.WaitOne();
            while (!stopFlag)
            {
                currentAlgo!.PerformOneGeneration();
            }
            syncSemaphore.Release();
        }
    }
}
