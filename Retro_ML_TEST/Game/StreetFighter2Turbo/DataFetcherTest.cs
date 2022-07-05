using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.StreetFighter2Turbo.Configuration;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.StreetFighter2Turbo
{
    [TestFixture]
    internal class DataFetcherTest
    {
        delegate bool TestFunction();

        private SF2TDataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new SF2TDataFetcher(mockEmulatorAdapter, new NeuralConfig(), new SF2TPluginConfig());
            dataFetcher.NextFrame();
        }

        [Test]
        public void FrameCache()
        {
            Assert.False(dataFetcher!.IsPlayer1Attacking());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.State.Address, 0x0A);
            Assert.False(dataFetcher!.IsPlayer1Attacking(), "The cache should not have updated");
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer1Attacking(), "The cache should have updated");
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
        public void IsPlayer1InEndRound()
        {
            Assert.IsFalse(dataFetcher!.IsPlayer1InEndRound());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsPlayer1InEndRound());
        }

        [Test]
        public void IsPlayer2InEndRound()
        {
            Assert.IsFalse(dataFetcher!.IsPlayer2InEndRound());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsPlayer2InEndRound());
        }

        [Test]
        public void IsRoundOver()
        {
            Assert.IsFalse(dataFetcher!.IsRoundOver());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsRoundOver());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.EndRoundStatus.Address, 0x00);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsRoundOver());
        }

        [Test]
        public void GetPlayer1Hp()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer1Hp());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xB0);
            dataFetcher!.NextFrame();
            Assert.Equals(0xB0, dataFetcher!.GetPlayer1Hp());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xFF);
            dataFetcher!.NextFrame();
            Assert.Equals(0x00, dataFetcher!.GetPlayer1Hp());
        }

        [Test]
        public void GetPlayer2Hp()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer2Hp());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xB0);
            dataFetcher!.NextFrame();
            Assert.Equals(0xB0, dataFetcher!.GetPlayer2Hp());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xFF);
            dataFetcher!.NextFrame();
            Assert.Equals(0x00, dataFetcher!.GetPlayer2Hp());
        }

        [Test]
        public void GetPlayer1HpNormalized()
        {
            Assert.Equals(0.0, dataFetcher!.GetPlayer1HpNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xB0);
            dataFetcher!.NextFrame();
            Assert.Equals(1.0, dataFetcher!.GetPlayer1HpNormalized());
        }

        [Test]
        public void GetPlayer2HpNormalized()
        {
            Assert.Equals(0.0, dataFetcher!.GetPlayer2HpNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xB0);
            dataFetcher!.NextFrame();
            Assert.Equals(1.0, dataFetcher!.GetPlayer2HpNormalized());
        }

        [Test]
        public void HasPlayerWon()
        {
            Assert.False(dataFetcher!.HasPlayerWon());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xB0);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xFF);
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.HasPlayerWon());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0x01);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x02);
            Assert.False(dataFetcher!.HasPlayerWon());
        }

        [Test]
        public void HasPlayerLost()
        {
            Assert.False(dataFetcher!.HasPlayerLost());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xFF);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xB0);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.HasPlayerLost());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xB0);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xFF);
            Assert.False(dataFetcher!.HasPlayerLost());
        }

        [Test]
        public void IsRoundDraw()
        {
            Assert.False(dataFetcher!.IsRoundDraw());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xFF);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xB0);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.False(dataFetcher!.IsRoundDraw());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.HP.Address, 0xB0);
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xB0);
            Assert.True(dataFetcher!.IsRoundDraw());
        }

        [Test]
        public void GetRoundTimer()
        {
            Assert.Equals(0, dataFetcher!.GetRoundTimer());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.RoundTimer.Address, 0x05);
            dataFetcher!.NextFrame();
            Assert.Equals(5, dataFetcher!.GetRoundTimer());
        }

        [Test]
        public void GetRoundTimerNormalized()
        {
            Assert.Equals(0, dataFetcher!.GetRoundTimerNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.RoundTimer.Address, 0x05);
            dataFetcher!.NextFrame();
            Assert.Equals((double) 5.0/99.0, dataFetcher!.GetRoundTimerNormalized());
        }

        [Test]
        public void GetPlayer1XPos()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer1XPos());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.XPos.Address, new byte[] { 0x32, 0xC8});
            dataFetcher!.NextFrame();
            Assert.Equals(13000, dataFetcher!.GetPlayer1XPos());
        }

        [Test]
        public void GetPlayer2XPos()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer2XPos());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.XPos.Address, new byte[] { 0x32, 0xC8 });
            dataFetcher!.NextFrame();
            Assert.Equals(13000, dataFetcher!.GetPlayer2XPos());
        }

        [Test]
        public void GetPlayer1XPosNormalized()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer1XPosNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.XPos.Address, new byte[] { 0x32, 0xC8 });
            dataFetcher!.NextFrame();
            Assert.Equals((13000 - 0x3700) / (double) (0x1CA00 - 0x3700) * 2 - 1, dataFetcher!.GetPlayer1XPosNormalized());
        }

        [Test]
        public void GetPlayer2XPosNormalized()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer2XPosNormalized());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.XPos.Address, new byte[] { 0x32, 0xC8 });
            dataFetcher!.NextFrame();
            Assert.Equals((13000 - 0x3700) / (double)(0x1CA00 - 0x3700) * 2 - 1, dataFetcher!.GetPlayer2XPosNormalized());
        }

        [Test]
        public void GetHorizontalDistanceBetweenPlayers()
        {
            Assert.Equals(0, dataFetcher!.GetHorizontalDistanceBetweenPlayers());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.XPos.Address, new byte[] { 0x32, 0xC8 });
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.XPos.Address, new byte[] { 0x36, 0xB0 });
            dataFetcher!.NextFrame();
            Assert.Equals(1000 / 0xD200, dataFetcher!.GetHorizontalDistanceBetweenPlayers());
        }

        [Test]
        public void GetPlayer1YPos()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer1YPos());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.YPos.Address, new byte[] { 0x32, 0xC8 });
            dataFetcher!.NextFrame();
            Assert.Equals(13000, dataFetcher!.GetPlayer1YPos());
        }

        [Test]
        public void GetPlayer2YPos()
        {
            Assert.Equals(0, dataFetcher!.GetPlayer2YPos());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.XPos.Address, new byte[] { 0x32, 0xC8 });
            dataFetcher!.NextFrame();
            Assert.Equals(13000, dataFetcher!.GetPlayer2YPos());
        }

        [Test]
        public void GetVerticalDistanceBetweenPlayers()
        {
            Assert.Equals(0, dataFetcher!.GetVerticalDistanceBetweenPlayers());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.YPos.Address, new byte[] { 0x32, 0xC8 });
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.YPos.Address, new byte[] { 0x36, 0xB0 });
            dataFetcher!.NextFrame();
            Assert.Equals(1000 / 0x4200, dataFetcher!.GetVerticalDistanceBetweenPlayers());
        }

        [Test]
        public void IsPlayer1Crouched()
        {
            Assert.False(dataFetcher!.IsPlayer1Crouched());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.State.Address, 0x02);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer1Crouched());
        }

        [Test]
        public void IsPlayer2Crouched()
        {
            Assert.False(dataFetcher!.IsPlayer2Crouched());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.State.Address, 0x02);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer2Crouched());
        }

        [Test]
        public void IsPlayer1Jumping()
        {
            Assert.False(dataFetcher!.IsPlayer1Jumping());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.State.Address, 0x04);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer1Jumping());
        }

        [Test]
        public void IsPlayer2Jumping()
        {
            Assert.False(dataFetcher!.IsPlayer2Jumping());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.State.Address, 0x04);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer2Jumping());
        }

        [Test]
        public void IsPlayer1Blocking()
        {
            Assert.False(dataFetcher!.IsPlayer1Blocking());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.State.Address, 0x03);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer1Blocking());
        }

        [Test]
        public void IsPlayer2Blocking()
        {
            Assert.False(dataFetcher!.IsPlayer2Blocking());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.State.Address, 0x03);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer2Blocking());
        }

        [Test]
        public void IsPlayer1Staggered()
        {
            Assert.False(dataFetcher!.IsPlayer1Staggered());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.State.Address, 0x14);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer1Staggered());
            mockEmulatorAdapter!.SetMemory(Addresses.Player1Addresses.State.Address, 0x0E);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer1Staggered());
        }

        [Test]
        public void IsPlayer2Staggered()
        {
            Assert.False(dataFetcher!.IsPlayer2Staggered());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.State.Address, 0x14);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer2Staggered());
            mockEmulatorAdapter!.SetMemory(Addresses.Player2Addresses.State.Address, 0x0E);
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.IsPlayer2Staggered());
        }
    }
}
