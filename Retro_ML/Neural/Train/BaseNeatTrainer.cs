using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train.StopCondition;
using Retro_ML.Utils;

namespace Retro_ML.Neural.Train;
public abstract class BaseNeatTrainer : INeuralTrainer
{
    public event Action<TrainingStatistics>? OnStatisticsUpdated;
    public event Action? OnStopConditionReached;

    protected readonly Mutex syncMutex;
    protected readonly EmulatorManager emulatorManager;
    protected readonly ApplicationConfig applicationConfig;

    protected string? trainingDirectory;
    protected double previousFitness;

    protected bool stopFlag = false;

    protected bool isTraining = false;

    public bool IsTraining => isTraining;

    protected bool forceStop = false;
    public bool ForceStop
    {
        get => forceStop;
        set => forceStop = value;
    }

    public ICollection<IStopCondition> StopConditions { private get; set; }

    public BaseNeatTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig)
    {
        syncMutex = new Mutex();
        this.emulatorManager = emulatorManager;
        applicationConfig = appConfig;
        StopConditions = appConfig.StopConditions;
    }

    /// <summary>
    /// Starts the training of AIs based on the given config file
    /// </summary>
    /// <param name="configPath">configuration file for the experiment</param>
    /// <exception cref="InvalidOperationException">The experiment may not be started twice</exception>
    public virtual void StartTraining(string configPath)
    {
        if (IsTraining)
        {
            throw new InvalidOperationException("An experiment is already ongoing.");
        }

        trainingDirectory = applicationConfig.GamePluginName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "/";
        _ = Directory.CreateDirectory(trainingDirectory + "/" + DefaultPaths.GENOME_DIR);

        string neuralConfig = applicationConfig.NeuralConfig.Serialize();
        File.WriteAllText(Path.Combine(trainingDirectory, DefaultPaths.NEURAL_CONFIG_NAME), neuralConfig);

        string gamePluginConfig = applicationConfig.GamePluginConfig!.Serialize();
        File.WriteAllText(Path.Combine(trainingDirectory, applicationConfig.GamePluginName + DefaultPaths.GAME_PLUGIN_CONFIG_NAME), gamePluginConfig);

        SetupTraining(configPath);

        new Thread(Training).Start();
    }

    protected abstract void SetupTraining(string configPath);

    public virtual void StopTraining()
    {
        stopFlag = true;
        syncMutex.WaitOne();
        syncMutex.ReleaseMutex();
    }

    public abstract void LoadPopulation(string path);

    public abstract TrainingStatistics GetTrainingStatistics();

    public abstract void SavePopulation(string path);

    public abstract void SaveBestGenome();

    protected void Training()
    {
        isTraining = true;

        syncMutex.WaitOne();

        if (!ForceStop)
        {
            OnStatisticsUpdated?.Invoke(GetTrainingStatistics());
            InitializeNeat();
            SavePopulation(trainingDirectory + DefaultPaths.CURRENT_POPULATION + DefaultPaths.POPULATION_EXTENSION);
        }

        foreach (var stopCondition in StopConditions)
        {
            stopCondition.Start();
        }

        while (!stopFlag)
        {
            RunOneGeneration();
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

    protected abstract void InitializeNeat();
    protected abstract void RunOneGeneration();
}
