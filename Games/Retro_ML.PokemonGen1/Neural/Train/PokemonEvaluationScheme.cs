using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.PokemonGen1.Neural.Train;

internal class PokemonEvaluationScheme : IBlackBoxEvaluationScheme<double>
{
    private readonly EmulatorManager emulatorManager;
    private readonly ApplicationConfig appConfig;
    private readonly PokemonTrainer trainer;

    public int InputCount => appConfig.NeuralConfig.GetInputCount();

    public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

    public bool IsDeterministic => false; // Change if using random states

    public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

    public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

    //We need an emulator instance for every AI.
    public bool EvaluatorsHaveState => true;

    public PokemonEvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, PokemonTrainer trainer)
    {
        this.emulatorManager = emulatorManager;
        this.appConfig = appConfig;
        this.trainer = trainer;
    }

    public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new PokemonPhenomeEvaluator(emulatorManager, appConfig, trainer);

    public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
}
