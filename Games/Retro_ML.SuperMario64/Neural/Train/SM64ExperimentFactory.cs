using Retro_ML.Configuration;
using Retro_ML.Emulator;
using SharpNeat.Experiments;
using SharpNeat.NeuralNets;
using System.Text.Json;

namespace Retro_ML.SuperMario64.Neural.Train;

internal class SM64ExperimentFactory : INeatExperimentFactory
{
    public string Id => "sm64-experiment-factory";

    private readonly EmulatorManager emulatorManager;
    private readonly ApplicationConfig appConfig;
    private readonly SM64Trainer trainer;

    public SM64ExperimentFactory(EmulatorManager emulator, ApplicationConfig appConfig, SM64Trainer trainer)
    {
        emulatorManager = emulator;
        this.appConfig = appConfig;
        this.trainer = trainer;
    }

    public INeatExperiment<double> CreateExperiment(JsonElement configElem)
    {
        var evalScheme = new SM64EvaluationScheme(emulatorManager, appConfig, trainer);

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
