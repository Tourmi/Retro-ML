using SharpNeat.Experiments;
using SharpNeat.NeuralNets;
using Retro_ML.Emulator;
using Retro_ML.Models.Config;
using System.Text.Json;

namespace Retro_ML.Neural.Training.SharpNeatImpl
{
    internal class SMWExperimentFactory
    {
        public static string Id => "smw-experiment-factory";

        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly INeuralTrainer trainer;

        public SMWExperimentFactory(EmulatorManager emulator, ApplicationConfig appConfig, INeuralTrainer trainer)
        {
            this.emulatorManager = emulator;
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
    }
}
