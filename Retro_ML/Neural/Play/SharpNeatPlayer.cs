using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace Retro_ML.Neural.Play;

public sealed class SharpNeatPlayer : BaseNeatPlayer
{
    private MetaNeatGenome<double> metaGenome;

    public SharpNeatPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) : base(emulatorManager, appConfig)
    {
        metaGenome = new MetaNeatGenome<double>(
                inputNodeCount: appConfig.NeuralConfig.GetInputCount(),
                outputNodeCount: appConfig.NeuralConfig.GetOutputCount(),
                isAcyclic: true,
                activationFn: new LeakyReLU());
    }

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
        phenomes.Add(new PhenomeWrapper(decoder.Decode(loader.Load(path))));
        return true;
    }
}
