using NUnit.Framework;
using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML_TEST.Utils.Game.Geometry3D;

[TestFixture]
internal class AABBTest
{
    private const float EPSILON = 0.001f;
    private readonly AABB DEFAULT_AABB = new(new Vector(1, 2, 3), new Vector(4, -5, 6));

    [Test]
    public void Constructor()
    {
        var aabb = new AABB(new Vector(1, 2, 3), -6f);
        VectorTest.AssertVectorEquals(new Vector(-2f, -1f, 0f), aabb.MinCorner);
        VectorTest.AssertVectorEquals(new Vector(4f, 5f, 6f), aabb.MaxCorner);
    }

    [Test]
    public void MinX() => Assert.AreEqual(-1f, DEFAULT_AABB.MinX, EPSILON);
    [Test]
    public void MaxX() => Assert.AreEqual(3f, DEFAULT_AABB.MaxX, EPSILON);
    [Test]
    public void MinY() => Assert.AreEqual(-0.5f, DEFAULT_AABB.MinY, EPSILON);
    [Test]
    public void MaxY() => Assert.AreEqual(4.5f, DEFAULT_AABB.MaxY, EPSILON);
    [Test]
    public void MinZ() => Assert.AreEqual(0f, DEFAULT_AABB.MinZ, EPSILON);
    [Test]
    public void MaxZ() => Assert.AreEqual(6f, DEFAULT_AABB.MaxZ, EPSILON);
    [Test]
    public void AABB() => Assert.AreEqual(DEFAULT_AABB, ((IRaytracable)DEFAULT_AABB).AABB);
    [Test]
    public void MinCorner() => VectorTest.AssertVectorEquals(new(-1f, -0.5f, 0f), DEFAULT_AABB.MinCorner);
    [Test]
    public void MaxCorner() => VectorTest.AssertVectorEquals(new(3f, 4.5f, 6f), DEFAULT_AABB.MaxCorner);
    [Test]
    public void FullyContains()
    {
        Assert.IsTrue(DEFAULT_AABB.FullyContains(DEFAULT_AABB));
        Assert.IsTrue(DEFAULT_AABB.FullyContains(new AABB(Vector.Origin, 0f)));
        Assert.IsTrue(DEFAULT_AABB.FullyContains(new AABB(new(0, 1, 2), 1f)));
        Assert.IsFalse(DEFAULT_AABB.FullyContains(new AABB(new(3f, 4.5f, 6f), 1f)));
        Assert.IsFalse(DEFAULT_AABB.FullyContains(new AABB(new(1f, 2f, 3f), new Vector(3f, 4f, 6.5f))));
        Assert.IsFalse(DEFAULT_AABB.FullyContains(new AABB(new(1f, 2f, 3f), new Vector(3f, 5.5f, 3f))));
        Assert.IsFalse(DEFAULT_AABB.FullyContains(new AABB(new(1f, 2f, 3f), new Vector(4.5f, 4f, 3f))));
    }
    [Test]
    public void Contains()
    {
        Assert.IsTrue(DEFAULT_AABB.Contains(Vector.Origin));
        Assert.IsTrue(DEFAULT_AABB.Contains(new(1, 2, 3)));
        Assert.IsTrue(DEFAULT_AABB.Contains(new(3, 4.5f, 6)));
        Assert.IsTrue(DEFAULT_AABB.Contains(new(-1f, -0.5f, 0f)));
        Assert.IsFalse(DEFAULT_AABB.Contains(new(-1.25f, 2, 3)));
        Assert.IsFalse(DEFAULT_AABB.Contains(new(1, 5f, 3)));
        Assert.IsFalse(DEFAULT_AABB.Contains(new(1, 2, 6.5f)));
    }
    [Test]
    public void GetRaytrace()
    {
        Assert.AreEqual(1f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 2, 7), new Vector(0, 0, -1))), EPSILON);
        Assert.AreEqual(2f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 6.5f, 3), new Vector(0, -1, 0))), EPSILON);
        Assert.AreEqual(3f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(6, 2, 3), new Vector(-1, 0, 0))), EPSILON);
        Assert.AreEqual(4f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 2, -4f), new Vector(0, 0, 1))), EPSILON);
        Assert.AreEqual(5f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, -5.5f, 3), new Vector(0, 1, 0))), EPSILON);
        Assert.AreEqual(6f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(-7f, 2, 3), new Vector(1, 0, 0))), EPSILON);
        Assert.AreEqual(4f, DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 2, 4), new Vector(0, 0, -1))), EPSILON, "Should be distance to interior side of AABB");

        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 2, 10), new Vector(0, 0, 1))));
        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 10, 3), new Vector(0, 1, 0))));
        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(10, 2, 3), new Vector(1, 0, 0))));
        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, 2, -10), new Vector(0, 0, -1))));
        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, -10, 3), new Vector(0, -1, 0))));
        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(-10, 2, 3), new Vector(-1, 0, 0))));

        Assert.IsNaN(DEFAULT_AABB.GetRaytrace(new Ray(new Vector(1, -10, 7), new Vector(0, 1, 0))));
    }

}
