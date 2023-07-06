using NUnit.Framework;
using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Creatures.Phenotype;

namespace Retro_ML_TEST.NEAT.Creatures.Phenotype;
[TestFixture]
internal class PhenomeTest
{
    private static Genome DefaultGenome => new()
    {
        ConnectionGenes = new ConnectionGene[] {
            new ConnectionGene() { InputNode=0, OutputNode=3, Enabled=true, InnovationNumber= 0, Weight=2 },
            new ConnectionGene() { InputNode=1, OutputNode=4, Enabled=false, InnovationNumber= 1, Weight=-2 },
            new ConnectionGene() { InputNode=1, OutputNode=5, Enabled=true, InnovationNumber= 2, Weight=-2 },
            new ConnectionGene() { InputNode=5, OutputNode=4, Enabled=true, InnovationNumber= 3, Weight=1.1 },
        },
        InputNodeCount = 3,
        OutputNodeCount = 2,
        TotalNodeCount = 6,
        HiddenActivationFunction = "Linear",
        InputActivationFunction = "Linear",
        OutputActivationFunction = "Linear",
        AdjustedFitness = new(5),
        Fitness = new(50)
    };

    [Test]
    public void ConstructorTest()
    {
        var phenome = new Phenome(DefaultGenome);

        Assert.AreEqual(3, phenome.InputCount);
        Assert.AreEqual(2, phenome.OutputCount);
        Assert.AreEqual(3, phenome.Inputs.Count);
        Assert.AreEqual(2, phenome.Outputs.Count);
    }

    [Test]
    public void GetConnectionLayersTest()
    {
        var phenome = new Phenome(DefaultGenome);
        var expectedLayers = new (int, int, double)[2][];
        expectedLayers[0] = new (int, int, double)[] { (0, 3, 2), (1, 5, -2) };
        expectedLayers[1] = new (int, int, double)[] { (5, 4, 1.1) };

        var expectedOutputs = new int[] { 3, 4 };

        Assert.AreEqual((expectedLayers, expectedOutputs), phenome.GetConnectionLayers());
    }

    [Test]
    public void ActivateTest()
    {
        var phenome = new Phenome(DefaultGenome);

        var inputs = phenome.Inputs;
        inputs[0] = 3.5;
        inputs[1] = -2.1;
        inputs[2] = 1000000;

        phenome.Activate();
        var outputs = phenome.Outputs;
        Assert.AreEqual(new double[] { 3.5 * 2, -2.1 * -2 * 1.1 }, outputs);
    }
}
