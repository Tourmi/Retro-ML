using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.NEAT;
using Retro_ML.NEAT.Configuration;
using Retro_ML.NEAT.Training;
using Retro_ML.Utils;

namespace Retro_ML.Neural.Train;
public sealed class RetroMLNeatTrainer : BaseNeatTrainer
{
    private TrainingStatistics? stats;
    private IPopulationTrainer? trainer;
    public RetroMLNeatTrainer(EmulatorManager emulatorManager, ApplicationConfig applicationConfig) : base(emulatorManager, applicationConfig)
    {
    }

    public override TrainingStatistics GetTrainingStatistics()
    {
        var trainerStats = trainer!.GetStatistics();
        stats = new TrainingStatistics();

        stats.AddStat(TrainingStatistics.CURRENT_GEN, trainerStats.GenerationCount);
        stats.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, trainerStats.BestGenomeFitness);
        stats.AddStat(TrainingStatistics.MEAN_FITNESS, trainerStats.AverageFitness);

        stats.AddStat(TrainingStatistics.BEST_GENOME_COMPLEXITY, trainerStats.BestGenomeComplexity);
        stats.AddStat(TrainingStatistics.MEAN_COMPLEXITY, trainerStats.AverageGenomeComplexity);
        stats.AddStat(TrainingStatistics.MAX_COMPLEXITY, trainerStats.MaximumGenomeComplexity);

        stats.AddStat(TrainingStatistics.SPECIES_COUNT, trainerStats.TotalSpecies);
        stats.AddStat(TrainingStatistics.BEST_SPECIES_POPULATION, trainerStats.BestSpeciesPopulation);
        stats.AddStat(TrainingStatistics.AVERAGE_SPECIES_POPULATION, trainerStats.AverageSpeciesPopulation);
        return stats;
    }

    public override void LoadPopulation(string path) => trainer!.LoadPopulation(path);
    public override void SaveBestGenome()
    {
        var path = Path.GetFullPath(Path.Combine(trainingDirectory!, DefaultPaths.GENOME_DIR, DefaultPaths.CURRENT_GENOME))
            + ((int?)stats?.GetStat(TrainingStatistics.CURRENT_GEN) ?? 0).ToString().PadLeft(5, '0')
        + DefaultPaths.GENOME_EXTENSION;

        double bestScore = stats?.GetStat(TrainingStatistics.BEST_GENOME_FITNESS) ?? 0;

        if (bestScore > previousFitness)
        {
            FileUtils.BackupThenDeleteFile(path);
            trainer!.SaveBestGenome(path);

            previousFitness = bestScore;
        }
    }
    protected override void InitializeNeat() => trainer!.Initialize(GetPhenomeEvaluator);
    protected override void RunOneGeneration() => trainer!.RunOneGeneration();
    public override void SavePopulation(string path)
    {
        path = Path.GetFullPath(path);
        FileUtils.BackupThenDeleteFile(path);

        trainer!.SavePopulation(path);
    }
    protected override void SetupTraining(string configPath)
    {
        var experimentSettings = new ExperimentSettings()
        {
            NeuralInputCount = applicationConfig.NeuralConfig.GetInputCount(),
            NeuralOutputCount = applicationConfig.NeuralConfig.GetOutputCount()
        };

        trainer = ExperimentBuilder.GetTrainer(configPath, experimentSettings, new Random());
    }

    private NEAT.Training.IEvaluator GetPhenomeEvaluator()
    {
        return new RetroMLNeatEvaluator(applicationConfig, null, applicationConfig.SaveStates, emulatorManager);
    }
}
