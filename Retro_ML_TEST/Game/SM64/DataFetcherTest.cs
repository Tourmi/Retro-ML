using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.SuperMario64.Game;
using Retro_ML.Utils.Game.Geometry3D;
using Retro_ML_TEST.Emulator;
using Retro_ML_TEST.Utils.Game.Geometry3D;
using System;
using static Retro_ML.SuperMario64.Game.Addresses;

namespace Retro_ML_TEST.Game.SM64;
[TestFixture]
internal class DataFetcherTest
{
    private const float EPSILON = 0.001f;

    private SM64DataFetcher? df;
    private MockEmulatorAdapter? emu;

    [SetUp]
    public void SetUp()
    {
        emu = new MockEmulatorAdapter();
        df = new SM64DataFetcher(emu, new NeuralConfig(), new SM64PluginConfig());
        df.NextState();
    }

    [Test]
    public void FrameCache()
    {
        Assert.AreEqual(0f, df!.GetMarioPos().X, EPSILON);
        emu!.SetMemory(Addresses.Mario.XPos.Address, 0x3f, 0x80, 0x00, 0x00);
        Assert.AreEqual(0f, df!.GetMarioPos().X, EPSILON, "The cache should not have been updated");
        df!.NextFrame();
        Assert.AreEqual(1f, df!.GetMarioPos().X, EPSILON, "The cache should have been updated");
        emu!.SetMemory(Addresses.Mario.XPos.Address, 0x40, 0x80, 0x00, 0x00);
        df!.NextState();
        Assert.AreEqual(4f, df!.GetMarioPos().X, EPSILON, "The cache should have been updated");
    }

    [Test]
    public void NextFrame() => Assert.DoesNotThrow(() => df!.NextFrame());
    [Test]
    public void NextState() => Assert.DoesNotThrow(() => df!.NextState());
    [Test]
    public void GetMarioAction() => TestUintFunction(Mario.Action, df!.GetMarioAction);
    [Test]
    public void GetMarioX() => TestFloatFunction(Mario.XPos, df!.GetMarioX);
    [Test]
    public void GetMarioY() => TestFloatFunction(Mario.YPos, df!.GetMarioY);
    [Test]
    public void GetMarioZ() => TestFloatFunction(Mario.ZPos, df!.GetMarioZ);
    [Test]
    public void GetMarioPos() => TestVectorFunction(df!.GetMarioPos, Mario.XPos, Mario.YPos, Mario.ZPos);
    [Test]
    public void GetMarioXSpeed() => TestFloatFunction(Mario.XSpeed, df!.GetMarioXSpeed);
    [Test]
    public void GetMarioYSpeed() => TestFloatFunction(Mario.YSpeed, df!.GetMarioYSpeed);
    [Test]
    public void GetMarioZSpeed() => TestFloatFunction(Mario.ZSpeed, df!.GetMarioZSpeed);
    [Test]
    public void GetMarioSpeed() => TestVectorFunction(df!.GetMarioSpeed, Mario.XSpeed, Mario.YSpeed, Mario.ZSpeed);
    [Test]
    public void GetCameraHorizontalAngle() => TestUShortFunction(Camera.HorizontalAngle, df!.GetCameraHorizontalAngle);
    [Test]
    public void GetMarioHorizontalSpeed() => TestFloatFunction(Mario.HorizontalSpeed, df!.GetMarioHorizontalSpeed);
    [Test]
    public void GetMarioFacingAngle() => TestUShortFunction(Mario.FacingAngle, df!.GetMarioFacingAngle);
    [Test]
    public void GetMarioCapFlags() => TestUShortFunction(Mario.HatFlags, df!.GetMarioCapFlags);
    [Test]
    public void GetMarioHealth() => TestByteFunction(Mario.Health, df!.GetMarioHealth);
    [Test]
    public void GetCoinCount() => TestUShortFunction(Mario.Coins, df!.GetCoinCount);
    [Test]
    public void GetStarCount() => TestUShortFunction(Progress.StarCount, df!.GetStarCount);
    [Test]
    public void GetBehaviourBankStart() => TestUintFunction(GameObjects.BehaviourBankStartAddress, df!.GetBehaviourBankStart);
    [Test]
    public void GetAreaCode() => TestByteFunction(Area.CurrentID, df!.GetAreaCode);
    [Test]
    public void GetStaticTriangleCount() => TestShortFunction(Collision.StaticTriangleCount, df!.GetStaticTriangleCount);
    [Test]
    public void GetTriangleCount() => TestShortFunction(Collision.TotalTriangleCount, df!.GetTriangleCount);

