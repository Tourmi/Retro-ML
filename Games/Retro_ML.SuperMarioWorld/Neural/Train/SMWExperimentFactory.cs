using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.Experiments;
using SharpNeat.NeuralNets;
using System.Text.Json;

namespace Retro_ML.SuperMarioWorld.Neural.Train
{
    internal class SMWExperimentFactory : INeatExperimentFactory
    {
        public string Id => "smw-experiment-factory";

        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly SMWTrainer trainer;

        public SMWExperimentFactory(EmulatorManager emulator, ApplicationConfig appConfig, SMWTrainer trainer)
        {
            emulatorManager = emulator;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public INeatExperiment<double> CreateExperiment(JsonElement configElem)
        {
            var evalScheme = new SMWEvaluationScheme(emulatorManager, appConfig, trainer);

            var experiment = new NeatExperiment<double>(evalScheme, Id)
            {
                IsAcyclic = true,
                ActivationFnName = ActivationFunctionId.LeakyReLU.ToString()
            };

            // Read standard neat experiment json config and use it configure the experiment.
            NeatExperimentJsonReader<double>.Read(experiment, configElem);
            return experiment;
        }

        public INeatExperiment<float> CreateExperimentSinglePrecision(JsonElement configElem)
        {
            throw new NotImplementedException();
        }
    }
}
