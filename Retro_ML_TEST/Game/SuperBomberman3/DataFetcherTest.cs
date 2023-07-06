using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperBomberman3.Configuration;
using Retro_ML.SuperBomberman3.Game;
using Retro_ML.Utils;
using System;
using Retro_ML_TEST.Mocks;

namespace Retro_ML_TEST.Game.SuperBomberman3
{
    [TestFixture]
    internal class DataFetcherTest
    {
        private SB3DataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new SB3DataFetcher(mockEmulatorAdapter, new NeuralConfig(), new SB3PluginConfig());
            dataFetcher.NextFrame();
        }

        [Test]
        public void FrameCache()
        {
            Assert.True(dataFetcher!.IsMainPlayerOnLouie());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.IsOnLouie.Address, 0x01);
            Assert.True(dataFetcher!.IsMainPlayerOnLouie(), "The cache should not have updated");
            dataFetcher!.NextFrame();
            Assert.False(dataFetcher!.IsMainPlayerOnLouie(), "The cache should have updated");
        }

        [Test]
        public void NextFrame()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextFrame());
        }

        [Test]
        public void NextState()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextState());
        }

        [Test]
        public void GetRemainingRoundTime()
        {
            Assert.AreEqual(0, dataFetcher!.GetRemainingRoundTime());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.GameMinutesTimer.Address, 0x01);
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.AreEqual(61, dataFetcher!.GetRemainingRoundTime());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x08);
            dataFetcher!.NextFrame();
            Assert.AreEqual(68, dataFetcher!.GetRemainingRoundTime());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.GameMinutesTimer.Address, 0x02);
            dataFetcher!.NextFrame();
            Assert.AreEqual(128, dataFetcher!.GetRemainingRoundTime());
        }

        [Test]
        public void GetRemainingRoundTimeNormalized()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetRemainingRoundTimeNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.GameMinutesTimer.Address, 0x01);
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x05);
            dataFetcher!.NextFrame();
            Assert.AreEqual((double)65.0 / 120.0, dataFetcher!.GetRemainingRoundTimeNormalized());
        }

        [Test]
        public void GetTiles()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.GetTiles());
        }

        [Test]
        public void GetDestructibleTilesRemaining()
        {
            Assert.AreEqual(0, dataFetcher!.GetDestructibleTilesRemaining());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.DestructibleTilesRemaining.Address, 0x09);
            dataFetcher!.NextFrame();
            Assert.AreEqual(9, dataFetcher!.GetDestructibleTilesRemaining());
        }

        [Test]
        public void GetPlayersXPos()
        {
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, dataFetcher!.GetPlayersXPos());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, dataFetcher!.GetPlayersXPos());
        }

        [Test]
        public void GetPlayersYPos()
        {
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, dataFetcher!.GetPlayersYPos());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, dataFetcher!.GetPlayersYPos());
        }

        [Test]
        public void GetPlayerXPositionNormalized()
        {
            int pos = 16;
            Assert.AreEqual(0.0, dataFetcher!.GetPlayerXPositionNormalized(pos));
            pos = 150;
            Assert.AreEqual(134 / (double)(0xD0 - 0x10), dataFetcher!.GetPlayerXPositionNormalized(pos));
        }

        [Test]
        public void GetPlayerYPositionNormalized()
        {
            int pos = 16;
            Assert.AreEqual(0.0, dataFetcher!.GetPlayerYPositionNormalized(pos));
            pos = 120;
            Assert.AreEqual(104 / (double)(0xB0 - 0x10), dataFetcher!.GetPlayerYPositionNormalized(pos));
        }

        [Test]
        public void GetMainPlayerXPositionNormalized()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetMainPlayerXPositionNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, new byte[] { 0x20, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual((0x20 - 0x10) / (double)(0xD0 - 0x10), dataFetcher!.GetMainPlayerXPositionNormalized());
        }

        [Test]
        public void GetMainPlayerYPositionNormalized()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetMainPlayerYPositionNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, new byte[] { 0x20, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual((0x20 - 0x10) / (double)(0xB0 - 0x10), dataFetcher!.GetMainPlayerYPositionNormalized());
        }

        [Test]
        public void GetBombsPos()
        {
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, dataFetcher!.GetBombsPos());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.BombsPositions.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }, dataFetcher!.GetBombsPos());
        }

        [Test]
        public void GetBombsTimer()
        {
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, dataFetcher!.GetBombsTimer());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.BombsTimers.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }, dataFetcher!.GetBombsTimer());
        }

        [Test]
        public void GetBombsTimerNormalized()
        {
            byte timer = 0x00;
            Assert.AreEqual(1.0, dataFetcher!.GetBombsTimerNormalized(timer));
            timer = 0x95;
            Assert.AreEqual(0.0, dataFetcher!.GetBombsTimerNormalized(timer));
            timer = 0x30;
            Assert.AreEqual(1.0 - (0x30 / (double)0x95), dataFetcher!.GetBombsTimerNormalized(timer));
        }

        [Test]
        public void GetMainPlayerBombsPlanted()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerBombsPlanted());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersBombsPlantedCount.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1, dataFetcher!.GetMainPlayerBombsPlanted());
        }

        [Test]
        public void GetMainPlayerBombsPlantedNormalized()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetMainPlayerBombsPlantedNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersBombsPlantedCount.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1 / (double)9.0, dataFetcher!.GetMainPlayerBombsPlantedNormalized());
        }

        [Test]
        public void GetClosestPowerUp()
        {
            byte[] powerups = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10,
                            0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[,] powerups2d = MathUtils.To2DArray(powerups, 11, 13);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00 });
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00 });
            dataFetcher!.NextFrame();
            Assert.AreEqual((0.0, 0x10 / (double)(0xB0 - 0x10)), dataFetcher!.GetClosestPowerUp(powerups2d));
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x00 });
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00 });
            dataFetcher!.NextFrame();
            Assert.AreEqual((0.25, 0.0), dataFetcher!.GetClosestPowerUp(powerups2d));
            byte[] new_powerups = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[,] new_powerups2d = MathUtils.To2DArray(new_powerups, 11, 13);
            Assert.AreEqual((1.0, 1.0), dataFetcher!.GetClosestPowerUp(new_powerups2d));
        }

        [Test]
        public void GetClosestPowerupToMainPlayerXPosNormalized()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.GetClosestPowerupToMainPlayerXPosNormalized());
        }

        [Test]
        public void GetClosestPowerupToMainPlayerYPosNormalized()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.GetClosestPowerupToMainPlayerYPosNormalized());
        }

        [Test]
        public void GetMainPlayerExtraBombPowerUpLevel()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerExtraBombPowerUpLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.ExtraBomb.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1, dataFetcher!.GetMainPlayerExtraBombPowerUpLevel());
        }

        [Test]
        public void GetMainPlayerExplosionExpanderPowerUpLevel()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerExplosionExpanderPowerUpLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.ExplosionExpander.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1, dataFetcher!.GetMainPlayerExplosionExpanderPowerUpLevel());
        }

        [Test]
        public void GetMainPlayerAcceleratorPowerUpLevel()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerAcceleratorPowerUpLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1, dataFetcher!.GetMainPlayerAcceleratorPowerUpLevel());
        }

        [Test]
        public void GetMainPlayerExtraBombPowerUpLevelNormalized()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerExtraBombPowerUpLevelNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.ExtraBomb.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1.0 / (double)9.0, dataFetcher!.GetMainPlayerExtraBombPowerUpLevelNormalized());
        }

        [Test]
        public void GetMainPlayerExplosionExpanderPowerUpLevelNormalized()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerExplosionExpanderPowerUpLevelNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.ExplosionExpander.Address, new byte[] { 0x04, 0x04, 0x04, 0x04, 0x04 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(4.0 / (double)(9.0 - 2.0), dataFetcher!.GetMainPlayerExplosionExpanderPowerUpLevelNormalized());
        }

        [Test]
        public void GetMainPlayerAcceleratorPowerUpLevelNormalized()
        {
            Assert.AreEqual(0, dataFetcher!.GetMainPlayerAcceleratorPowerUpLevelNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1.0 / (double)0x100, dataFetcher!.GetMainPlayerAcceleratorPowerUpLevelNormalized());
        }

        [Test]
        public void IsMainPlayerOnLouie()
        {
            Assert.AreEqual(true, dataFetcher!.IsMainPlayerOnLouie());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.IsOnLouie.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.AreEqual(false, dataFetcher!.IsMainPlayerOnLouie());
        }

        [Test]
        public void IsLouieColourYellow()
        {
            Assert.AreEqual(false, dataFetcher!.IsLouieColourYellow());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x03 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsLouieColourYellow());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x23 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(false, dataFetcher!.IsLouieColourYellow());
        }

        [Test]
        public void IsLouieColourBrown()
        {
            Assert.AreEqual(false, dataFetcher!.IsLouieColourBrown());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0x0D, 0x21, 0xB2, 0x35 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsLouieColourBrown());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x23 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(false, dataFetcher!.IsLouieColourBrown());
        }

        [Test]
        public void IsLouieColourPink()
        {
            Assert.AreEqual(false, dataFetcher!.IsLouieColourPink());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0x5A, 0x69, 0xFF, 0x7D });
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsLouieColourPink());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x23 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(false, dataFetcher!.IsLouieColourPink());
        }

        [Test]
        public void IsLouieColourGreen()
        {
            Assert.AreEqual(false, dataFetcher!.IsLouieColourGreen());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xAC, 0x00, 0xBE, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsLouieColourGreen());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x23 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(false, dataFetcher!.IsLouieColourGreen());
        }

        [Test]
        public void IsLouieColourBlue()
        {
            Assert.AreEqual(false, dataFetcher!.IsLouieColourBlue());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0x40, 0x59, 0x80, 0x7E });
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsLouieColourBlue());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x23 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(false, dataFetcher!.IsLouieColourBlue());
        }

        [Test]
        public void GetLouieColour()
        {
            Assert.AreEqual(0.0, dataFetcher!.GetLouieColour());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xDD, 0x02, 0x5F, 0x03 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(0.2, dataFetcher!.GetLouieColour());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0x0D, 0x21, 0xB2, 0x35 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(0.4, dataFetcher!.GetLouieColour());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0x5A, 0x69, 0xFF, 0x7D });
            dataFetcher!.NextFrame();
            Assert.AreEqual(0.6, dataFetcher!.GetLouieColour());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0xAC, 0x00, 0xBE, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(0.8, dataFetcher!.GetLouieColour());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.MountedLouieColours.Address, new byte[] { 0x40, 0x59, 0x80, 0x7E });
            dataFetcher!.NextFrame();
            Assert.AreEqual(1.0, dataFetcher!.GetLouieColour());
        }

        [Test]
        public void GetMainPlayerKickUpgradeState()
        {
            Assert.AreEqual(false, dataFetcher!.GetMainPlayerKickUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x02);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerKickUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, 0x06);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerKickUpgradeState());
        }

        [Test]
        public void GetMainPlayerGloveUpgradeState()
        {
            Assert.AreEqual(false, dataFetcher!.GetMainPlayerGloveUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x04);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerGloveUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, 0x06);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerGloveUpgradeState());
        }

        [Test]
        public void GetMainPlayerSlimeBombUpgradeState()
        {
            Assert.AreEqual(false, dataFetcher!.GetMainPlayerSlimeBombUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x20);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerSlimeBombUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, 0x26);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerSlimeBombUpgradeState());
        }

        [Test]
        public void GetMainPlayerPowerBombUpgradeState()
        {
            Assert.AreEqual(false, dataFetcher!.GetMainPlayerPowerBombUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x40);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerPowerBombUpgradeState());
            mockEmulatorAdapter!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, 0x46);
            dataFetcher!.NextFrame();
            Assert.AreEqual(true, dataFetcher!.GetMainPlayerPowerBombUpgradeState());
        }

        [Test]
        public void GetPlayersDeathTimer()
        {
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, dataFetcher!.GetPlayersDeathTimer());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 });
            dataFetcher!.NextFrame();
            Assert.AreEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, dataFetcher!.GetPlayersDeathTimer());
        }

        [Test]
        public void CheckPlayerDeathStatus()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x00, 0x3C, 0x00, 0x3C });
            //The function CheckPlayerDeathStatus() gets called in NextFrame()
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsPlayerDead(0));
            Assert.IsTrue(dataFetcher!.IsPlayerDead(2));
            Assert.IsFalse(dataFetcher!.IsPlayerDead(1));
            Assert.IsFalse(dataFetcher!.IsPlayerDead(3));
        }

        [Test]
        public void GetNumberOfPlayersAlive()
        {
            Assert.AreEqual(4, dataFetcher!.GetNumberOfPlayersAlive());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3C, 0x3C, 0x3C, 0x3C, 0x3C });
            dataFetcher!.NextFrame();
            Assert.AreEqual(0, dataFetcher!.GetNumberOfPlayersAlive());

        }

        [Test]
        public void IsMainPlayerDead()
        {
            Assert.IsFalse(dataFetcher!.IsMainPlayerDead());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3C, 0x3C, 0x3C, 0x3C, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsMainPlayerDead());
        }

        [Test]
        public void IsPlayerDead()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x00, 0x3C, 0x00, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsPlayerDead(0));
            Assert.IsTrue(dataFetcher!.IsPlayerDead(2));
            Assert.IsFalse(dataFetcher!.IsPlayerDead(1));
            Assert.IsFalse(dataFetcher!.IsPlayerDead(3));
        }

        [Test]
        public void IsRoundOver()
        {
            Assert.IsFalse(dataFetcher!.IsRoundOver());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x00, 0x3C, 0x00, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsRoundOver());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x3C, 0x3C, 0x00, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsRoundOver());
        }

        [Test]
        public void IsRoundWon()
        {
            Assert.IsFalse(dataFetcher!.IsRoundWon());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x00, 0x3C, 0x00, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsRoundWon());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x3C, 0x3C, 0x3C, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsRoundWon());
        }

        [Test]
        public void IsRoundLost()
        {
            Assert.IsFalse(dataFetcher!.IsRoundLost());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x00 });
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsRoundLost());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3C, 0x3C, 0x00, 0x3C, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsRoundLost());
        }

        [Test]
        public void IsRoundDraw()
        {
            Assert.IsFalse(dataFetcher!.IsRoundDraw());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x00 });
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsRoundDraw());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3C, 0x3C, 0x3C, 0x3C, 0x3C });
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsRoundDraw());
        }

        [Test]
        public void BombToGridPos()
        {
            Assert.AreEqual((0,0), dataFetcher!.BombToGridPos(17));
            Assert.AreEqual((7, 11), dataFetcher!.BombToGridPos(140));
        }

        [Test]
        public void MainPlayerToGridPos()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, 0x10);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, 0x10);
            dataFetcher!.NextFrame();
            Assert.AreEqual((0, 0), dataFetcher!.MainPlayerToGridPos());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, 0x50);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, 0x30);
            dataFetcher!.NextFrame();
            Assert.AreEqual((2, 4), dataFetcher!.MainPlayerToGridPos());
        }

        [Test]
        public void GetEnemiesXDistanceToThePlayer()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, new byte[] { 0x50, 0x40, 0x10, 0x70, 0x00 });
            dataFetcher!.NextFrame();
            double[] tab = new double[] { (0x40 - 0x50) / (double)(0xD0 - 0x10), (0x10 - 0x50) / (double)(0xD0 - 0x10), (0x70 - 0x50) / (double)(0xD0 - 0x10) };
            var tab2 = MathUtils.To2DArray(tab, 3, 1);
            Assert.AreEqual(tab2[0, 0], dataFetcher!.GetEnemiesXDistanceToThePlayer()[0, 0], 0.0000001);
            Assert.AreEqual(tab2[1, 0], dataFetcher!.GetEnemiesXDistanceToThePlayer()[0, 1], 0.0000001);
            Assert.AreEqual(tab2[2, 0], dataFetcher!.GetEnemiesXDistanceToThePlayer()[0, 2], 0.0000001);
        }

        [Test]
        public void GetEnemiesYDistanceToThePlayer()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, new byte[] { 0x50, 0x40, 0x10, 0x70, 0x00 });
            dataFetcher!.NextFrame();
            double[] tab = new double[] { (0x40 - 0x50) / (double)(0xB0 - 0x10), (0x10 - 0x50) / (double)(0xB0 - 0x10), (0x70 - 0x50) / (double)(0xB0 - 0x10) };
            var tab2 = MathUtils.To2DArray(tab, 3, 1);
            Assert.AreEqual(tab2[0, 0], dataFetcher!.GetEnemiesYDistanceToThePlayer()[0, 0], 0.0000001);
            Assert.AreEqual(tab2[1, 0], dataFetcher!.GetEnemiesYDistanceToThePlayer()[0, 1], 0.0000001);
            Assert.AreEqual(tab2[2, 0], dataFetcher!.GetEnemiesYDistanceToThePlayer()[0, 2], 0.0000001);
        }

        [Test]
        public void MapPlayableTiles()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.MapPlayableTiles());
        }

        [Test]
        public void DrawTiles()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.DrawTiles());
        }

        [Test]
        public void DrawDangers()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.DrawDangers());
        }

        [Test]
        public void TrackBombPlanted()
        {
            //Not testing function using df's internal tile cache
            Assert.DoesNotThrow(() => dataFetcher!.TrackBombPlanted());
        }

        [Test]
        public void TrackBombExpired()
        {
            Assert.DoesNotThrow(() => dataFetcher!.TrackBombExpired());
        }

        [Test]
        public void TrackBombExploded()
        {
            Assert.DoesNotThrow(() => dataFetcher!.TrackBombExploded());
        }

        [Test]
        public void IsBombAlreadyTracked()
        {
            Assert.DoesNotThrow(() => dataFetcher!.IsBombAlreadyTracked(0, 0));
        }

        [Test]
        public void GetBombIndex()
        {
            Assert.DoesNotThrow(() => dataFetcher!.TrackBombExploded());
        }

        [Test]
        public void FreeBombIndex()
        {
            Assert.DoesNotThrow(() => dataFetcher!.FreeBombIndex(0));
        }
    }
}
