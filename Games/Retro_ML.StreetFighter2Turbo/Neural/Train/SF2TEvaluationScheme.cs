using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.StreetFighter2Turbo.Neural.Train
{
    internal class SF2TEvaluationScheme : IBlackBoxEvaluationScheme<double>
    {
        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly SF2TTrainer trainer;

        public int InputCount => appConfig.NeuralConfig.GetInputCount();

        public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

        public bool IsDeterministic => false; // Change if using random levels

        public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

        public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

        //We need an emulator instance for every AI.
        public bool EvaluatorsHaveState => true;

        public SF2TEvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, SF2TTrainer trainer)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SharpNeatPhenomeEvaluator(emulatorManager, appConfig, trainer);

        public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
    }
}
