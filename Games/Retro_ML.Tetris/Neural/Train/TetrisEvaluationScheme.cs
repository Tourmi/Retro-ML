using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.Tetris.Neural.Train;

internal class TetrisEvaluationScheme : IBlackBoxEvaluationScheme<double>
{
    private readonly EmulatorManager emulatorManager;
    private readonly ApplicationConfig appConfig;
    private readonly TetrisTrainer trainer;

    public int InputCount => appConfig.NeuralConfig.GetInputCount();

    public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

    public bool IsDeterministic => false; // Change if using random levels

    public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

    public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

    //We need an emulator instance for every AI.
    public bool EvaluatorsHaveState => true;

    public TetrisEvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, TetrisTrainer trainer)
    {
        this.emulatorManager = emulatorManager;
        this.appConfig = appConfig;
        this.trainer = trainer;
    }

    public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SharpNeatPhenomeEvaluator(emulatorManager, appConfig, trainer);

    public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
}
