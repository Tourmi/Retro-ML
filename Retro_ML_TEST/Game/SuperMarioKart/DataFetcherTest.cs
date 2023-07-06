using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperMarioKart.Configuration;
using Retro_ML.SuperMarioKart.Game;
using Retro_ML_TEST.Mocks;
using System;

namespace Retro_ML_TEST.Game.SuperMarioKart
{
    [TestFixture]
    internal class DataFetcherTest
    {
        private SMKDataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new SMKDataFetcher(mockEmulatorAdapter, new NeuralConfig(), new SMKPluginConfig());
            dataFetcher.NextFrame();
        }


        [Test]
        public void FrameCache()
        {
            Assert.False(dataFetcher!.IsItemReady());
            mockEmulatorAdapter!.SetMemory(Addresses.Race.ItemState.Address, 0xC0);
            Assert.False(dataFetcher!.IsItemReady(), "The cache should not have updated");
            dataFetcher.NextFrame();
            Assert.True(dataFetcher!.IsItemReady(), "The cache should have updated");
            mockEmulatorAdapter!.SetMemory(Addresses.Race.ItemState.Address, 0x00);
            dataFetcher.NextState();
            Assert.False(dataFetcher!.IsItemReady(), "The cache should have updated");
        }

        [Test]
        public void StateCache()
        {
            Assert.True(dataFetcher!.IsRace());
            mockEmulatorAdapter!.SetMemory(Addresses.Race.Type.Address, 0x04);
            dataFetcher.NextFrame();
            Assert.True(dataFetcher!.IsRace(), "The cache should not have been updated");
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsRace(), "The cache should not have been updated");
            dataFetcher!.NextState();
            Assert.False(dataFetcher!.IsRace(), "The cache should have been updated");
        }

        [Test]
        public void NextFrame()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextFrame());
        }

        [Test]
        public void NextLevel()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextState());
        }

        [Test]
        public void GetPositionX()
        {
            TestUShortValue(dataFetcher!.GetPositionX, Addresses.Racers.XPosition.Address);
        }

        [Test]
        public void GetPositionY()
        {
            TestUShortValue(dataFetcher!.GetPositionY, Addresses.Racers.YPosition.Address);
        }

        [Test]
        public void GetHeadingAngle()
        {
            TestUShortValue(dataFetcher!.GetHeadingAngle, Addresses.Racers.HeadingAngle.Address);
        }

        [Test]
        public void GetHeadingAngleRadian()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetHeadingAngleRadians(), 0.001);

            mockEmulatorAdapter!.SetMemory(Addresses.Racers.HeadingAngle.Address, 0x00, 0x40);
            dataFetcher!.NextFrame();
            Assert.AreEqual(Math.PI / 2.0, dataFetcher!.GetHeadingAngleRadians(), 0.001);
        }

        [Test]
        public void GetKartStatus()
        {
            TestByteValue(dataFetcher!.GetKartStatus, Addresses.Racers.KartStatus.Address);
        }

        [Test]
        public void GetMaxCheckpoint()
        {
            TestByteValue(dataFetcher!.GetMaxCheckpoint, Addresses.Race.CheckpointCount.Address);
        }

        [Test]
        public void GetCurrentCheckpoint()
        {
            TestByteValue(dataFetcher!.GetCurrentCheckpoint, Addresses.Racers.CurrentCheckpointNumber.Address);
        }

        [Test]
        public void GetCurrentLap()
        {
            Assert.AreEqual((sbyte)-128, dataFetcher!.GetCurrentLap());

            mockEmulatorAdapter!.SetMemory(Addresses.Racers.CurrentLap.Address, 0x85);
            dataFetcher!.NextFrame();
            Assert.AreEqual((sbyte)5, dataFetcher!.GetCurrentLap());
        }

        [Test]
        public void IsOffroad()
        {
            TestFlagValue(dataFetcher!.IsOffroad, Addresses.Racers.OnRoad.Address, 0x10, false);
        }

        [Test]
        public void GetTrackNumber()
        {
            TestUShortValue(dataFetcher!.GetTrackNumber, Addresses.Racetrack.Number.Address);
        }

        [Test]
        public void GetCollisionTimer()
        {
            TestUShortValue(dataFetcher!.GetCollisionTimer, Addresses.Racers.CollisionTimer.Address);
        }

        [Test]
        public void GetRaceStatus()
        {
            TestByteValue(dataFetcher!.GetRaceStatus, Addresses.Race.RaceStatus.Address);
        }

        [Test]
        public void GetCoins()
        {
            TestByteValue(dataFetcher!.GetCoins, Addresses.Race.Coins.Address);
        }

        [Test]
        public void IsItemReady()
        {
            TestFlagValue(dataFetcher!.IsItemReady, Addresses.Race.ItemState.Address, 0xC0, false);
        }

        [Test]
        public void GetRanking()
        {
            Assert.AreEqual(0, dataFetcher!.GetRanking());

            mockEmulatorAdapter!.SetMemory(Addresses.Racers.CurrentRank.Address, 0x08);
            dataFetcher!.NextFrame();
            Assert.AreEqual(4, dataFetcher!.GetRanking());
        }

        [Test]
        public void IsRace()
        {
            TestFlagValue(dataFetcher!.IsRace, Addresses.Race.Type.Address, 0x4, true);
        }

        [Test]
        public void GetSpeedOutOfMaxSpeed()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetSpeedOutOfMaxSpeed(), 0.00001);

            mockEmulatorAdapter!.SetMemory(Addresses.Racers.MaximumSpeed.Address, 0x32, 0xFE);
            mockEmulatorAdapter!.SetMemory(Addresses.Racers.AbsoluteSpeed.Address, 0x1F, 0x6C);
            dataFetcher!.NextFrame();

            Assert.AreEqual(0x6C1F / (double)0xFE32, dataFetcher!.GetSpeedOutOfMaxSpeed(), 0.00001);
        }

        [Test]
        public void GetCurrentItem()
        {
            var defaultItems = dataFetcher!.GetCurrentItem();
            for (int i = 0; i < defaultItems.GetLength(0); i++)
            {
                for (int j = 0; j < defaultItems.GetLength(1); j++)
                {
                    Assert.IsFalse(defaultItems[i, j], "Items should all be false");
                }
            }

            mockEmulatorAdapter!.SetMemory(Addresses.Race.ItemId.Address, 0x03);
            dataFetcher!.NextFrame();
            for (int i = 0; i < defaultItems.GetLength(0); i++)
            {
                for (int j = 0; j < defaultItems.GetLength(1); j++)
                {
                    Assert.IsFalse(defaultItems[i, j], "Item isn't marked as ready");
                }
            }
            mockEmulatorAdapter!.SetMemory(Addresses.Race.ItemState.Address, 0xC0);
            dataFetcher!.NextFrame();

            defaultItems = dataFetcher!.GetCurrentItem();

            for (int i = 0; i < defaultItems.GetLength(0); i++)
            {
                for (int j = 0; j < defaultItems.GetLength(1); j++)
                {
                    if (i == 1 && j == 0)
                    {
                        Assert.IsTrue(defaultItems[i, j]);
                    }
                    else
                    {
                        Assert.IsFalse(defaultItems[i, j]);
                    }
                }
            }
        }

        private void TestUShortValue(Func<ushort> funcToTest, uint address)
        {
            Assert.AreEqual((ushort)0, funcToTest());

            mockEmulatorAdapter!.SetMemory(address, 0xFE, 0x32);
            dataFetcher!.NextState();
            Assert.AreEqual((ushort)0x32FE, funcToTest());
        }

        private void TestByteValue(Func<byte> funcToTest, uint address)
        {
            Assert.AreEqual((ushort)0, funcToTest());

            mockEmulatorAdapter!.SetMemory(address, 0xFE);
            dataFetcher!.NextState();
            Assert.AreEqual((ushort)0xFE, funcToTest());
        }

        private void TestFlagValue(Func<bool> funcToTest, uint address, byte valueToSet, bool startsTrue)
        {
            Assert.AreEqual(startsTrue, funcToTest());

            mockEmulatorAdapter!.SetMemory(address, valueToSet);
            dataFetcher!.NextState();
            Assert.AreEqual(!startsTrue, funcToTest());
        }
    }
}
