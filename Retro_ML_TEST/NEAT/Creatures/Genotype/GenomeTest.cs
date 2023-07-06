using NUnit.Framework;
using Retro_ML.NEAT.Creatures.Genotype;
using System.Linq;

namespace Retro_ML_TEST.NEAT.Creatures.Genotype;
[TestFixture]
internal class GenomeTest
{
    private static Genome DefaultGenome => new()
    {
        ConnectionGenes = new ConnectionGene[] {
            new ConnectionGene() { InputNode=0, OutputNode=3, Enabled=true, InnovationNumber= 0, Weight=2 },
            new ConnectionGene() { InputNode=1, OutputNode=4, Enabled=false, InnovationNumber= 1, Weight=-2 },
            new ConnectionGene() { InputNode=1, OutputNode=5, Enabled=true, InnovationNumber= 2, Weight=-2 },
            new ConnectionGene() { InputNode=5, OutputNode=4, Enabled=true, InnovationNumber= 3, Weight=1 },
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
    public void CopyTest()
    {
        var genome = DefaultGenome;

        Assert.False(ReferenceEquals(genome, DefaultGenome.Copy()));
        Assert.False(ReferenceEquals(genome.ConnectionGenes, DefaultGenome.Copy().ConnectionGenes));

        Assert.AreEqual(genome.InputNodeCount, DefaultGenome.Copy().InputNodeCount);
        Assert.AreEqual(genome.OutputNodeCount, DefaultGenome.Copy().OutputNodeCount);
        Assert.AreEqual(genome.TotalNodeCount, DefaultGenome.Copy().TotalNodeCount);
        Assert.AreEqual(genome.InputActivationFunction, DefaultGenome.Copy().InputActivationFunction);
        Assert.AreEqual(genome.HiddenActivationFunction, DefaultGenome.Copy().HiddenActivationFunction);
        Assert.AreEqual(genome.OutputActivationFunction, DefaultGenome.Copy().OutputActivationFunction);

        Assert.AreEqual(genome.Fitness, DefaultGenome.Copy().Fitness);
        Assert.AreEqual(genome.AdjustedFitness, DefaultGenome.Copy().AdjustedFitness);

        Assert.AreEqual(genome.ConnectionGenes, DefaultGenome.Copy().ConnectionGenes);
    }

    [Test]
    public void WithGenesTest()
    {
        var genome = DefaultGenome;
        var genes = new ConnectionGene[] {
            new ConnectionGene() { InputNode=0, OutputNode=5, Enabled=true, InnovationNumber= 5, Weight=1 },
            new ConnectionGene() { InputNode=5, OutputNode=3, Enabled=false, InnovationNumber= 6, Weight=-1 },
            new ConnectionGene() { InputNode=2, OutputNode=3, Enabled=true, InnovationNumber= 7, Weight=-4 },
            new ConnectionGene() { InputNode=1, OutputNode=4, Enabled=true, InnovationNumber= 8, Weight=3 },
        };
        genome.ConnectionGenes = genes.ToArray();

        Assert.False(ReferenceEquals(genome, DefaultGenome.WithGenes(genes)));
        Assert.False(ReferenceEquals(genome.ConnectionGenes, DefaultGenome.WithGenes(genes).ConnectionGenes));

        Assert.AreEqual(genome.InputNodeCount, DefaultGenome.WithGenes(genes).InputNodeCount);
        Assert.AreEqual(genome.OutputNodeCount, DefaultGenome.WithGenes(genes).OutputNodeCount);
        Assert.AreEqual(genome.TotalNodeCount, DefaultGenome.WithGenes(genes).TotalNodeCount);
        Assert.AreEqual(genome.InputActivationFunction, DefaultGenome.WithGenes(genes).InputActivationFunction);
        Assert.AreEqual(genome.HiddenActivationFunction, DefaultGenome.WithGenes(genes).HiddenActivationFunction);
        Assert.AreEqual(genome.OutputActivationFunction, DefaultGenome.WithGenes(genes).OutputActivationFunction);

        Assert.AreEqual(genome.Fitness, DefaultGenome.WithGenes(genes).Fitness);
        Assert.AreEqual(genome.AdjustedFitness, DefaultGenome.WithGenes(genes).AdjustedFitness);

        Assert.AreEqual(genome.ConnectionGenes, DefaultGenome.WithGenes(genes).ConnectionGenes);
    }

    [Test]
    public void HasCycleTest()
    {
        var genome = DefaultGenome;

        Assert.False(genome.HasCycle());

        genome.ConnectionGenes = new ConnectionGene[]
        {
            new() {InputNode=0, OutputNode=0}
        };
        Assert.True(genome.HasCycle());

        genome.ConnectionGenes = new ConnectionGene[]
        {
            new() {InputNode = 0, OutputNode = 1},
            new() {InputNode = 1, OutputNode = 0},
        };
        Assert.True(genome.HasCycle());

        genome.ConnectionGenes = new ConnectionGene[]
        {
            new() {InputNode = 0, OutputNode = 1},
            new() {InputNode = 0, OutputNode = 2},
            new() {InputNode = 2, OutputNode = 1},
            new() {InputNode = 1, OutputNode = 3},
            new() {InputNode = 3, OutputNode = 2},
        };
        Assert.True(genome.HasCycle());
    }

    [Test]
    public void GetDeltaTest()
    {
        var other = DefaultGenome;
        var genome = DefaultGenome;

        Assert.AreEqual(0, other.GetDelta(genome, 1000, 1000, 1000, 10), 0.0001);

        genome.ConnectionGenes[0] = genome.ConnectionGenes[0].WithWeight(1); //-1
        genome.ConnectionGenes[1] = genome.ConnectionGenes[1].WithWeight(-4); //-2
        genome.ConnectionGenes[2] = genome.ConnectionGenes[2].WithWeight(2); //+4
        genome.ConnectionGenes[3] = genome.ConnectionGenes[3].WithWeight(1); //0

        Assert.AreEqual(7.0 / 4.0 * 1000, other.GetDelta(genome, 1, 1, 1000, 10), 0.0001);
        Assert.AreEqual(7.0 / 4.0 * 2000, genome.GetDelta(other, 1, 1, 2000, 10), 0.0001);

        genome = DefaultGenome;
        genome.ConnectionGenes = genome.ConnectionGenes.Append(new() { InnovationNumber = 4 }).ToArray();
        Assert.AreEqual(10, genome.GetDelta(other, 10, 30, 60, 100), 0.0001);
        Assert.AreEqual(20, other.GetDelta(genome, 20, 30, 60, 100), 0.0001);
        Assert.AreEqual(20.0 / 5.0, genome.GetDelta(other, 20, 30, 60, 0), 0.0001);
        Assert.AreEqual(40.0 / 5.0, other.GetDelta(genome, 40, 30, 60, 0), 0.0001);

        genome = DefaultGenome;
        genome.ConnectionGenes = genome.ConnectionGenes.Append(new() { InnovationNumber = 4 }).Append(new() { InnovationNumber = 5 }).ToArray();
        other.ConnectionGenes = other.ConnectionGenes.Append(new() { InnovationNumber = 5 }).ToArray();
        Assert.AreEqual(30, genome.GetDelta(other, 10, 30, 60, 100), 0.0001);
        Assert.AreEqual(40, other.GetDelta(genome, 20, 40, 60, 100), 0.0001);
        Assert.AreEqual(30.0 / 6.0, genome.GetDelta(other, 20, 30, 60, 0), 0.0001);
        Assert.AreEqual(40.0 / 6.0, other.GetDelta(genome, 10, 40, 60, 0), 0.0001);
    }

    [Test]
    public void GetNodesDepthTest()
    {
        Assert.AreEqual((new int[] { 0, 0, 0, 2, 2, 1 }, 2), DefaultGenome.GetNodesDepth());
    }

    [Test]
    public void ToPhenomeTest()
    {
        Assert.DoesNotThrow(() => DefaultGenome.ToPhenome());
        Assert.IsNotNull(DefaultGenome.ToPhenome());
    }
}
