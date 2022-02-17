using NUnit.Framework;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML_TEST.Emulator;

namespace SMW_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class DataFetcherTest
    {
        delegate bool TestFunction();

        private DataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new DataFetcher(mockEmulatorAdapter);
        }

        [Test]
        public void FrameCache()
        {
            Assert.True(dataFetcher!.CanAct());
            mockEmulatorAdapter!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x03);
            Assert.True(dataFetcher!.CanAct(), "The cache should not have updated");
            dataFetcher.NextFrame();
            Assert.False(dataFetcher!.CanAct(), "The cache should have updated");
            mockEmulatorAdapter!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x00);
            dataFetcher.NextLevel();
            Assert.True(dataFetcher!.CanAct(), "The cache should have updated");
        }

        [Test]
        public void LevelCache()
        {
            Assert.False(dataFetcher!.IsWaterLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.Level.IsWater.Address, 0x01);
            Assert.False(dataFetcher!.IsWaterLevel(), "The cache should not have been updated");
            dataFetcher!.NextFrame();
            Assert.False(dataFetcher!.IsWaterLevel(), "The cache should not have been updated");
            dataFetcher!.NextLevel();
            Assert.True(dataFetcher!.IsWaterLevel(), "The cache should have been updated");
        }

        [Test]
        public void NextFrame()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextFrame());
        }

        [Test]
        public void NextLevel()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextLevel());
        }

        [Test]
        public void GetPositionX()
        {
            Assert.AreEqual(0, dataFetcher!.GetPositionX());

            //Big endian bytes
            mockEmulatorAdapter!.SetMemory(Addresses.Player.PositionX.Address, new byte[] { 0xFF, 0x01 });
            dataFetcher.NextFrame();
            Assert.AreEqual(0x01FF, dataFetcher!.GetPositionX());
        }

        [Test]
        public void GetPositionY()
        {
            Assert.AreEqual(0, dataFetcher!.GetPositionY());

            //Big endian bytes
            mockEmulatorAdapter!.SetMemory(Addresses.Player.PositionY.Address, new byte[] { 0xFF, 0x01 });

            dataFetcher.NextFrame();
            Assert.AreEqual(0x01FF, dataFetcher!.GetPositionY());
        }

        [Test]
        public void IsOnGround()
        {
            TestFlagFunction(dataFetcher!.IsOnGround, Addresses.Player.IsOnGround.Address, 0x02);
            mockEmulatorAdapter!.SetMemory(Addresses.Player.IsOnGround.Address, 0x00);
            dataFetcher!.NextFrame();
            TestFlagFunction(dataFetcher!.IsOnGround, Addresses.Player.IsOnSolidSprite.Address, 0x03);
        }

        [Test]
        public void CanAct()
        {
            TestFlagFunction(dataFetcher!.CanAct, Addresses.Player.PlayerAnimationState.Address, 0x03, true);
        }

        [Test]
        public void IsDead()
        {
            TestFlagFunction(dataFetcher!.IsDead, Addresses.Player.PlayerAnimationState.Address, 0x09);
        }

        [Test]
        public void WonLevel()
        {
            TestFlagFunction(dataFetcher!.WonLevel, Addresses.Level.EndLevelTimer.Address, 0xFF);
        }

        [Test]
        public void IsInWater()
        {
            TestFlagFunction(dataFetcher!.IsInWater, Addresses.Player.IsInWater.Address, 0x01);
        }

        [Test]
        public void CanJumpOutOfWater()
        {
            TestFlagFunction(dataFetcher!.CanJumpOutOfWater, Addresses.Player.CanJumpOutOfWater.Address, 0x01);
        }

        [Test]
        public void IsSinking()
        {
            TestFlagFunction(dataFetcher!.IsSinking, Addresses.Player.AirFlag.Address, 0x24);
        }

        [Test]
        public void IsRaising()
        {
            TestFlagFunction(dataFetcher!.IsRaising, Addresses.Player.AirFlag.Address, 0x0B);
        }

        [Test]
        public void IsCarryingSomething()
        {
            TestFlagFunction(dataFetcher!.IsCarryingSomething, Addresses.Player.IsCarryingSomething.Address, 0x01);
        }

        [Test]
        public void CanClimb()
        {
            TestFlagFunction(dataFetcher!.CanClimb, Addresses.Player.CanClimb.Address, 0b00001011);
            mockEmulatorAdapter!.SetMemory(Addresses.Player.CanClimb.Address, 0x00);
            dataFetcher!.NextFrame();
            TestFlagFunction(dataFetcher!.CanClimb, Addresses.Player.CanClimbOnAir.Address, 0x01);
        }

        [Test]
        public void IsAtMaxSpeed()
        {
            TestFlagFunction(dataFetcher!.IsAtMaxSpeed, Addresses.Player.DashTimer.Address, 0x70);
        }

        [Test]
        public void WasDialogBoxOpened()
        {
            TestFlagFunction(dataFetcher!.WasDialogBoxOpened, Addresses.Level.TextBoxTriggered.Address, 0x01);
        }

        [Test]
        public void IsWaterLevel()
        {
            TestFlagFunction(dataFetcher!.IsWaterLevel, Addresses.Level.IsWater.Address, 0x01);
        }

        [Test]
        public void WasInternalClockTriggered()
        {
            for (int i = 0; i < DataFetcher.INTERNAL_CLOCK_LENGTH; i++)
            {
                dataFetcher!.NextFrame();
                Assert.False(dataFetcher!.WasInternalClockTriggered());
            }
            for (int i = 0; i < DataFetcher.INTERNAL_CLOCK_LENGTH; i++)
            {
                dataFetcher!.NextFrame();
                Assert.True(dataFetcher!.WasInternalClockTriggered());
            }

            dataFetcher!.NextFrame();
            Assert.False(dataFetcher!.WasInternalClockTriggered());
        }

        [Test]
        public void GetWalkableTilesAroundPosition()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void GetDangerousTilesAroundPosition()
        {
            Assert.Ignore("Not implemented yet");
        }

        private void TestFlagFunction(TestFunction tf, uint address, byte newVal, bool invertResult = false)
        {
            bool result = tf();
            if (invertResult) result = !result;
            Assert.False(result);

            mockEmulatorAdapter!.SetMemory(address, newVal);
            dataFetcher!.NextLevel();
            result = tf();
            if (invertResult) result = !result;
            Assert.True(result);
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
