using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.Experiments;
using SharpNeat.NeuralNets;
using System.Text.Json;

namespace Retro_ML.StreetFighter2Turbo.Neural.Train
{
    internal class SF2TExperimentFactory : INeatExperimentFactory
    {
        public string Id => "smb-experiment-factory";

        private readonly EmulatorManager emulatorManager;
        private readonly ApplicationConfig appConfig;
        private readonly SF2TTrainer trainer;

        public SF2TExperimentFactory(EmulatorManager emulator, ApplicationConfig appConfig, SF2TTrainer trainer)
        {
            emulatorManager = emulator;
            this.appConfig = appConfig;
            this.trainer = trainer;
        }

        public INeatExperiment<double> CreateExperiment(JsonElement configElem)
        {
            var evalScheme = new SF2TEvaluationScheme(emulatorManager, appConfig, trainer);

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
