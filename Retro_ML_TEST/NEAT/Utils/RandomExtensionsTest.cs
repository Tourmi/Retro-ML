using NUnit.Framework;
using Retro_ML.NEAT.Utils;
using Retro_ML_TEST.Mocks;

namespace Retro_ML_TEST.NEAT.Utils;
[TestFixture]
internal class RandomExtensionsTest
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private RandomMock random;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [SetUp]
    public void Setup()
    {
        random = new RandomMock();
    }

    [Test]
    public void RandomFromListTest()
    {
        var list = new string[] { "a", "b", "c", "d", "e" };
        random.SetInt(0);
        Assert.AreEqual("a", random.RandomFromList(list));
        random.SetInt(1);
        Assert.AreEqual("b", random.RandomFromList(list));
        random.SetInt(2);
        Assert.AreEqual("c", random.RandomFromList(list));
        random.SetInt(3);
        Assert.AreEqual("d", random.RandomFromList(list));
        random.SetInt(4);
        Assert.AreEqual("e", random.RandomFromList(list));
    }

    [Test]
    public void RandomFromWeightedListTest()
    {
        var list = new string[] { "a", "b", "c", "d", "e" };
        var weights = new double[] { 1, 2, 3, 4, 5 };
        var totalWeights = 15d;

        random.SetDouble(0);
        Assert.AreEqual("a", random.RandomFromWeightedList(list, weights, totalWeights));

        random.SetDouble(0.99 / totalWeights);
        Assert.AreEqual("a", random.RandomFromWeightedList(list, weights, totalWeights));

        random.SetDouble(1 / totalWeights);
        Assert.AreEqual("b", random.RandomFromWeightedList(list, weights, totalWeights));

        random.SetDouble(3 / totalWeights);
        Assert.AreEqual("c", random.RandomFromWeightedList(list, weights, totalWeights));

        random.SetDouble(6 / totalWeights);
        Assert.AreEqual("d", random.RandomFromWeightedList(list, weights, totalWeights));

        random.SetDouble(10 / totalWeights);
        Assert.AreEqual("e", random.RandomFromWeightedList(list, weights, totalWeights));
        random.SetDouble(0.9999999999);
        Assert.AreEqual("e", random.RandomFromWeightedList(list, weights, totalWeights));
    }

    [Test]
    public void RandomNodeTest()
    {
        var nodeDepths = new int[] { 0, 0, 0, 3, 3, 1, 1, 2, 1, };

        random.SetInt(0);
        Assert.AreEqual(0, random.RandomNode(nodeDepths, 0, 3));
        Assert.AreEqual(5, random.RandomNode(nodeDepths, 1, 2));
        Assert.AreEqual(3, random.RandomNode(nodeDepths, 1, 3));
        Assert.AreEqual(7, random.RandomNode(nodeDepths, 2, 2));

        random.SetInt(1);
        Assert.AreEqual(1, random.RandomNode(nodeDepths, 0, 3));
        Assert.AreEqual(6, random.RandomNode(nodeDepths, 1, 2));
        Assert.AreEqual(4, random.RandomNode(nodeDepths, 1, 3));
    }
}
