﻿using System;
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
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.Neat.Genome;
using SharpNeat.NeuralNets.Double.ActivationFunctions;
using SMW_ML.Utils;
using SharpNeat.EvolutionAlgorithm;
using SMW_ML.Neural.Training.SharpNeat;

namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    internal class SharpNeatTrainer : INeuralTrainer
    {
        public event Action<TrainingStatistics>? OnStatisticsUpdated;

        private readonly SMWExperimentFactory experimentFactory;
        private readonly Semaphore syncSemaphore;
        private readonly EmulatorManager emulatorManager;

        private INeatExperiment<double>? currentExperiment;
        private NeatEvolutionAlgorithm<double>? currentAlgo;
        private bool stopFlag = false;

        private MetaNeatGenome<double> metaGenome;
        private INeatGenomeBuilder<double> genomeBuilder;
        private List<NeatGenome<double>> genomes;

        private bool isTraining = false;

        public bool IsTraining => isTraining;

        /// <summary>
        /// Neural training using the SharpNEAT library
        /// </summary>
        public SharpNeatTrainer(EmulatorManager emulatorManager)
        {
            syncSemaphore = new Semaphore(1, 1);
            this.emulatorManager = emulatorManager;
            experimentFactory = new SMWExperimentFactory(emulatorManager);

            metaGenome = new MetaNeatGenome<double>(
                   inputNodeCount: emulatorManager.GetInputCount(),
                   outputNodeCount: emulatorManager.GetOutputCount(),
                   isAcyclic: true,
                   activationFn: new LeakyReLU());
            genomeBuilder = NeatGenomeBuilderFactory<double>.Create(metaGenome);
            genomes = new List<NeatGenome<double>>();
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
            currentExperiment.ActivationFnName = nameof(LeakyReLU);
            currentExperiment.IsAcyclic = true;
            stopFlag = false;

            if (genomes.Any())
            {
                currentAlgo = NeatUtils.CreateNeatEvolutionAlgorithm(currentExperiment, new NeatPopulation<double>(metaGenome, genomeBuilder, genomes));
            }
            else
            {
                currentAlgo = NeatUtils.CreateNeatEvolutionAlgorithm(currentExperiment);
            }

            new Thread(Training).Start();
        }

        public void StopTraining()
        {
            stopFlag = true;
            syncSemaphore.WaitOne();
            syncSemaphore.Release();
        }

        private void Training()
        {
            isTraining = true;

            syncSemaphore.WaitOne();
            OnStatisticsUpdated?.Invoke(GetTrainingStatistics());
            currentAlgo!.Initialise();
            SavePopulation(DefaultPaths.CURRENT_POPULATION);

            while (!stopFlag)
            {
                currentAlgo!.PerformOneGeneration();

                OnStatisticsUpdated?.Invoke(GetTrainingStatistics());

                SavePopulation(DefaultPaths.CURRENT_POPULATION);
            }

            syncSemaphore.Release();
            isTraining = false;
        }

        public void LoadPopulation(string path)
        {
            NeatPopulationLoader<double> npl = new(NeatGenomeLoaderFactory.CreateLoaderDouble(metaGenome));
            genomes = npl.LoadFromZipArchive(path);
        }

        public void SavePopulation(string path)
        {
            path = Path.GetFullPath(path);
            string filename = Path.GetFileName(path);

            if (File.Exists(path))
            {
                File.Copy(path, path + ".backup", true);
                File.Delete(path);
            }

            genomes = currentAlgo!.Population.GenomeList;

            NeatPopulationSaver<double>.SaveToZipArchive(genomes, path[..path.IndexOf(filename)], filename, System.IO.Compression.CompressionLevel.Fastest);
        }

        public TrainingStatistics GetTrainingStatistics()
        {
            TrainingStatistics ts = new();

            ts.AddStat("Current Generation", currentAlgo!.Stats.Generation + 1);
            ts.AddStat("Best Genome's Fitness", currentAlgo!.Population.Stats.BestFitness.PrimaryFitness);
            ts.AddStat("Best Genome's Complexity", currentAlgo!.Population.Stats.BestComplexity);
            ts.AddStat("Mean Fitness", currentAlgo!.Population.Stats.MeanFitness);
            ts.AddStat("Mean Complexity", currentAlgo!.Population.Stats.MeanComplexity);
            ts.AddStat("Maximum Complexity", currentAlgo!.Population!.Stats.MaxComplexity);
            ts.AddStat("Evaluations per second", currentAlgo!.Stats.EvaluationsPerSec);
            ts.AddStat("Total evaluations so far", currentAlgo!.Stats.TotalEvaluationCount);

            return ts;
        }
    }
}
