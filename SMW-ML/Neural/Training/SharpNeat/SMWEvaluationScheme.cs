using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SMW_ML.Emulator;
using SMW_ML.Game;
using SMW_ML.Game.SuperMarioWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training.SharpNeat
{
    internal class SMWEvaluationScheme : IBlackBoxEvaluationScheme<double>
    {
        private readonly IEmulatorAdapter emulator;
        private readonly DataGetter dataReader;
        private readonly InputSetter inputSetter;
        private readonly OutputGetter outputGetter;

        public int InputCount => inputSetter.GetInputCount();

        public int OutputCount => outputGetter.GetOutputCount();

        public bool IsDeterministic => true; // Change if using random levels

        public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

        public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

        //We need an emulator instance for every AI.
        public bool EvaluatorsHaveState => true;

        public SMWEvaluationScheme(IEmulatorAdapter emulator, DataGetter dataReader, InputSetter inputSetter, OutputGetter outputGetter)
        {
            this.emulator = emulator;
            this.dataReader = dataReader;
            this.inputSetter = inputSetter;
            this.outputGetter = outputGetter;
        }

        public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SMWPhenomeEvaluator(emulator, dataReader, inputSetter, outputGetter);

        public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
    }
}
