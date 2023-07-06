using NUnit.Framework;
using Retro_ML.NEAT.Utils;
using System;
using System.Linq;

namespace Retro_ML_TEST.NEAT.Utils;
[TestFixture]
internal class ActivationFunctionsTest
{
    [Test]
    public void GetAvailableActivationFunctionsTest()
    {
        Assert.Contains("Linear", ActivationFunctions.GetAvailableActivationFunctions().ToList());
        Assert.Contains("ReLU", ActivationFunctions.GetAvailableActivationFunctions().ToList());
        Assert.Contains("LeakyReLU", ActivationFunctions.GetAvailableActivationFunctions().ToList());
        Assert.Contains("Tanh", ActivationFunctions.GetAvailableActivationFunctions().ToList());

        Assert.AreEqual(4, ActivationFunctions.GetAvailableActivationFunctions().ToList().Count, "Wrong activation function count. Update tests");
    }

    [Test]
    public void DefaultTest()
    {
        var func = ActivationFunctions.GetActivationFunction(null);

        //We should get the linear function back
        Assert.AreEqual(-12345, func(-12345), 0.0001);
        Assert.AreEqual(12345.6, func(12345.6), 0.0001);
    }

    [Test]
    public void LinearTest()
    {
        var func = ActivationFunctions.GetActivationFunction("Linear");
        Assert.AreEqual(-12345, func(-12345), 0.0001);
        Assert.AreEqual(12345.6, func(12345.6), 0.0001);
    }

    [Test]
    public void ReLUTest()
    {
        var func = ActivationFunctions.GetActivationFunction("ReLU");
        Assert.AreEqual(0, func(-12345), 0.0001);
        Assert.AreEqual(12345.6, func(12345.6), 0.0001);
    }

    [Test]
    public void LeakyReLUTest()
    {
        var func = ActivationFunctions.GetActivationFunction("LeakyReLU");
        Assert.AreEqual(-1234.5, func(-12345), 0.0001);
        Assert.AreEqual(12345.6, func(12345.6), 0.0001);
    }

    [Test]
    public void TanhTest()
    {
        var func = ActivationFunctions.GetActivationFunction("Tanh");
        Assert.AreEqual(Math.Tanh(-12345), func(-12345), 0.0001);
        Assert.AreEqual(Math.Tanh(12345.6), func(12345.6), 0.0001);
    }
}
