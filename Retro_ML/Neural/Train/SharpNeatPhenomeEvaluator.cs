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
    private IEmulatorAdapter? emulator;
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
            emulator = emulatorManager.WaitOne();
            int[] outputMap = new int[phenome.OutputCount];
            Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);
            emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(phenome), outputMap);

            new Thread(CheckShouldStopThread).Start();
            evaluator = appConfig.GetGamePlugin().GetEvaluator(appConfig, phenome, appConfig.SaveStates, emulator);
            double score = evaluator.Evaluate();

            shouldStop = true;
            emulatorManager.FreeOne(emulator);
            emulator = null;

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
