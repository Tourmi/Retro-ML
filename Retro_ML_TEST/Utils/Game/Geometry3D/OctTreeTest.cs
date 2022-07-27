using NUnit.Framework;
using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML_TEST.Utils.Game.Geometry3D;

[TestFixture]
internal class OctTreeTest
{
    private const float EPSILON = 0.001f;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private OctTree defaultScene;
    private int defaultObjCount;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [SetUp]
    public void SetUp()
    {
        defaultScene = new OctTree(new Vector(50f, 60f, 70f), 1000f, 1f, 8, 4);
        defaultScene.AddObject(new AABB(new Vector(100, 100, 100), 50f, isStatic: true));
        defaultScene.AddObject(new AABB(new Vector(100, 200, 100), 50f, isStatic: true));
        defaultScene.AddObject(new AABB(new Vector(100, 300, 100), 50f, isStatic: true));
        defaultScene.AddObject(new AABB(new Vector(-100, -100, -100), 50f, isStatic: true));
        defaultScene.AddObject(new Triangle(new Vector(100, 150, -100), new Vector(150, 100, -100), new Vector(50, 100, -100), new Vector(0, 0, -1), isStatic: true));

        defaultObjCount = 6;
    }

    [Test]
    public void MinX() => Assert.AreEqual(-450f, defaultScene.MinX, EPSILON);
    [Test]
    public void MaxX() => Assert.AreEqual(550f, defaultScene.MaxX, EPSILON);
    [Test]
    public void MinY() => Assert.AreEqual(-440f, defaultScene.MinY, EPSILON);
    [Test]
    public void MaxY() => Assert.AreEqual(560f, defaultScene.MaxY, EPSILON);
    [Test]
    public void MinZ() => Assert.AreEqual(-430f, defaultScene.MinZ, EPSILON);
    [Test]
    public void MaxZ() => Assert.AreEqual(570f, defaultScene.MaxZ, EPSILON);
    [Test]
    public void AABB()
    {
        var aabb = defaultScene.AABB;
        VectorTest.AssertVectorEquals(new Vector(-450f, -440f, -430f), aabb.MinCorner);
        VectorTest.AssertVectorEquals(new Vector(550, 560, 570), aabb.MaxCorner);
    }

    [Test]
    public void CountAndUpdate()
    {
        Assert.AreEqual(defaultObjCount, defaultScene.Count);

        defaultScene.AddObject(new AABB(new Vector(300, -250, 200), 0f, isStatic: true));
        defaultScene.AddObject(new AABB(new Vector(300, 250, 200), 0f, isStatic: false));
        defaultScene.AddObject(new AABB(new Vector(300, 250, -200), 0f, isStatic: true));
        Assert.AreEqual(defaultObjCount + 3, defaultScene.Count);

        defaultScene.Update();
        Assert.AreEqual(defaultObjCount + 2, defaultScene.Count, "Dynamic object should have been removed");

        defaultScene.AddObjects(new AABB(), new AABB(), new AABB(), new AABB(), new AABB());
        Assert.AreEqual(defaultObjCount + 7, defaultScene.Count);
    }

    [Test]
    public void Contains()
    {
        Assert.IsTrue(defaultScene.Contains(new Vector(100, 100, 100)), "Should be inside first AABB");
        Assert.IsTrue(defaultScene.Contains(new Vector(-110, 105, -95)), "Should be inside sphere");
        Assert.IsFalse(defaultScene.Contains(Vector.Origin), "No objects at the origin");
        Assert.IsFalse(defaultScene.Contains(Vector.NaN), "NaN vector should always be false");
    }

    [Test]
    public void AddObject()
    {
        Assert.IsFalse(defaultScene.Contains(Vector.Origin), "No objects at the origin");
        Assert.AreEqual(defaultObjCount, defaultScene.Count);
        defaultScene.AddObject(new Sphere(Vector.Origin, 5f));
        Assert.IsTrue(defaultScene.Contains(Vector.Origin), "New sphere at the origin");
        Assert.AreEqual(defaultObjCount + 1, defaultScene.Count);
    }

    [Test]
    public void AddObjects()
    {
        Assert.IsFalse(defaultScene.Contains(Vector.Origin), "No objects at the origin");
        Assert.AreEqual(defaultObjCount, defaultScene.Count);

        defaultScene.AddObjects(new Sphere(Vector.Origin, 5f), new Sphere(Vector.Origin.WithX(10), 5f));
        Assert.IsTrue(defaultScene.Contains(Vector.Origin), "New sphere 1 at the origin");
        Assert.IsTrue(defaultScene.Contains(Vector.Origin.WithX(10)), "New sphere 2 next to the origin");
        Assert.AreEqual(defaultObjCount + 2, defaultScene.Count);
    }

    [Test]
    public void GetRayTrace()
    {
        Assert.AreEqual(700f - (50f / 2f), defaultScene.GetRaytrace(new Ray(new Vector(100, 1000, 100), new Vector(0, -1, 0))), EPSILON, "Should have collided with AABB #3");
        Assert.AreEqual(25f, defaultScene.GetRaytrace(new Ray(new Vector(100, 250, 100), new Vector(0, -1, 0))), EPSILON, "Should have collided with AABB #2");
        Assert.IsNaN(defaultScene.GetRaytrace(new Ray(new Vector(0, 1000, 0), new Vector(0, -1, 0))), "Should have missed all scene objects");
    }
}
