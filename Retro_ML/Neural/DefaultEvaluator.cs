using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;

namespace Retro_ML.Neural;
public class DefaultEvaluator : IEvaluator
{
    protected readonly IEmulatorAdapter emulator;
    protected readonly ApplicationConfig appConfig;
    protected readonly IBlackBox<double> phenome;
    protected readonly IGamePluginConfig gamePluginConfig;
    protected readonly IEnumerable<string> saveStates;
    protected readonly IDataFetcher dataFetcher;
    protected readonly InputSetter inputSetter;
    protected readonly OutputGetter outputGetter;

    public DefaultEvaluator(ApplicationConfig appConfig,
                                  IBlackBox<double> phenome,
                                  IEnumerable<string> saveStates,
                                  IEmulatorAdapter emulator)
    {
        this.appConfig = appConfig;
        gamePluginConfig = appConfig.GamePluginConfig!;
        VerifyScoreFactors(gamePluginConfig.ScoreFactors);

        this.phenome = phenome;
        this.saveStates = saveStates;

        this.emulator = emulator;
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
        DoSaveStateLoop(phenome, score);
        return score.GetFinalScore();
    }

    /// <summary>
    /// Goes through all savestates for the specified training
    /// </summary>
    /// <param name="phenome"></param>
    /// <param name="score"></param>
    protected virtual void DoSaveStateLoop(IBlackBox<double> phenome, Score score)
    {
        foreach (var state in saveStates)
        {
            if (ShouldStop)
            {
                return;
            }

            DoSaveState(phenome, score, state);
        }
    }

    /// <summary>
    /// Executes evaluation for a single given savestate
    /// </summary>
    /// <param name="phenome"></param>
    /// <param name="score"></param>
    /// <param name="state"></param>
    protected virtual void DoSaveState(IBlackBox<double> phenome, Score score, string state)
    {
        emulator.LoadState(Path.GetFullPath(state));
        emulator.NextFrame();
        dataFetcher.NextState();

        DoEvaluationLoop(phenome, score);

        score.LevelDone();
    }

    /// <summary>
    /// Does the evaluation loop for the current savestate
    /// </summary>
    protected virtual void DoEvaluationLoop(IBlackBox<double> phenome, Score score)
    {
        while (!score.ShouldStop)
        {
            if (ShouldStop)
            {
                return;
            }
            DoEvaluationIteration(phenome, score);

            dataFetcher.NextFrame();
            emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(phenome.InputVector), SharpNeatUtils.VectorToArray(phenome.OutputVector));
        }
    }

    /// <summary>
    /// Runs a single score iteration, including the 
    /// </summary>
    protected virtual void DoEvaluationIteration(IBlackBox<double> phenome, Score score)
    {
        DoActivation(phenome);
        DoAIAction(phenome);

        score.Update(dataFetcher!);
    }

    /// <summary>
    /// Runs one iteration of AI evaluation
    /// </summary>
    /// <param name="phenome"></param>
    protected virtual void DoActivation(IBlackBox<double> phenome)
    {
        phenome.ResetState();
        inputSetter.SetInputs(phenome.InputVector);

        phenome.Activate();
    }

    /// <summary>
    /// Does the action(s) the AI chose to do
    /// </summary>
    /// <param name="phenome"></param>
    protected virtual void DoAIAction(IBlackBox<double> phenome)
    {
        emulator.SendInput(outputGetter!.GetControllerInput(phenome.OutputVector));
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
}
