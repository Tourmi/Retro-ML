using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Plugin;
using Retro_ML.Utils;

namespace Retro_ML.Neural.Play;
public abstract class BaseNeatPlayer : INeuralPlayer
{
    public event Action? FinishedPlaying;

    protected readonly Semaphore syncSemaphore;
    protected readonly EmulatorManager emulatorManager;
    protected readonly IEmulatorAdapter emulator;
    protected readonly ApplicationConfig appConfig;
    protected readonly IGamePlugin gamePlugin;
    protected readonly List<IPhenomeWrapper> phenomes;

    protected readonly List<string> states;

    protected IEvaluator? genomeEvaluator;

    protected bool shouldStop;

    public bool IsPlaying { get; protected set; }

    public BaseNeatPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig)
    {
        this.emulatorManager = emulatorManager;
        emulatorManager.Init(false);
        syncSemaphore = new Semaphore(1, 1);

        states = new();
        this.appConfig = appConfig;
        gamePlugin = appConfig.GetGamePlugin();

        emulator = emulatorManager.GetFirstEmulator();

        phenomes = new();
    }

    public bool LoadGenomes(string[] paths)
    {
        bool shouldRestart = IsPlaying;
        StopPlaying();

        phenomes.Clear();
        foreach (var path in paths.OrderBy(p => p))
        {
            if (!CheckAndLoadGenome(path))
            {
                return false;
            }
        }

        if (shouldRestart) StartPlaying();
        return true;
    }

    public void LoadStates(string[] paths)
    {
        bool shouldRestart = IsPlaying;
        StopPlaying();

        states.Clear();

        foreach (var path in paths)
        {
            LoadState(path);
        }

        if (shouldRestart) StartPlaying();
    }

    public void StartPlaying()
    {
        if (IsPlaying)
        {
            throw new InvalidOperationException("Already playing");
        }
        if (!phenomes.Any())
        {
            throw new InvalidOperationException("A genome wasn't loaded before playing");
        }
        if (states.Count == 0)
        {
            throw new InvalidOperationException("A save state wasn't loaded before playing");
        }
        IsPlaying = true;
        shouldStop = false;

        new Thread(Play).Start();
    }

    public void StopPlaying()
    {
        shouldStop = true;
        if (genomeEvaluator != null)
        {
            genomeEvaluator.ShouldStop = true;
        }
        syncSemaphore.WaitOne();
        syncSemaphore.Release();
    }

    protected bool CheckAndLoadGenome(string path)
    {
        if (!VerifyGenome(path))
        {
            Exceptions.QueueException(new Exception($"Could not load genome {path}. It uses a different Neural Configuration."));
            return false;
        }

        return LoadGenome(path);
    }

    protected abstract bool LoadGenome(string path);
    protected abstract bool VerifyGenome(string path);

    protected void LoadState(string path)
    {
        states.Add(Path.GetFullPath(path));
    }

    /// <summary>
    /// Play loop
    /// </summary>
    protected virtual void Play()
    {
        try
        {
            IsPlaying = true;
            syncSemaphore.WaitOne();

            DoPlayLoop();

            syncSemaphore.Release();
            IsPlaying = false;
            if (!shouldStop) FinishedPlaying?.Invoke();
        }
        catch (Exception ex)
        {
            Exceptions.QueueException(new Exception($"An exception occured during play. Was the emulator window closed?\n{ex.Message}\n{ex.StackTrace}"));
        }
    }

    /// <summary>
    /// Goes through every savestates for every loaded genomes
    /// </summary>

    protected virtual void DoPlayLoop()
    {
        var phenomesEnum = phenomes.GetEnumerator();

        while (!shouldStop && phenomesEnum.MoveNext())
        {
            genomeEvaluator = gamePlugin.GetEvaluator(appConfig, phenomesEnum.Current, states, emulatorManager);
            _ = genomeEvaluator.Evaluate();
            genomeEvaluator.Dispose();
        }
    }

    public virtual void Dispose()
    {
        emulatorManager.Clean();
        GC.SuppressFinalize(this);
    }
}
