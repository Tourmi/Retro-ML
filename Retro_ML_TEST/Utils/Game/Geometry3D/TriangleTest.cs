using NUnit.Framework;
using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML_TEST.Utils.Game.Geometry3D;

[TestFixture]
internal class TriangleTest
{
    private const float EPSILON = 0.0001f;
    private static readonly Triangle DEFAULT_TRIANGLE = new(new(-1, 5, 3), new(4, 2, -3), new(1, -2, 6), new Vector(-1, 5, 3).Cross(new(4, 2, -3)));

    [Test]
    public void MinX() => Assert.AreEqual(-1f, DEFAULT_TRIANGLE.MinX, EPSILON);
    [Test]
    public void MaxX() => Assert.AreEqual(4f, DEFAULT_TRIANGLE.MaxX, EPSILON);
    [Test]
    public void MinY() => Assert.AreEqual(-2f, DEFAULT_TRIANGLE.MinY, EPSILON);
    [Test]
    public void MaxY() => Assert.AreEqual(5f, DEFAULT_TRIANGLE.MaxY, EPSILON);
    [Test]
    public void MinZ() => Assert.AreEqual(-3f, DEFAULT_TRIANGLE.MinZ, EPSILON);
    [Test]
    public void MaxZ() => Assert.AreEqual(6f, DEFAULT_TRIANGLE.MaxZ, EPSILON);

    [Test]
    public void AABB()
    {
        var aabb = DEFAULT_TRIANGLE.AABB;

        VectorTest.AssertVectorEquals(new(-1, -2, -3), aabb.MinCorner);
        VectorTest.AssertVectorEquals(new(4, 5, 6), aabb.MaxCorner);
    }

    [Test]
    public void GetRaytrace()
    {
        Assert.AreEqual(2.63793f, DEFAULT_TRIANGLE.GetRaytrace(new Ray(new(0, 3.5f, 0), new(0, 0, 1))), EPSILON, "Should have hit triangle");
        Assert.IsNaN(DEFAULT_TRIANGLE.GetRaytrace(new Ray(new(0, 0.5f, 0), new(0, 0, 1))), "Should have missed triangle");
        Assert.IsNaN(DEFAULT_TRIANGLE.GetRaytrace(new Ray(new(0, 3.5f, 2.65f), new(0, 0, -1))), "Hit the triangle from the wrong side");
        Assert.IsNaN(DEFAULT_TRIANGLE.GetRaytrace(new Ray(new(0, 3.5f, 2.65f), new(0, 0, 1))), "Ray starts from the other side of triangle");
    }

    [Test]
    public void Contains()
    {
        Assert.IsFalse(DEFAULT_TRIANGLE.Contains(new(-1, 5, 3)));
        Assert.IsFalse(DEFAULT_TRIANGLE.Contains(Vector.Origin));
        Assert.IsFalse(DEFAULT_TRIANGLE.Contains(Vector.NaN));
    }
}
