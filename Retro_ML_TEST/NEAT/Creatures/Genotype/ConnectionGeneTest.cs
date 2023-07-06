using NUnit.Framework;
using Retro_ML.NEAT.Creatures.Genotype;

namespace Retro_ML_TEST.NEAT.Creatures.Genotype;
[TestFixture]
internal class ConnectionGeneTest
{
    [Test]
    public void WithWeightTest()
    {
        var gene = new ConnectionGene() { InputNode = 1, OutputNode = 2, Enabled = true, Weight = 4.0, InnovationNumber = 3 };

        Assert.AreNotEqual(gene, gene.WithWeight(10));
        gene.Weight = 10;
        Assert.AreEqual(gene, gene.WithWeight(10));
    }
}
