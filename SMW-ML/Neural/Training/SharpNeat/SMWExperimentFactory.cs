using SharpNeat.Experiments;
using SharpNeat.NeuralNets;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using System.Text.Json;

namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    internal class SMWExperimentFactory
    {
        public static string Id => "smw-experiment-factory";

        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;

        public SMWExperimentFactory(EmulatorManager emulator, ApplicationConfig appConfig)
        {
            this.emulatorManager = emulator;
            this.appConfig = appConfig;
        }

        public INeatExperiment<double> CreateExperiment(JsonElement configElem)
        {
            var evalScheme = new SMWEvaluationScheme(emulatorManager, appConfig);

            var experiment = new NeatExperiment<double>(evalScheme, Id)
            {
                IsAcyclic = true,
                ActivationFnName = ActivationFunctionId.LeakyReLU.ToString()
            };

            // Read standard neat experiment json config and use it configure the experiment.
            NeatExperimentJsonReader<double>.Read(experiment, configElem);
            return experiment;
        }
    }
}
