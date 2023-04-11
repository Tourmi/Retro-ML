using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.NEAT;
using Retro_ML.NEAT.Configuration;
using Retro_ML.Utils.RetroMLNeat;

namespace Retro_ML.Neural.Play;
public sealed class RetroMLNeatPlayer : BaseNeatPlayer
{
    private readonly ExperimentSettings experimentSettings;

    public RetroMLNeatPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        experimentSettings = new ExperimentSettings() { NeuralInputCount = appConfig.NeuralConfig.GetInputCount(), NeuralOutputCount = appConfig.NeuralConfig.GetOutputCount() };
    }

    protected override bool LoadGenome(string path)
    {
        phenomes.Add(new PhenomeWrapper(ExperimentBuilder.GetPhenome(path)));

        return true;
    }

    protected override bool VerifyGenome(string path)
    {
        var phenome = ExperimentBuilder.GetPhenome(path);
        return phenome.InputCount == experimentSettings.NeuralInputCount && phenome.OutputCount == experimentSettings.NeuralOutputCount;
    }
}
