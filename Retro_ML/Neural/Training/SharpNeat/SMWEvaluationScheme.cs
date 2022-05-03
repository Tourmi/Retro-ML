using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using System.Collections.Generic;

namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    internal class SMWEvaluationScheme : IBlackBoxEvaluationScheme<double>
    {
        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly INeuralTrainer trainer;

        public int InputCount => appConfig.NeuralConfig.GetInputCount();

        public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

        public bool IsDeterministic => true; // Change if using random levels

        public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

        public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

        //We need an emulator instance for every AI.
        public bool EvaluatorsHaveState => true;

        public SMWEvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, INeuralTrainer trainer)
        {
            this.emulatorManager = emulatorManager;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SMWPhenomeEvaluator(emulatorManager, appConfig, trainer);

        public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
    }
}
