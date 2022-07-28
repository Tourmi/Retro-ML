using NUnit.Framework;
using Retro_ML.Utils.Game.Geometry3D;
using System;

namespace Retro_ML_TEST.Utils.Game.Geometry3D;

[TestFixture]
internal class SphereTest
{
    private const float EPSILON = 0.001f;
    private static readonly Sphere DEFAULT_SPHERE = new Sphere(new(1, 2, 3), 3.5f);

    [Test]
    public void MinX() => Assert.AreEqual(-2.5f, DEFAULT_SPHERE.MinX, EPSILON);
    [Test]
    public void MaxX() => Assert.AreEqual(4.5f, DEFAULT_SPHERE.MaxX, EPSILON);
    [Test]
    public void MinY() => Assert.AreEqual(-1.5f, DEFAULT_SPHERE.MinY, EPSILON);
    [Test]
    public void MaxY() => Assert.AreEqual(5.5f, DEFAULT_SPHERE.MaxY, EPSILON);
    [Test]
    public void MinZ() => Assert.AreEqual(-0.5f, DEFAULT_SPHERE.MinZ, EPSILON);
    [Test]
    public void MaxZ() => Assert.AreEqual(6.5f, DEFAULT_SPHERE.MaxZ, EPSILON);
    [Test]
    public void AABB()
    {
        var aabb = DEFAULT_SPHERE.AABB;

        VectorTest.AssertVectorEquals(new(-2.5f, -1.5f, -0.5f), aabb.MinCorner);
        VectorTest.AssertVectorEquals(new(4.5f, 5.5f, 6.5f), aabb.MaxCorner);
    }
    [Test]
    public void Contains()
    {
        Assert.IsTrue(DEFAULT_SPHERE.Contains(new(0, 0, 1)));
        Assert.IsTrue(DEFAULT_SPHERE.Contains(new(4.5f, 2, 3)));
        Assert.IsTrue(DEFAULT_SPHERE.Contains(new(1, 2, 3)));
        Assert.IsFalse(DEFAULT_SPHERE.Contains(new(0, 0, 0)));
        Assert.IsFalse(DEFAULT_SPHERE.Contains(new(-2, 0, 0)));
    }
    [Test]
    public void GetRaytrace() => Assert.Throws<NotImplementedException>(() => DEFAULT_SPHERE.GetRaytrace(new Ray()), "Update this test if this method is implemented");
}
