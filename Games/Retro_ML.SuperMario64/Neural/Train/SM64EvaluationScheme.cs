using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.SuperMario64.Neural.Train;

internal class SM64EvaluationScheme : IBlackBoxEvaluationScheme<double>
{
    private readonly EmulatorManager emulatorManager;
    private readonly ApplicationConfig appConfig;
    private readonly SM64Trainer trainer;

    public int InputCount => appConfig.NeuralConfig.GetInputCount();

    public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

    public bool IsDeterministic => true; // Change if using random states

    public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

    public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

    //We need an emulator instance for every AI.
    public bool EvaluatorsHaveState => true;

    public SM64EvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, SM64Trainer trainer)
    {
        this.emulatorManager = emulatorManager;
        this.appConfig = appConfig;
        this.trainer = trainer;
    }

    public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SM64PhenomeEvaluator(emulatorManager, appConfig, trainer);

    public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
}
