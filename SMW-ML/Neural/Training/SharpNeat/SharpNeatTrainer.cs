using SharpNeat.Experiments;
using SharpNeat.IO;
using SharpNeat.Neat;
using SharpNeat.Neat.EvolutionAlgorithm;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Training.SharpNeat;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
        public SharpNeatTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig)
        {
            syncSemaphore = new Semaphore(1, 1);
            this.emulatorManager = emulatorManager;
            experimentFactory = new SMWExperimentFactory(emulatorManager, appConfig);

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

                SaveBestGenome(DefaultPaths.CURRENT_GENOME);
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

            ts.AddStat(TrainingStatistics.CURRENT_GEN, currentAlgo!.Stats.Generation + 1);
            ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, currentAlgo!.Population.Stats.BestFitness.PrimaryFitness);
            ts.AddStat(TrainingStatistics.BEST_GENOME_COMPLEXITY, currentAlgo!.Population.Stats.BestComplexity);
            ts.AddStat(TrainingStatistics.MEAN_FITNESS, currentAlgo!.Population.Stats.MeanFitness);
            ts.AddStat(TrainingStatistics.MEAN_COMPLEXITY, currentAlgo!.Population.Stats.MeanComplexity);
            ts.AddStat(TrainingStatistics.MAX_COMPLEXITY, currentAlgo!.Population!.Stats.MaxComplexity);
            ts.AddStat(TrainingStatistics.EVALS_PER_MINUTE, currentAlgo!.Stats.EvaluationsPerSec * 60);
            ts.AddStat(TrainingStatistics.TOTAL_EVALUATIONS, currentAlgo!.Stats.TotalEvaluationCount);

            return ts;
        }

        public void SaveBestGenome(string path)
        {
            path = Path.GetFullPath(path);

            if (File.Exists(path))
            {
                File.Copy(path, path + ".backup", true);
                File.Delete(path);
            }

            NeatGenomeSaver<double>.Save(currentAlgo!.Population.BestGenome, path);
        }
    }
}
