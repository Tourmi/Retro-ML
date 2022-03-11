namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    /// <summary>
    /// Neural trainer that uses the SharpNeat library for its training
    /// </summary>
    internal class SharpNeatTrainer : INeuralTrainer
    {
        public event Action<TrainingStatistics>? OnStatisticsUpdated;

        private readonly SMWExperimentFactory experimentFactory;
        private readonly Semaphore syncSemaphore;
        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig applicationConfig;

        private string? trainingDirectory;
        private double previousFitness;

        private INeatExperiment<double>? currentExperiment;
        private NeatEvolutionAlgorithm<double>? currentAlgo;
        private bool stopFlag = false;

        private MetaNeatGenome<double> metaGenome;
        private INeatGenomeBuilder<double> genomeBuilder;
        private List<NeatGenome<double>> genomes;

        private bool isTraining = false;

        public bool IsTraining => isTraining;

        private bool forceStop = false;
        public bool ForceStop
        {
            get => forceStop;
            set => forceStop = value;
        }

        /// <summary>
        /// Neural training using the SharpNEAT library
        /// </summary>
        public SharpNeatTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig)
        {
            syncSemaphore = new Semaphore(1, 1);
            this.emulatorManager = emulatorManager;
            experimentFactory = new SMWExperimentFactory(emulatorManager, appConfig, this);
            applicationConfig = appConfig;
            metaGenome = new MetaNeatGenome<double>(
                   inputNodeCount: appConfig.NeuralConfig.GetInputCount(),
                   outputNodeCount: appConfig.NeuralConfig.GetOutputCount(),
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

            if (!ForceStop)
            {
                OnStatisticsUpdated?.Invoke(GetTrainingStatistics());
                currentAlgo!.Initialise();
                SavePopulation(trainingDirectory + DefaultPaths.CURRENT_POPULATION + DefaultPaths.POPULATION_EXTENSION);
            }

            while (!stopFlag)
            {
                currentAlgo!.PerformOneGeneration();

                if (!ForceStop)
                {
                    OnStatisticsUpdated?.Invoke(GetTrainingStatistics());

                    SaveBestGenome();
                    SavePopulation(trainingDirectory + DefaultPaths.CURRENT_POPULATION + DefaultPaths.POPULATION_EXTENSION);
                }
            }

            syncSemaphore.Release();
            isTraining = false;

        }

        public void LoadPopulation(string path)
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

        public void SaveBestGenome()
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
    }
}
