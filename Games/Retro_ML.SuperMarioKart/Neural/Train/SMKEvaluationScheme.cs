using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.SuperMarioKart.Neural.Train
{
    internal class SMKEvaluationScheme : IBlackBoxEvaluationScheme<double>
    {
        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly SMKTrainer trainer;

        public int InputCount => appConfig.NeuralConfig.GetInputCount();

        public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

        public bool IsDeterministic => true; // Change if using random states

        public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

        public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

        //We need an emulator instance for every AI.
        public bool EvaluatorsHaveState => true;

        public SMKEvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, SMKTrainer trainer)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SMKPhenomeEvaluator(emulatorManager, appConfig, trainer);

        public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
    }
}
