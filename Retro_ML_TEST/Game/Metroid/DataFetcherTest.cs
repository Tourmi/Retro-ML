using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Metroid.Game;
using Retro_ML_TEST.Mocks;
using System;

namespace Retro_ML_TEST.Game.Metroid
{
    [TestFixture]
    internal class DataFetcherTest
    {
        delegate bool TestFunction();

        private MetroidDataFetcher? df;
        private MockEmulatorAdapter? emu;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new MetroidDataFetcher(emu, new NeuralConfig(), new MetroidPluginConfig());
            df.NextFrame();
        }

        [Test]
        public void FrameCache()
        {
            Assert.AreEqual(0, df!.GetSamusXPosition());
            emu!.SetMemory(Addresses.Samus.XPosition.Address, 50);
            Assert.AreEqual(0, df!.GetSamusXPosition(), "The cache should not have been updated");
            df!.NextFrame();
            Assert.AreEqual(50, df!.GetSamusXPosition(), "The cache should have been updated");
            emu!.SetMemory(Addresses.Samus.XPosition.Address, 100);
            df!.NextState();
            Assert.AreEqual(100, df!.GetSamusXPosition(), "The cache should have been updated");
        }

        [Test]
        public void NextFrame() => Assert.DoesNotThrow(() => df!.NextFrame());

        [Test]
        public void NextState() => Assert.DoesNotThrow(() => df!.NextState());

        [Test]
        public void GetSamusHealth()
        {
            Assert.AreEqual(0, df!.GetSamusHealth());

            emu!.SetMemory(Addresses.Samus.Health.Address, 0x34, 0x12);
            df.NextFrame();
            Assert.AreEqual(1234, df.GetSamusHealth());
        }

        [Test]
        public void GetMaximumHealth()
        {
            Assert.AreEqual(999, df!.GetMaximumHealth());

            emu!.SetMemory(Addresses.Progress.EnergyTanks.Address, 3);
            df.NextFrame();
            Assert.AreEqual(3999, df.GetMaximumHealth());
        }

        [Test]
        public void GetSamusHealthRatio()
        {
            Assert.AreEqual(0.0, df!.GetSamusHealthRatio(), 0.0001);

            emu!.SetMemory(Addresses.Progress.EnergyTanks.Address, 3);
            emu!.SetMemory(Addresses.Samus.Health.Address, 0x80, 0x21);
            df.NextFrame();
            Assert.AreEqual(2180.0 / 3999.0, df.GetSamusHealthRatio());
        }

        [Test]
        public void GetSamusYSpeed() => SpeedTest(df!.GetSamusYSpeed, Addresses.Samus.VerticalSpeed, Addresses.Samus.VerticalFractionalSpeed);

        [Test]
        public void GetSamusXSpeed() => SpeedTest(df!.GetSamusXSpeed, Addresses.Samus.HorizontalSpeed, Addresses.Samus.HorizontalFractionalSpeed);

        private void SpeedTest(Func<short> funcToTest, Addresses.AddressData msbAddress, Addresses.AddressData lsbAddress)
        {
            Assert.AreEqual(0, funcToTest());

            emu!.SetMemory(msbAddress.Address, 0x40);
            emu!.SetMemory(lsbAddress.Address, 0x50);
            df!.NextFrame();
            Assert.AreEqual(0x4000 + 0x50, funcToTest());

            emu!.SetMemory(msbAddress.Address, unchecked((byte)-0x10));
            df!.NextFrame();
            Assert.AreEqual(-0x1000 + 0x50, funcToTest());
        }

        [Test]
        public void GetSamusXPosition() => TestByteValue(df!.GetSamusXPosition, Addresses.Samus.XPosition.Address);

        [Test]
        public void GetSamusYPosition() => TestByteValue(df!.GetSamusYPosition, Addresses.Samus.YPosition.Address);

        [Test]
        public void GetSamusScreensPosition()
        {
            emu!.SetMemory(Addresses.Samus.YPosition.Address, (byte)(4 * df!.RealTileSize));
            emu!.SetMemory(Addresses.Samus.XPosition.Address, (byte)(5 * df!.RealTileSize));
            df!.NextFrame();
            Assert.AreEqual((5, 4), df!.GetSamusScreensPosition());

            emu!.SetMemory(Addresses.Room.HorizontalOrVertical.Address, 0b1000);
            emu!.SetMemory(Addresses.Room.PPUCTL0.Address, 0b0011);
            df.NextFrame();
            Assert.AreEqual((5, 4 + df.RealRoomHeight), df.GetSamusScreensPosition());

            emu!.SetMemory(Addresses.Room.HorizontalOrVertical.Address, 0b0000);
            df.NextFrame();
            Assert.AreEqual((5 + df.RealRoomWidth, 4), df.GetSamusScreensPosition());

            emu!.SetMemory(Addresses.Samus.CurrentScreen.Address, 1);
            df.NextFrame();
            Assert.AreEqual((5, 4), df!.GetSamusScreensPosition());
        }

