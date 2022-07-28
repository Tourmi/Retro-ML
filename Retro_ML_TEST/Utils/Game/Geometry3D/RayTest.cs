using NUnit.Framework;
using Retro_ML.Utils.Game.Geometry3D;
using System;

namespace Retro_ML_TEST.Utils.Game.Geometry3D;

[TestFixture]
internal class RayTest
{
    private const float EPSILON = 0.0001f;

    [Test]
    public void Constructor()
    {
        VectorTest.AssertVectorEquals(new Vector(1, 2, 3), new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6)).P);
        VectorTest.AssertVectorEquals(new Vector(4, 5, 6), new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6)).Direction);
    }

    [Test]
    public void InverseDirection() => VectorTest.AssertVectorEquals(new Vector(1, 2, 3).Normalized().Inverse(), new Ray(new(), new Vector(1, 2, 3).Normalized()).InverseDirection);
    [Test]
    public void RotateVertically() => AssertRayEquals(new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized().RotateVertically(MathF.Tau / 8)), new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized()).RotateVertically(MathF.Tau / 8));
    [Test]
    public void RotateXY() => AssertRayEquals(new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized().RotateXY(MathF.Tau / 8)), new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized()).RotateXY(MathF.Tau / 8));
    [Test]
    public void RotateXZ() => AssertRayEquals(new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized().RotateXZ(MathF.Tau / 8)), new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized()).RotateXZ(MathF.Tau / 8));
    [Test]
    public void RotateYZ() => AssertRayEquals(new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized().RotateYZ(MathF.Tau / 8)), new Ray(new Vector(1, 2, 3), new Vector(4, 5, 6).Normalized()).RotateYZ(MathF.Tau / 8));

    public static void AssertRayEquals(Ray expected, Ray actual)
    {
        VectorTest.AssertVectorEquals(expected.P, actual.P);
        VectorTest.AssertVectorEquals(expected.Direction, actual.Direction);
    }
}
