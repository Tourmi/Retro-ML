using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.Neural.Train;
public sealed class SharpNeatPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
{
    private readonly EmulatorManager emulatorManager;
    private readonly INeuralTrainer trainer;
    private readonly ApplicationConfig appConfig;

    private bool shouldStop;
    private IEvaluator? evaluator;

    public SharpNeatPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, INeuralTrainer trainer)
    {
        this.emulatorManager = emulatorManager;
        this.appConfig = appConfig;
        this.trainer = trainer;
    }

    public FitnessInfo Evaluate(IBlackBox<double> phenome)
    {
        try
        {
            shouldStop = false;
            int[] outputMap = new int[phenome.OutputCount];
            Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);

            new Thread(CheckShouldStopThread).Start();
            evaluator = appConfig.GetGamePlugin().GetEvaluator(appConfig, new PhenomeWrapper(phenome), appConfig.SaveStates, emulatorManager);
            double score = evaluator.Evaluate();
            evaluator.Dispose();

            shouldStop = true;

            return new FitnessInfo(score);
        }
        catch (Exception ex)
        {
            Exceptions.QueueException(new Exception($"Error occured during training. Was an emulator closed?\n{ex.Message}\n{ex.StackTrace}"));

            return FitnessInfo.DefaultFitnessInfo;
        }
    }

    private void CheckShouldStopThread()
    {
        while (!trainer.ForceStop && !shouldStop)
        {
            Thread.Sleep(250);
        }

        evaluator!.ShouldStop = true;
    }
}
