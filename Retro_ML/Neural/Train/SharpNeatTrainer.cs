using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Utils;
using SharpNeat.Experiments;
using SharpNeat.IO;
using SharpNeat.Neat;
using SharpNeat.Neat.EvolutionAlgorithm;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace Retro_ML.Neural.Train;

/// <summary>
/// Neural trainer that uses the SharpNeat library for its training
/// </summary>
public abstract class SharpNeatTrainer : BaseNeatTrainer
{
    protected INeatExperimentFactory? experimentFactory;
    protected INeatExperiment<double>? currentExperiment;
    protected NeatEvolutionAlgorithm<double>? currentAlgo;
    protected MetaNeatGenome<double> metaGenome;
    protected INeatGenomeBuilder<double> genomeBuilder;
    protected List<NeatGenome<double>> genomes;

    /// <summary>
    /// Neural training using the SharpNEAT library
    /// </summary>
    protected SharpNeatTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        metaGenome = new MetaNeatGenome<double>(
               inputNodeCount: appConfig.NeuralConfig.GetInputCount(),
               outputNodeCount: appConfig.NeuralConfig.GetOutputCount(),
               isAcyclic: true,
               activationFn: new LeakyReLU());
        genomeBuilder = NeatGenomeBuilderFactory<double>.Create(metaGenome);
        genomes = new List<NeatGenome<double>>();
    }

    public override void StartTraining(string configPath)
    {
        if (experimentFactory == null)
        {
            throw new InvalidOperationException("No experiment factories were provided");
        }
        base.StartTraining(configPath);
    }

    public override void SavePopulation(string path)
    {
        path = Path.GetFullPath(path);
        FileUtils.BackupThenDeleteFile(path);

        string filename = Path.GetFileName(path);
        genomes = currentAlgo!.Population.GenomeList;

        NeatPopulationSaver<double>.SaveToZipArchive(genomes, path[..path.IndexOf(filename)], filename, System.IO.Compression.CompressionLevel.Fastest);
    }

    public override void LoadPopulation(string path)
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

    public override void SaveBestGenome()
    {
        var bestGenome = currentAlgo!.Population.BestGenome;
        var path = Path.GetFullPath(Path.Combine(trainingDirectory!, DefaultPaths.GENOME_DIR, DefaultPaths.CURRENT_GENOME))
            + currentAlgo!.Stats.Generation.ToString().PadLeft(5, '0')
            + DefaultPaths.GENOME_EXTENSION;
        if (bestGenome.FitnessInfo.PrimaryFitness > previousFitness)
        {
            FileUtils.BackupThenDeleteFile(path);
            NeatGenomeSaver<double>.Save(bestGenome, path);

            previousFitness = bestGenome.FitnessInfo.PrimaryFitness;
        }
    }

    public override TrainingStatistics GetTrainingStatistics()
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

    protected override void SetupTraining(string configPath)
    {
        currentExperiment = experimentFactory!.CreateExperiment(JsonUtils.LoadUtf8(configPath).RootElement);
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
    }

    protected override void InitializeNeat() => currentAlgo!.Initialise();
    protected override void RunOneGeneration() => currentAlgo!.PerformOneGeneration();
}