    [Test]
    public void IsMarioGrounded()
    {
        Assert.IsFalse(df!.IsMarioGrounded());
        emu!.SetMemory(Mario.Action.Address, 0x0C, 0x40, 0x02, 0x01);
        df!.NextFrame();
        Assert.IsTrue(df!.IsMarioGrounded());
    }

    [Test]
    public void IsMarioSwimming()
    {
        Assert.IsFalse(df!.IsMarioSwimming());
        emu!.SetMemory(Mario.Action.Address, 0x30, 0x00, 0x24, 0xD0);
        df!.NextFrame();
        Assert.IsTrue(df!.IsMarioSwimming());
    }

    [Test]
    public void GetMarioFacingAngleRadian()
    {
        Assert.AreEqual(0f, df!.GetMarioFacingAngleRadian(), EPSILON);
        emu!.SetMemory(Mario.FacingAngle.Address, 0x10, 0x20);
        df!.NextFrame();
        Assert.AreEqual(-(0x1020 / (float)ushort.MaxValue) * MathF.Tau, df!.GetMarioFacingAngleRadian());
    }

    [Test]
    public void GetMarioNormalizedHealth()
    {
        Assert.AreEqual(0f, df!.GetMarioNormalizedHealth(), EPSILON);
        emu!.SetMemory(Mario.Health.Address, 8);
        df!.NextFrame();
        Assert.AreEqual(1f, df!.GetMarioNormalizedHealth(), EPSILON);
    }

    private void TestByteFunction(AddressData address, Func<byte> function)
    {
        Assert.AreEqual(0, function());
        emu!.SetMemory(address.Address, 0x10);
        df!.NextFrame();
        Assert.AreEqual(0x10, function());
        emu!.SetMemory(address.Address, 0xFF);
        df!.NextFrame();
        Assert.AreEqual(0xFF, function());
    }

    private void TestUShortFunction(AddressData address, Func<ushort> function)
    {
        Assert.AreEqual(0, function());
        emu!.SetMemory(address.Address, 0x10, 0x20);
        df!.NextFrame();
        Assert.AreEqual(0x1020, function());
        emu!.SetMemory(address.Address, 0xFF, 0xFE);
        df!.NextFrame();
        Assert.AreEqual(0xFFFE, function());
    }
    private void TestShortFunction(AddressData address, Func<short> function)
    {
        Assert.AreEqual(0, function());
        emu!.SetMemory(address.Address, 0x10, 0x20);
        df!.NextFrame();
        Assert.AreEqual((short)0x1020, function());
        emu!.SetMemory(address.Address, 0xFF, 0xFE);
        df!.NextFrame();
        Assert.AreEqual(-2, function());
    }

    private void TestUintFunction(AddressData address, Func<uint> function)
    {
        Assert.AreEqual(0, function());
        emu!.SetMemory(address.Address, 0x10, 0x20, 0x40, 0x80);
        df!.NextFrame();
        Assert.AreEqual(0x10204080, function());
        emu!.SetMemory(address.Address, 0xFF, 0xFE, 0xFD, 0xFC);
        df!.NextFrame();
        Assert.AreEqual(0xFFFEFDFC, function());
    }

    private void TestFloatFunction(AddressData address, Func<float> function)
    {
        Assert.AreEqual(0, function());
        emu!.SetMemory(address.Address, 0x3F, 0x80, 0x00, 0x00);
        df!.NextFrame();
        Assert.AreEqual(1f, function(), EPSILON);
        emu!.SetMemory(address.Address, 0xFF, 0x80, 0x00, 0x00);
        df!.NextFrame();
        Assert.AreEqual(float.NegativeInfinity, function(), EPSILON);
        emu!.SetMemory(address.Address, 0xC6, 0x40, 0xE6, 0xB7);
        df!.NextFrame();
        Assert.AreEqual(-12345.6789f, function());
    }

    private void TestVectorFunction(Func<Vector> function, params AddressData[] addresses)
    {
        Assert.AreEqual(3, addresses.Length, "Should have 3 addresses for a vector");
        VectorTest.AssertVectorEquals(Vector.Origin, function());
        emu!.SetMemory(addresses[0].Address, 0x3F, 0x80, 0x00, 0x00); //1f
        emu!.SetMemory(addresses[1].Address, 0xC2, 0x20, 0x00, 0x00); //-40f
        emu!.SetMemory(addresses[2].Address, 0x7f, 0xff, 0xff, 0xff); //NaN
        df!.NextFrame();
        VectorTest.AssertVectorEquals(new Vector(1f, -40f, float.NaN), function());
    }
}