        [Test]
        public void SamusLookDirection()
        {
            Assert.AreEqual(1.0, df!.SamusLookDirection(), 0.000001);

            emu!.SetMemory(Addresses.Samus.LookingDirection.Address, 1);
            df!.NextState();
            Assert.AreEqual(-1.0, df!.SamusLookDirection(), 0.000001);
        }

        [Test]
        public void IsSamusInMorphBall() => TestFlagValue(df!.IsSamusInMorphBall, Addresses.Samus.Status.Address, 0x03, false);

        [Test]
        public void IsSamusInLava() => TestFlagValue(df!.IsSamusInLava, Addresses.Samus.InLava.Address, 1, false);

        [Test]
        public void IsMetroidOnSamusHead() => TestFlagValue(df!.IsMetroidOnSamusHead, Addresses.Samus.HasMetroidOnHead.Address, 1, false);

        [Test]
        public void IsSamusOnElevator() => TestFlagValue(df!.IsSamusOnElevator, Addresses.Samus.IsOnElevator.Address, 1, false);

        [Test]
        public void IsSamusUsingMissiles() => TestFlagValue(df!.IsSamusUsingMissiles, Addresses.Samus.UsingMissiles.Address, 1, false);

        [Test]
        public void IsSamusInDoor() => TestFlagValue(df!.IsSamusInDoor, Addresses.Gamestate.InADoor.Address, 1, false);

        [Test]
        public void IsSamusInFirstScreen() => TestFlagValue(df!.IsSamusInFirstScreen, Addresses.Samus.CurrentScreen.Address, 1, true);

        [Test]
        public void IsSamusFrozen() => TestFlagValue(df!.IsSamusFrozen, Addresses.Gamestate.FreezeTimer.Address, 10, false);

        [Test]
        public void IsSamusGrounded() => TestFlagValue(df!.IsSamusGrounded, Addresses.Samus.VerticalFractionalSpeed.Address, 10, true);

        [Test]
        public void IsBossPresent() => TestFlagValue(df!.IsBossPresent, Addresses.Gamestate.IsMiniBossPresent.Address, 1, false);

        [Test]
        public void HasBombs() => TestFlagValue(df!.HasBombs, Addresses.Progress.Equipment.Address, 0b0000_0001, false);
        
        [Test]
        public void HasHighJump() => TestFlagValue(df!.HasHighJump, Addresses.Progress.Equipment.Address, 0b0000_0010, false);
        
        [Test]
        public void HasLongBeam() => TestFlagValue(df!.HasLongBeam, Addresses.Progress.Equipment.Address, 0b0000_0100, false);
        
        [Test]
        public void HasScrewAttack() => TestFlagValue(df!.HasScrewAttack, Addresses.Progress.Equipment.Address, 0b0000_1000, false);
        
        [Test]
        public void HasMorphBall() => TestFlagValue(df!.HasMorphBall, Addresses.Progress.Equipment.Address, 0b0001_0000, false);
        
        [Test]
        public void HasVariaSuit() => TestFlagValue(df!.HasVariaSuit, Addresses.Progress.Equipment.Address, 0b0010_0000, false);
        
        [Test]
        public void HasWaveBeam() => TestFlagValue(df!.HasWaveBeam, Addresses.Progress.Equipment.Address, 0b0100_0000, false);
        
        [Test]
        public void HasIceBeam() => TestFlagValue(df!.HasIceBeam, Addresses.Progress.Equipment.Address, 0b1000_0000, false);
        
        [Test]
        public void MultipleEquipments() => TestFlagValue(df!.HasIceBeam, Addresses.Progress.Equipment.Address, 0b1111_1111, false);
        
        [Test]
        public void HasMissiles() => TestFlagValue(df!.HasMissiles, Addresses.Progress.MissileCapacity.Address, 5, false);
        
        [Test]
        public void KilledKraid() => Assert.Ignore("Function not properly implemented");
        
        [Test]
        public void KilledRidley() => Assert.Ignore("Function not properly implemented");
        
        [Test]
        public void KilledMotherBrain() => Assert.Ignore("Function not properly implemented");

        [Test]
        public void GetSamusInvincibilityTimerRatio()
        {
            Assert.AreEqual(0.0, df!.GetSamusInvincibilityTimerRatio(), 0.00001);

            emu!.SetMemory(Addresses.Samus.InvincibleTimer.Address, 25);
            df.NextFrame();
            Assert.AreEqual(25.0 / MetroidDataFetcher.INVINCIBILITY_TIMER_LENGTH, df!.GetSamusInvincibilityTimerRatio(), 0.00001);
        }

        [Test]
        public void GetCurrentMissileRatio()
        {
            Assert.AreEqual(0.0, df!.GetCurrentMissileRatio(), 0.00001);

            emu!.SetMemory(Addresses.Progress.MissileCapacity.Address, 10);
            emu.SetMemory(Addresses.Progress.Missiles.Address, 5);
            df.NextFrame();
            Assert.AreEqual(0.5, df.GetCurrentMissileRatio(), 0.00001);
        }

        [Test]
        public void GetSamusYSpeedRatio() => TestSpeedRatio(df!.GetSamusYSpeedRatio, Addresses.Samus.VerticalSpeed, Addresses.Samus.VerticalFractionalSpeed, MetroidDataFetcher.MAXIMUM_VERTICAL_SPEED);

