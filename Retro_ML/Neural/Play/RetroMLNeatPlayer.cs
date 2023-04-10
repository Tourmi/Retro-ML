using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.NEAT;
using Retro_ML.NEAT.Configuration;
using Retro_ML.NEAT.Creatures.Phenotype;
using Retro_ML.Utils.RetroMLNeat;

namespace Retro_ML.Neural.Play;
public sealed class RetroMLNeatPlayer : BaseNeatPlayer
{
    private readonly ExperimentSettings experimentSettings;
    private readonly List<IPhenome> phenomes;

    public RetroMLNeatPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        phenomes = new();
        experimentSettings = new ExperimentSettings() { NeuralInputCount = appConfig.NeuralConfig.GetInputCount(), NeuralOutputCount = appConfig.NeuralConfig.GetOutputCount() };
    }

    protected override bool AreAnyGenomesLoaded() => phenomes.Any();
    protected override void ClearGenomes() => phenomes.Clear();
    protected override void DoPlayLoop()
    {
        var phenomesEnum = phenomes.GetEnumerator();

        while (!shouldStop && phenomesEnum.MoveNext())
        {
            genomeEvaluator = gamePlugin.GetEvaluator(appConfig, new PhenomeWrapper(phenomesEnum.Current), states, emulatorManager);
            _ = genomeEvaluator.Evaluate();
        }
    }
    protected override bool LoadGenome(string path)
    {
        phenomes.Add(ExperimentBuilder.GetPhenome(path));

        return true;
    }
    protected override bool VerifyGenome(string path)
    {
        var phenome = ExperimentBuilder.GetPhenome(path);
        return phenome.InputCount == experimentSettings.NeuralInputCount && phenome.OutputCount == experimentSettings.NeuralOutputCount;
    }
}
