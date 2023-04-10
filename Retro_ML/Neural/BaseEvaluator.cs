using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.Neural;
public class BaseEvaluator : IEvaluator
{
    protected IPhenomeWrapper? phenome;
    protected readonly EmulatorManager emulatorManager;
    protected readonly IEmulatorAdapter emulator;
    protected readonly ApplicationConfig appConfig;
    protected readonly IGamePluginConfig gamePluginConfig;
    protected readonly IEnumerable<string> saveStates;
    protected readonly IDataFetcher dataFetcher;
    protected readonly InputSetter inputSetter;
    protected readonly OutputGetter outputGetter;

    public BaseEvaluator(ApplicationConfig appConfig,
                                  IPhenomeWrapper? phenome,
                                  IEnumerable<string> saveStates,
                                  EmulatorManager emulatorManager)
    {
        this.appConfig = appConfig;
        this.phenome = phenome;
        gamePluginConfig = appConfig.GamePluginConfig!;
        VerifyScoreFactors(gamePluginConfig.ScoreFactors);

        this.saveStates = saveStates;
        this.emulatorManager = emulatorManager;
        this.emulator = emulatorManager.WaitOne();
        dataFetcher = emulator.GetDataFetcher();
        inputSetter = emulator.GetInputSetter();
        outputGetter = emulator.GetOutputGetter();
    }

    /// <summary>
    /// When set to true, quickly stops the evaluation of the current genome
    /// </summary>
    public virtual bool ShouldStop { get; set; }

    /// <summary>
    /// How many frames to skip per evaluation. Used in real-time games.
    /// </summary>
    protected virtual int FrameSkip => 0;
    /// <summary>
    /// Whether or not the controller inputs should be held down while skipping frames
    /// </summary>
    protected virtual bool FrameSkipShouldKeepControllerInputs => true;

    public virtual double Evaluate()
    {
        Score score = new(appConfig.GetScoreFactorClones());
        DoSaveStateLoop(score);
        return score.GetFinalScore();
    }

    /// <summary>
    /// Goes through all savestates for the specified training
    /// </summary>
    /// <param name="phenome"></param>
    /// <param name="score"></param>
    protected virtual void DoSaveStateLoop(Score score)
    {
        foreach (var state in saveStates)
        {
            if (ShouldStop)
            {
                return;
            }

            DoSaveState(score, state);
        }
    }

    /// <summary>
    /// Executes evaluation for a single given savestate
    /// </summary>
    /// <param name="phenome"></param>
    /// <param name="score"></param>
    /// <param name="state"></param>
    protected virtual void DoSaveState(Score score, string state)
    {
        emulator.LoadState(Path.GetFullPath(state));
        emulator.NextFrame();
        dataFetcher.NextState();

        DoEvaluationLoop(score);

        score.LevelDone();
    }

    /// <summary>
    /// Does the evaluation loop for the current savestate
    /// </summary>
    protected virtual void DoEvaluationLoop(Score score)
    {
        while (!score.ShouldStop)
        {
            if (ShouldStop)
            {
                return;
            }
            DoEvaluationIteration(score);

            dataFetcher.NextFrame();
            emulator.NetworkUpdated(phenome!.InputNodes.ToArray(), phenome!.OutputNodes.ToArray());
        }
    }

    /// <summary>
    /// Runs a single score iteration, including the 
    /// </summary>
    protected virtual void DoEvaluationIteration(Score score)
    {
        DoActivation();
        DoAIAction();

        score.Update(dataFetcher!);
    }

    /// <summary>
    /// Runs one iteration of AI evaluation
    /// </summary>
    /// <param name="phenome"></param>
    protected virtual void DoActivation()
    {
        phenome?.ResetState();
        inputSetter.SetInputs(phenome!.InputNodes);

        phenome?.Activate();
    }

    /// <summary>
    /// Does the action(s) the AI chose to do
    /// </summary>
    /// <param name="phenome"></param>
    protected virtual void DoAIAction()
    {
        emulator.SendInput(outputGetter!.GetControllerInput(phenome!.OutputNodes));
        emulator.NextFrames(FrameSkip + 1, FrameSkipShouldKeepControllerInputs);
    }

    protected virtual void VerifyScoreFactors(List<IScoreFactor> scoreFactors)
    {
        foreach (var sf in scoreFactors)
        {
            var clone = sf.Clone();

            if (sf.GetType() != clone.GetType())
            {
                Exceptions.QueueException(new Exception($"Type: {sf.GetType()} did not match type {clone.GetType()} after cloning. The Objective will not work properly"));
            }
        }
    }

    public void Dispose()
    {
        emulatorManager.FreeOne(emulator);
    }
}
