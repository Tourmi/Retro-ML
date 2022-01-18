using SharpNeat.Evaluation;
using SharpNeat.Experiments;
using SharpNeat.Neat.ComplexityRegulation;
using SharpNeat.Neat.EvolutionAlgorithm;
using SharpNeat.Neat.Reproduction.Asexual;
using SharpNeat.Neat.Reproduction.Sexual;
using SharpNeat.NeuralNets;
using SMW_ML.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training.SharpNeat
{
    internal class SMWExperimentFactory : INeatExperimentFactory
    {
        public string Id => "smw-experiment-factory";

        private IEmulatorAdapter emulator;

        public SMWExperimentFactory(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
        }

        public INeatExperiment<double> CreateExperiment(JsonElement configElem)
        {
            var evalScheme = new SMWEvaluationScheme(emulator);

            var experiment = new NeatExperiment<double>(evalScheme, this.Id)
            {
                IsAcyclic = true,
                ActivationFnName = ActivationFunctionId.LeakyReLU.ToString()
            };

            // Read standard neat experiment json config and use it configure the experiment.
            NeatExperimentJsonReader<double>.Read(experiment, configElem);
            return experiment;
        }

        /// <exception cref="NotImplementedException">Not implemented</exception>
        public INeatExperiment<float> CreateExperimentSinglePrecision(JsonElement configElem)
        {
            throw new NotImplementedException();
        }
    }
}
