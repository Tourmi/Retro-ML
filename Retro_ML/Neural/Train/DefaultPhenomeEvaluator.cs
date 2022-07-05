using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.Neural.Train;
public class DefaultPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
{
    protected readonly EmulatorManager emulatorManager;
    protected readonly INeuralTrainer trainer;
    protected readonly ApplicationConfig appConfig;
    protected readonly IGamePluginConfig gamePluginConfig;

    protected IEmulatorAdapter? emulator;
    protected IDataFetcher? dataFetcher;
    protected InputSetter? inputSetter;
    protected OutputGetter? outputGetter;

    public DefaultPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, INeuralTrainer trainer)
    {
        this.emulatorManager = emulatorManager;
        this.appConfig = appConfig;
        this.trainer = trainer;
        gamePluginConfig = appConfig.GamePluginConfig!;
    }

    /// <summary>
    /// How many frames to skip per evaluation. Used in real-time games.
    /// </summary>
    protected virtual int FrameSkip => 0;
    /// <summary>
    /// Whether or not the controller inputs should be held down while skipping frames
    /// </summary>
    protected virtual bool FrameSkipShouldKeepControllerInputs => true;

    public virtual FitnessInfo Evaluate(IBlackBox<double> phenome)
    {
        try
        {
            Score score = new(appConfig.GetScoreFactorClones());

            emulator = emulatorManager.WaitOne();
            int[] outputMap = new int[phenome.OutputCount];
            Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);
            emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(phenome), outputMap);
            dataFetcher = emulator.GetDataFetcher();
            inputSetter = emulator.GetInputSetter();
            outputGetter = emulator.GetOutputGetter();

            DoSaveStateLoop(phenome, score);

            dataFetcher = null;
            emulatorManager.FreeOne(emulator);
            emulator = null;

            return new FitnessInfo(score.GetFinalScore());
        }
        catch (Exception ex)
        {
            Exceptions.QueueException(new Exception($"Error occured during training. Was an emulator closed?\n{ex.Message}\n{ex.StackTrace}"));

            return FitnessInfo.DefaultFitnessInfo;
        }
    }

    /// <summary>
    /// Goes through all savestates for the specified training
    /// </summary>
    /// <param name="phenome"></param>
    /// <param name="score"></param>
    protected virtual void DoSaveStateLoop(IBlackBox<double> phenome, Score score)
    {
        var saveStates = appConfig.SaveStates;
        foreach (var state in saveStates)
        {
            if (trainer.ForceStop)
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
        emulator!.LoadState(Path.GetFullPath(state));
        emulator.NextFrame();
        dataFetcher!.NextState();

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
            if (trainer.ForceStop)
            {
                return;
            }
            DoEvaluationIteration(phenome, score);

            dataFetcher!.NextFrame();
            emulator!.NetworkUpdated(SharpNeatUtils.VectorToArray(phenome.InputVector), SharpNeatUtils.VectorToArray(phenome.OutputVector));
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
        inputSetter!.SetInputs(phenome.InputVector);

        phenome.Activate();
    }

    /// <summary>
    /// Does the action(s) the AI chose to do
    /// </summary>
    /// <param name="phenome"></param>
    protected virtual void DoAIAction(IBlackBox<double> phenome)
    {
        emulator!.SendInput(outputGetter!.GetControllerInput(phenome.OutputVector));
        emulator!.NextFrames(FrameSkip + 1, FrameSkipShouldKeepControllerInputs);
    }
}