        [Test]
        public void GetSamusXSpeedRatio() => TestSpeedRatio(df!.GetSamusXSpeedRatio, Addresses.Samus.HorizontalSpeed, Addresses.Samus.HorizontalFractionalSpeed, MetroidDataFetcher.MAXIMUM_HORIZONTAL_SPEED);

        private void TestSpeedRatio(Func<double> funcToTest, Addresses.AddressData msbAddress, Addresses.AddressData lsbAddress, int max)
        {
            Assert.AreEqual(0.0, funcToTest(), 0.0001);

            emu!.SetMemory(lsbAddress.Address, 0x50);
            emu!.SetMemory(msbAddress.Address, unchecked((byte)-0x2));
            df!.NextFrame();
            Assert.AreEqual((-0x200 + 0x50) / (double)max, funcToTest(), 0.00001);
        }

        [Test]
        public void IsHorizontalRoom() => TestFlagValue(df!.IsHorizontalRoom, Addresses.Room.HorizontalOrVertical.Address, 0b1000, true);

        [Test]
        public void IsOnNameTable3() => TestFlagValue(df!.IsOnNameTable3, Addresses.Room.PPUCTL0.Address, 0b0011, false);

        [Test]
        public void DoorOnNameTable0() => TestDoorOnNameTable(df!.DoorOnNameTable0, Addresses.Room.DoorOnNameTable0);

        [Test]
        public void DoorOnNameTable3() => TestDoorOnNameTable(df!.DoorOnNameTable3, Addresses.Room.DoorOnNameTable3);

        private void TestDoorOnNameTable(Func<bool, bool> funcToTest, Addresses.AddressData address)
        {
            Assert.IsFalse(funcToTest(false));
            Assert.IsFalse(funcToTest(true));

            emu!.SetMemory(address.Address, 0b01);
            df!.NextState();
            Assert.IsFalse(funcToTest(false));
            Assert.IsTrue(funcToTest(true));

            emu!.SetMemory(address.Address, 0b11);
            df!.NextState();
            Assert.IsTrue(funcToTest(false));
            Assert.IsTrue(funcToTest(true));

            emu!.SetMemory(address.Address, 0b10);
            df!.NextState();
            Assert.IsTrue(funcToTest(false));
            Assert.IsFalse(funcToTest(true));
        }

        [Test]
        public void GetScrollX() => TestByteValue(df!.GetScrollX, Addresses.Room.ScrollX.Address);

        [Test]
        public void GetScrollY() => TestByteValue(df!.GetScrollY, Addresses.Room.ScrollY.Address);

        [Test]
        public void GetMapPosition()
        {
            Assert.AreEqual(((byte, byte))(0, 0), df!.GetMapPosition());

            emu!.SetMemory(Addresses.Gamestate.MapX.Address, 5);
            emu!.SetMemory(Addresses.Gamestate.MapY.Address, 10);
            df.NextFrame();
            Assert.AreEqual(((byte, byte))(5, 10), df!.GetMapPosition());
        }

        [Test]
        public void GetLastScrollDirection()
        {
            emu!.SetMemory(Addresses.Gamestate.LastScrollDirection.Address, 0);
            df!.NextFrame();
            Assert.AreEqual((0, -1), df!.GetLastScrollDirection());

            emu!.SetMemory(Addresses.Gamestate.LastScrollDirection.Address, 1);
            df!.NextFrame();
            Assert.AreEqual((0, 1), df!.GetLastScrollDirection());

            emu!.SetMemory(Addresses.Gamestate.LastScrollDirection.Address, 2);
            df!.NextFrame();
            Assert.AreEqual((-1, 0), df!.GetLastScrollDirection());

            emu!.SetMemory(Addresses.Gamestate.LastScrollDirection.Address, 3);
            df!.NextFrame();
            Assert.AreEqual((1, 0), df!.GetLastScrollDirection());
        }

        [Test]
        public void CanSamusAct()
        {
            Assert.IsTrue(df!.CanSamusAct());

            emu!.SetMemory(Addresses.Gamestate.InADoor.Address, 1);
            df.NextFrame();
            Assert.IsFalse(df!.CanSamusAct());

            emu!.SetMemory(Addresses.Gamestate.InADoor.Address, 0);
            emu!.SetMemory(Addresses.Gamestate.FreezeTimer.Address, 10);
            df.NextFrame();
            Assert.IsFalse(df!.CanSamusAct());
        }

        private void TestByteValue(Func<byte> funcToTest, uint address)
        {
            Assert.AreEqual((ushort)0, funcToTest());

            emu!.SetMemory(address, 0xFE);
            df!.NextState();
            Assert.AreEqual((ushort)0xFE, funcToTest());
        }

        private void TestFlagValue(Func<bool> funcToTest, uint address, byte valueToSet, bool startsTrue)
        {
            Assert.AreEqual(startsTrue, funcToTest());

            emu!.SetMemory(address, valueToSet);
            df!.NextState();
            Assert.AreEqual(!startsTrue, funcToTest());
        }
    }
}
