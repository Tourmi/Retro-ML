using NUnit.Framework;
using System;

namespace Retro_ML_TEST.Mocks;

internal class RandomMock : Random
{
    private int nextInt;
    private double nextDouble;

    public RandomMock() { }

    public void SetInt(int val) => nextInt = val;
    public void SetDouble(double val) => nextDouble = val;

    public override int Next(int maxValue) => Next(0, maxValue);
    public override int Next(int minValue, int maxValue)
    {
        Assert.GreaterOrEqual(nextInt, minValue);
        Assert.Less(nextInt, maxValue);

        return nextInt;
    }
    public override double NextDouble()
    {
        Assert.GreaterOrEqual(nextDouble, 0);
        Assert.Less(nextDouble, 1);

        return nextDouble;
    }
}