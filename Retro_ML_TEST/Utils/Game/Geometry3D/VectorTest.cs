using NUnit.Framework;
using Retro_ML.Utils.Game.Geometry3D;
using System;

namespace Retro_ML_TEST.Utils.Game.Geometry3D;

[TestFixture]
internal class VectorTest
{
    private const float EPSILON = 0.001f;

    [Test]
    public void Constructor()
    {
        AssertVectorEquals(new(3), new(3, 3, 3));
        AssertVectorEquals(new(0), new());
        AssertVectorEquals(new(5, 5, 5), new(5));
    }

    [Test]
    public void SquaredLength()
    {
        Assert.AreEqual(4 * 4 + 3 * 3 + 7.5f * 7.5f, new Vector(-4, 3, 7.5f).SquaredLength, EPSILON);
        Assert.AreEqual(0, new Vector(0f).SquaredLength, EPSILON);
    }

    [Test]
    public void Length()
    {
        Assert.AreEqual(9.013878f, new Vector(-4, 3, 7.5f).Length, EPSILON);
        Assert.AreEqual(0, new Vector(0f).Length, EPSILON);
    }

    [Test]
    public void Origin() => AssertVectorEquals(new(), Vector.Origin);
    [Test]
    public void NaN() => AssertVectorEquals(new(float.NaN), Vector.NaN);

    [Test]
    public void WithX() => AssertVectorEquals(new(-1, 2, 3), new Vector(1, 2, 3).WithX(-1));
    [Test]
    public void WithY() => AssertVectorEquals(new(1, -1, 3), new Vector(1, 2, 3).WithY(-1));
    [Test]
    public void WithZ() => AssertVectorEquals(new(1, 2, -1), new Vector(1, 2, 3).WithZ(-1));
    [Test]
    public void WithXY() => AssertVectorEquals(new(-1, -2, 3), new Vector(1, 2, 3).WithXY(-1, -2));
    [Test]
    public void WithXZ() => AssertVectorEquals(new(-1, 2, -2), new Vector(1, 2, 3).WithXZ(-1, -2));
    [Test]
    public void WithYZ() => AssertVectorEquals(new(1, -1, -2), new Vector(1, 2, 3).WithYZ(-1, -2));
    [Test]
    public void Inverse()
    {
        AssertVectorEquals(new(1), new Vector(1).Inverse());
        AssertVectorEquals(new(0.5f, -0.33333f, 0.25f), new Vector(2, -3, 4).Inverse());
        AssertVectorEquals(new(float.PositiveInfinity, float.NegativeInfinity, 0.25f), new Vector(0f, -0f, 4).Inverse());
    }
    [Test]
    public void Absolute()
    {
        AssertVectorEquals(new(1, 2, 3), new Vector(-1, -2, -3).Absolute());
        AssertVectorEquals(new(1, 2, 3), new Vector(1, 2, 3).Absolute());
        AssertVectorEquals(new(float.PositiveInfinity), new Vector(float.NegativeInfinity).Absolute());
    }
    [Test]
    public void RotateVertically()
    {
        AssertVectorEquals(new(0, 5, 0), new Vector(3, 0, 4).RotateVertically(MathF.Tau / 4));
        AssertVectorEquals(new(0, -5, 0), new Vector(3, 0, 4).RotateVertically(-MathF.Tau / 4));
        AssertVectorEquals(new(-3, 0, -4), new Vector(3, 0, 4).RotateVertically(MathF.Tau / 2));
        AssertVectorEquals(new(0.70710678f, 0.70710678f, 0), new Vector(1, 0, 0).RotateVertically(MathF.Tau / 8));
        AssertVectorEquals(Vector.NaN, new Vector(0, 1, 0).RotateVertically(MathF.Tau));
    }
    [Test]
    public void RotateXY() => AssertVectorEquals(new(-1, -2, 3), new Vector(1, 2, 3).RotateXY(MathF.Tau / 2));
    [Test]
    public void RotateXZ() => AssertVectorEquals(new(-1, 2, -3), new Vector(1, 2, 3).RotateXZ(MathF.Tau / 2));
    [Test]
    public void RotateYZ() => AssertVectorEquals(new(1, -2, -3), new Vector(1, 2, 3).RotateYZ(MathF.Tau / 2));
    [Test]
    public void AngleBetween()
    {
        Assert.AreEqual(MathF.Tau / 2, new Vector(1, 2, 3).AngleBetween(new Vector(-1, -2, -3)), EPSILON);
        Assert.AreEqual(MathF.Tau / 4, new Vector(0, 1, 0).AngleBetween(new Vector(1, 0, 0)), EPSILON);
        Assert.AreEqual(0f, new Vector(0, 1, 0).AngleBetween(new Vector(0, 3, 0)), EPSILON);
    }
    [Test]
    public void Normalized()
    {
        AssertVectorEquals(new(0.70710678f, -0.70710678f, 0), new Vector(50, -50, 0).Normalized());
        AssertVectorEquals(new(0, 0.70710678f, -0.70710678f), new Vector(0, 0.70710678f, -0.70710678f).Normalized());
        AssertVectorEquals(Vector.NaN, new Vector(0, 0, 0).Normalized());
    }
    [Test]
    public void Dot()
    {
        Assert.AreEqual(0f, new Vector(0).Dot(new Vector(1, 2, 3)), EPSILON);
        Assert.AreEqual(14f, new Vector(1, 2, 3).Dot(new Vector(1, 2, 3)), EPSILON);
        Assert.AreEqual(-4f, new Vector(1, -2, 3).Dot(new Vector(1, -2, -3)), EPSILON);
    }
    [Test]
    public void Cross() => AssertVectorEquals(new(0, 0, 1), new Vector(1, 0, 0).Cross(new Vector(0, 1, 0)));
    [Test]
    public void Plus()
    {
        AssertVectorEquals(new(1, 2, 3), +new Vector(1, 2, 3));
        AssertVectorEquals(new(5, 7, 9), new Vector(1, 2, 3) + new Vector(4, 5, 6));
        AssertVectorEquals(new(6, 7, 8), new Vector(1, 2, 3) + 5f);
    }
    [Test]
    public void Minus()
    {
        AssertVectorEquals(new(-1, -2, -3), -new Vector(1, 2, 3));
        AssertVectorEquals(new(-3, -3, -3), new Vector(1, 2, 3) - new Vector(4, 5, 6));
        AssertVectorEquals(new(-2, -1, 0), new Vector(1, 2, 3) - 3f);
    }
    [Test]
    public void Multiply() => AssertVectorEquals(new(3, 6, 9), 3f * new Vector(1, 2, 3));
    [Test]
    public void Divide() => AssertVectorEquals(new(1, 2, 3), new Vector(3, 6, 9) / 3f);

    public static void AssertVectorEquals(Vector expected, Vector actual)
    {
        Assert.AreEqual(expected.X, actual.X, EPSILON, "Expected: {{{0}; {1}; {2}}}  Actual: {{{3}; {4}; {5}}}", expected.X, expected.Y, expected.Z, actual.X, actual.Y, actual.Z);
        Assert.AreEqual(expected.Y, actual.Y, EPSILON, "Expected: {{{0}; {1}; {2}}}  Actual: {{{3}; {4}; {5}}}", expected.X, expected.Y, expected.Z, actual.X, actual.Y, actual.Z);
        Assert.AreEqual(expected.Z, actual.Z, EPSILON, "Expected: {{{0}; {1}; {2}}}  Actual: {{{3}; {4}; {5}}}", expected.X, expected.Y, expected.Z, actual.X, actual.Y, actual.Z);
    }
}
