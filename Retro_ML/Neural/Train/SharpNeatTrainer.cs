using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train.StopCondition;
using Retro_ML.Utils;
using SharpNeat.Experiments;
using SharpNeat.IO;
using SharpNeat.Neat;
using SharpNeat.Neat.EvolutionAlgorithm;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace Retro_ML.Neural.Train
{
    /// <summary>
    /// Neural trainer that uses the SharpNeat library for its training
    /// </summary>
    public abstract class SharpNeatTrainer : INeuralTrainer
    {
        public event Action<TrainingStatistics>? OnStatisticsUpdated;
        public event Action? OnStopConditionReached;

        protected INeatExperimentFactory? experimentFactory;
        protected readonly Mutex syncMutex;
        protected readonly EmulatorManager emulatorManager;
        protected readonly ApplicationConfig applicationConfig;

        protected string? trainingDirectory;
        protected double previousFitness;

        protected INeatExperiment<double>? currentExperiment;
        protected NeatEvolutionAlgorithm<double>? currentAlgo;
        protected bool stopFlag = false;

        protected MetaNeatGenome<double> metaGenome;
        protected INeatGenomeBuilder<double> genomeBuilder;
        protected List<NeatGenome<double>> genomes;

        protected bool isTraining = false;

        public bool IsTraining => isTraining;

        protected bool forceStop = false;
        public bool ForceStop
        {
            get => forceStop;
            set => forceStop = value;
        }

        public ICollection<IStopCondition> StopConditions { private get; set; }

        /// <summary>
        /// Neural training using the SharpNEAT library
        /// </summary>
        protected SharpNeatTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig)
        {
            syncMutex = new Mutex();
            this.emulatorManager = emulatorManager;
            applicationConfig = appConfig;
            metaGenome = new MetaNeatGenome<double>(
                   inputNodeCount: appConfig.NeuralConfig.GetInputCount(),
                   outputNodeCount: appConfig.NeuralConfig.GetOutputCount(),
                   isAcyclic: true,
                   activationFn: new LeakyReLU());
            genomeBuilder = NeatGenomeBuilderFactory<double>.Create(metaGenome);
            genomes = new List<NeatGenome<double>>();
            StopConditions = appConfig.StopConditions;
        }

        /// <summary>
        /// Starts the training of AIs based on the given config file
        /// </summary>
        /// <param name="configPath">configuration file for the experiment</param>
        /// <exception cref="InvalidOperationException">The experiment may not be started twice</exception>
        public virtual void StartTraining(string configPath)
        {
            if (experimentFactory == null)
            {
                throw new InvalidOperationException("No experiment factories were provided");
            }
            if (IsTraining)
            {
                throw new InvalidOperationException("An experiment is already ongoing.");
            }

            trainingDirectory = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "/";
            Directory.CreateDirectory(trainingDirectory + "/" + DefaultPaths.GENOME_DIR);

            string neuralConfig = applicationConfig.NeuralConfig.Serialize();
            File.WriteAllText(Path.Combine(trainingDirectory, DefaultPaths.NEURAL_CONFIG_NAME), neuralConfig);

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

        public virtual void StopTraining()
        {
            stopFlag = true;
            syncMutex.WaitOne();
            syncMutex.ReleaseMutex();
        }

        public virtual void LoadPopulation(string path)
        {
            try
            {
                NeatPopulationLoader<double> npl = new(NeatGenomeLoaderFactory.CreateLoaderDouble(metaGenome));
                genomes = npl.LoadFromZipArchive(path);
            }
            catch
            {
                Exceptions.QueueException(new Exception("Unable to load population. Is the population compatible with the current Neural Network Configuration?"));
            }
        }

        public virtual TrainingStatistics GetTrainingStatistics()
        {
            TrainingStatistics ts = new();

            ts.AddStat(TrainingStatistics.CURRENT_GEN, currentAlgo!.Stats.Generation);
            ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, currentAlgo!.Population.Stats.BestFitness.PrimaryFitness);
            ts.AddStat(TrainingStatistics.BEST_GENOME_COMPLEXITY, currentAlgo!.Population.Stats.BestComplexity);
            ts.AddStat(TrainingStatistics.MEAN_FITNESS, currentAlgo!.Population.Stats.MeanFitness);
            ts.AddStat(TrainingStatistics.MEAN_COMPLEXITY, currentAlgo!.Population.Stats.MeanComplexity);
            ts.AddStat(TrainingStatistics.MAX_COMPLEXITY, currentAlgo!.Population!.Stats.MaxComplexity);
            ts.AddStat(TrainingStatistics.EVALS_PER_MINUTE, currentAlgo!.Stats.EvaluationsPerSec * 60);
            ts.AddStat(TrainingStatistics.TOTAL_EVALUATIONS, currentAlgo!.Stats.TotalEvaluationCount);

            return ts;
        }

        public virtual void SavePopulation(string path)
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

        public virtual void SaveBestGenome()
        {
            var bestGenome = currentAlgo!.Population.BestGenome;
            var path = Path.GetFullPath(Path.Combine(trainingDirectory!, DefaultPaths.GENOME_DIR, DefaultPaths.CURRENT_GENOME))
                + currentAlgo!.Stats.Generation.ToString().PadLeft(5, '0')
                + DefaultPaths.GENOME_EXTENSION;
            if (bestGenome.FitnessInfo.PrimaryFitness > previousFitness)
            {
                if (File.Exists(path))
                {
                    File.Copy(path, path + ".backup", true);
                    File.Delete(path);
                }

                NeatGenomeSaver<double>.Save(bestGenome, path);

                previousFitness = bestGenome.FitnessInfo.PrimaryFitness;
            }
        }

        private void Training()
        {
            isTraining = true;

            syncMutex.WaitOne();

            if (!ForceStop)
            {
                OnStatisticsUpdated?.Invoke(GetTrainingStatistics());
                currentAlgo!.Initialise();
                SavePopulation(trainingDirectory + DefaultPaths.CURRENT_POPULATION + DefaultPaths.POPULATION_EXTENSION);
            }

            foreach (var stopCondition in StopConditions)
            {
                stopCondition.Start();
            }

            while (!stopFlag)
            {
                currentAlgo!.PerformOneGeneration();
                TrainingStatistics ts = GetTrainingStatistics();

                if (!ForceStop)
                {
                    OnStatisticsUpdated?.Invoke(ts);

                    SaveBestGenome();
                    SavePopulation(trainingDirectory + DefaultPaths.CURRENT_POPULATION + DefaultPaths.POPULATION_EXTENSION);
                }

                foreach (var stopCondition in StopConditions)
                {
                    if (stopCondition.ShouldUse && stopCondition.CheckShouldStop(ts))
                    {
                        OnStopConditionReached?.Invoke();
                        stopFlag = true;
                        break;
                    }
                }
            }

            syncMutex.ReleaseMutex();
            isTraining = false;
        }

    }
}
