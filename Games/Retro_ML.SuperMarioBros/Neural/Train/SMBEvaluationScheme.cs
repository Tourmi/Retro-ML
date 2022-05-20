using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.SuperMarioBros.Neural.Train
{
    internal class SMBEvaluationScheme : IBlackBoxEvaluationScheme<double>
    {
        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly SMBTrainer trainer;

        public int InputCount => appConfig.NeuralConfig.GetInputCount();

        public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

        public bool IsDeterministic => true; // Change if using random levels

        public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

        public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

        //We need an emulator instance for every AI.
        public bool EvaluatorsHaveState => true;

        public SMBEvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, SMBTrainer trainer)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SMBPhenomeEvaluator(emulatorManager, appConfig, trainer);

        public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
    }
}
