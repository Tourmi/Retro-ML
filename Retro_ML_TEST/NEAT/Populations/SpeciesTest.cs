using NUnit.Framework;
using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Populations;
using Retro_ML.NEAT.Training;

namespace Retro_ML_TEST.NEAT.Populations;
[TestFixture]
internal class SpeciesTest
{
    [Test]
    public void SortGenomesTest()
    {
        var species = new Species()
        {
            Genomes = new()
            {
                new Genome() {Fitness = new Fitness(1)},
                new Genome() {Fitness = new Fitness(5)},
                new Genome() {Fitness = new Fitness(0.5)}
            }
        };

        species.SortGenomes();

        Assert.AreEqual(new Fitness(5), species.Genomes[0].Fitness);
        Assert.AreEqual(new Fitness(1), species.Genomes[1].Fitness);
        Assert.AreEqual(new Fitness(0.5), species.Genomes[2].Fitness);
    }
}
