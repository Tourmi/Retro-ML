using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace Retro_ML.Neural.Play;

public sealed class SharpNeatPlayer : BaseNeatPlayer
{
    private MetaNeatGenome<double> metaGenome;
    private readonly List<IBlackBox<double>> blackBoxes;

    public SharpNeatPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        metaGenome = new MetaNeatGenome<double>(
                inputNodeCount: appConfig.NeuralConfig.GetInputCount(),
                outputNodeCount: appConfig.NeuralConfig.GetOutputCount(),
                isAcyclic: true,
                activationFn: new LeakyReLU());
        blackBoxes = new();
    }

    protected override bool AreAnyGenomesLoaded() => blackBoxes.Any();

    protected override bool VerifyGenome(string path)
    {
        string[] inputOutput = File.ReadLines(path).Where(l => !l.Trim().StartsWith("#") && !string.IsNullOrEmpty(l.Trim())).First().Trim().Split(null);
        int input = int.Parse(inputOutput[0]);
        int output = int.Parse(inputOutput[1]);

        return input == metaGenome.InputNodeCount && output == metaGenome.OutputNodeCount;
    }

    protected override bool LoadGenome(string path)
    {
        var loader = NeatGenomeLoaderFactory.CreateLoaderDouble(metaGenome);

        NeatGenomeDecoderAcyclic decoder = new();
        blackBoxes.Add(decoder.Decode(loader.Load(path)));
        return true;
    }

    protected override void DoPlayLoop()
    {
        var blackBoxesEnum = blackBoxes.GetEnumerator();

        while (!shouldStop && blackBoxesEnum.MoveNext())
        {
            UpdateNetwork(blackBoxesEnum.Current);
            genomeEvaluator = gamePlugin.GetEvaluator(appConfig, new PhenomeWrapper(blackBoxesEnum.Current), states, emulatorManager);
            _ = genomeEvaluator.Evaluate();
        }
    }

    /// <summary>
    /// Sends the network changed event.
    /// </summary>
    private void UpdateNetwork(IBlackBox<double> blackBox)
    {
        int[] outputMap = new int[blackBox!.OutputCount];
        Array.Copy(blackBox.OutputVector.GetField<int[]>("_map"), outputMap, blackBox.OutputCount);
        emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(blackBox), outputMap);
    }

    protected override void ClearGenomes() => blackBoxes.Clear();
}
