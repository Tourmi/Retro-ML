using Retro_ML.NEAT.Training;

namespace Retro_ML_TEST.NEAT.Training;
[TestFixture]
internal class FitnessTest
{
    [Test]
    public void MaxMinTest()
    {
        var fitness1 = new Fitness(5, 3, 2, 1);
        var fitness2 = new Fitness(4, 1, 2, 3);

        Assert.AreEqual((fitness1, fitness2), Fitness.MaxMin(fitness1, fitness2));
        Assert.AreEqual((fitness1, fitness2), Fitness.MaxMin(fitness2, fitness1));
    }

    [Test]
    public void CompareToTest()
    {
        var fitness1 = new Fitness(5, 3, 2, 1);
        var fitness2 = new Fitness(4);

        Assert.Greater(fitness1.CompareTo(fitness2), 0);
        Assert.Less(fitness2.CompareTo(fitness1), 0);

        fitness2 = new Fitness(5, 3, 2, 0);
        Assert.Greater(fitness1.CompareTo(fitness2), 0);
        Assert.Less(fitness2.CompareTo(fitness1), 0);
    }

    [Test]
    public void EqualsTest()
    {
        var fitness1 = new Fitness(5, 3, 2, 1);
        var fitness2 = new Fitness(4);

        Assert.False(fitness1.Equals(null));
        Assert.False(fitness1.Equals(new { }));
        Assert.False(fitness1.Equals(Fitness.Zero));
        Assert.False(fitness1.Equals(fitness2));
        Assert.AreNotEqual(fitness1.GetHashCode(), fitness2.GetHashCode());
        fitness2 = new Fitness(5);
        Assert.False(fitness1.Equals(fitness2));
        Assert.False(fitness2.Equals(fitness1));
        fitness2 = new Fitness(5, 3, 2);
        Assert.False(fitness1.Equals(fitness2));
        Assert.False(fitness2.Equals(fitness1));
        fitness2 = new Fitness(5, 3, 2, 2);
        Assert.False(fitness1.Equals(fitness2));
        Assert.False(fitness2.Equals(fitness1));
        fitness2 = new Fitness(5, 3, 2, 1);
        Assert.True(fitness1.Equals(fitness2));
        Assert.AreEqual(fitness1.GetHashCode(), fitness2.GetHashCode());
    }

    [Test]
    public void ToStringTest()
    {
        var fitness = new Fitness(5, 3, 2, 1);

        Assert.IsNotNull(fitness.ToString());
        Assert.IsTrue(fitness.ToString()!.Contains("5"));
        Assert.IsTrue(fitness.ToString()!.Contains("3"));
        Assert.IsTrue(fitness.ToString()!.Contains("2"));
        Assert.IsTrue(fitness.ToString()!.Contains("1"));
    }

    [Test]
    public void OperatorsTest()
    {
        var fitness = new Fitness(5, 3, 2, 1);

        Assert.False(fitness == new Fitness(4, 1));
        Assert.True(fitness != new Fitness(4, 1));
        Assert.True(fitness == new Fitness(5, 3, 2, 1));
        Assert.False(fitness != new Fitness(5, 3, 2, 1));

        Assert.True(fitness < new Fitness(10, 1, 1));
        Assert.False(fitness < new Fitness(5, 3, 2, 1));
        Assert.True(fitness < new Fitness(5, 3, 2, 2));
        Assert.False(fitness < new Fitness(5, 3, 2, 0));

        Assert.True(fitness <= new Fitness(10, 1, 1));
        Assert.True(fitness <= new Fitness(5, 3, 2, 1));
        Assert.True(fitness <= new Fitness(5, 3, 2, 2));
        Assert.False(fitness <= new Fitness(5, 3, 2, 0));

        Assert.False(fitness > new Fitness(10, 1, 1));
        Assert.False(fitness > new Fitness(5, 3, 2, 1));
        Assert.False(fitness > new Fitness(5, 3, 2, 2));
        Assert.True(fitness > new Fitness(5, 3, 2, 0));

        Assert.False(fitness >= new Fitness(10, 1, 1));
        Assert.True(fitness >= new Fitness(5, 3, 2, 1));
        Assert.False(fitness >= new Fitness(5, 3, 2, 2));
        Assert.True(fitness >= new Fitness(5, 3, 2, 0));

        Assert.AreEqual(new Fitness(6, 4, 3, 2, 1), fitness + new Fitness(1, 1, 1, 1, 1));
        Assert.AreEqual(fitness, +fitness);
        Assert.AreEqual(new Fitness(-5, -3, -2, -1), -fitness);
        Assert.AreEqual(new Fitness(4, 2, 1, 0, -1), fitness - new Fitness(1, 1, 1, 1, 1));
        Assert.AreEqual(new Fitness(10, 6, 4, 2), fitness * 2);
        Assert.AreEqual(new Fitness(10, 6, 4, 2), 2 * fitness);
        Assert.AreEqual(new Fitness(2.5, 1.5, 1, 0.5), fitness / 2);
    }
}
