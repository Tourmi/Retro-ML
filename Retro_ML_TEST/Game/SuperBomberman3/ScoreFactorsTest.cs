using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperBomberman3.Configuration;
using Retro_ML.SuperBomberman3.Game;
using Retro_ML.SuperBomberMan3.Neural.Scoring;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.SuperBomberman3
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private SB3DataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new SB3DataFetcher(emu, new NeuralConfig(), new SB3PluginConfig());
        }

        [Test]
        public void EndRoundScoreFactor()
        {
            EndRoundScoreFactor sf = new() { ScoreMultiplier = 100 };
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x00, 0x00, 0x00, 0x00 });
            df!.NextFrame();
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            //Player Died before 1v1 situation
            Assert.AreEqual(-10.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x3B, 0x3B, 0x00, 0x00 });
            df!.NextFrame();
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            //Player Died in 1v1 situation
            Assert.AreEqual(-20.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x3B, 0x3B, 0x3B, 0x3B });
            df!.NextFrame();
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            //Player Draw
            Assert.AreEqual(-10.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x3B, 0x3B, 0x3B, 0x00 });
            df!.NextFrame();
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            //Player Died in 1v1 situation
            Assert.AreEqual(90.0, sf.GetFinalScore());
        }

        [Test]
        public void IdleScoreFactor()
        {
            IdleScoreFactor sf = new() { ScoreMultiplier = -5 };
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, 0x10);
            sf.Update(df!);
            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df!);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-5, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, 0x10);
            var curPos = 0x10;
            sf.Update(df);
            for (int i = 0; i < 170; i++)
            {
                curPos = curPos++;
                emu!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, (byte)curPos);
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Should not stop while the player is moving");
            }

            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-10, sf.GetFinalScore());
        }

        [Test]
        public void TimeTakenScoreFactor()
        {
            TimeTakenScoreFactor sf = new() { ScoreMultiplier = 0.25 };
            emu!.SetMemory(Addresses.GameAddresses.GameMinutesTimer.Address, 0x00);
            emu!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x64);
            df!.NextFrame();
            sf.Update(df!);
            emu!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x14);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x3B, 0x3B, 0x3B, 0x3B });
            df!.NextFrame();
            sf.Update(df!);
            sf.LevelDone();
            //Player won
            Assert.AreEqual(30.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.GameAddresses.GameMinutesTimer.Address, 0x00);
            emu!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x64);
            df!.NextFrame();
            sf.Update(df!);
            emu!.SetMemory(Addresses.GameAddresses.GameSecondsTimer.Address, 0x14);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x00, 0x3B, 0x3B, 0x00 });
            df!.NextFrame();
            sf.Update(df!);
            sf.LevelDone();
            //Player lost
            Assert.AreEqual(50.0, sf.GetFinalScore());
        }

        [Test]
        public void PowerupScoreFactor()
        {
            PowerupScoreFactor sf = new() { ScoreMultiplier = 1.0 };
            emu!.SetMemory(Addresses.PowerupsAddresses.IsOnLouie.Address, 0x01);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x3B, 0x00, 0x3B, 0x3B, 0x00 });
            df!.NextFrame();
            sf.Update(df!);
            emu!.SetMemory(Addresses.PowerupsAddresses.ExtraBomb.Address, 0x01);
            df!.NextFrame();
            sf.Update(df!);
            //Increase in extra bomb
            Assert.AreEqual(3.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.ExplosionExpander.Address, 0x01);
            df!.NextFrame();
            sf.Update(df!);
            //Increase in explosion expander
            Assert.AreEqual(6.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.Accelerator.Address, 0x20);
            df!.NextFrame();
            sf.Update(df!);
            //Increase in accelerator
            Assert.AreEqual(9.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.IsOnLouie.Address, 0x00);
            df!.NextFrame();
            sf.Update(df!);
            //Louie
            Assert.AreEqual(18.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x02);
            df!.NextFrame();
            sf.Update(df!);
            //Kick
            Assert.AreEqual(24.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x06);
            df!.NextFrame();
            sf.Update(df!);
            //Gloves
            Assert.AreEqual(30.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x26);
            df!.NextFrame();
            sf.Update(df!);
            //Slime bomb
            Assert.AreEqual(36.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PowerupsAddresses.BombermanUpgrade.Address, 0x46);
            df!.NextFrame();
            sf.Update(df!);
            //Power bomb
            Assert.AreEqual(42.0, sf.GetFinalScore());
        }

        [Test]
        public void BombScoreFactor()
        {
            BombScoreFactor sf = new() { ScoreMultiplier = 1.0 };
            emu!.SetMemory(Addresses.GameAddresses.DynamicTiles.Address, new byte[] { 0x50, 0x50 });
            emu!.SetMemory(Addresses.GameAddresses.BombsPositions.Address, 0x11);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, 0x10);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, 0x10);
            emu!.SetMemory(Addresses.GameAddresses.DestructibleTilesRemaining.Address, 0x04);
            df!.NextFrame();
            for (uint i = 0; i < 0x95; i++)
            {
                if (i == 0x94)
                {
                    emu!.SetMemory(Addresses.GameAddresses.DestructibleTilesRemaining.Address, 0x02);
                }
                df!.NextFrame();
                sf.Update(df!);
            }
            sf.LevelDone();
            //Destroyed 2 Tiles
            Assert.AreEqual(6.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.GameAddresses.DynamicTiles.Address, new byte[] { 0x50, 0x50 });
            emu!.SetMemory(Addresses.GameAddresses.BombsPositions.Address, 0x11);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersXPos.Address, 0x10);
            emu!.SetMemory(Addresses.PlayersAddresses.PlayersYPos.Address, 0x10);
            df!.NextFrame();
            for (uint i = 0; i < 0x95 + 0x05; i++)
            {
                if (i == 0x94 + 0x05)
                {
                    emu!.SetMemory(Addresses.PlayersAddresses.PlayersDeathTimer.Address, new byte[] { 0x00, 0x00, 0x3B, 0x00, 0x00 });
                }
                df!.NextFrame();
                sf.Update(df!);
            }
            sf.LevelDone();
            //Killed a player
            Assert.AreEqual(31.0, sf.GetFinalScore());
        }
    }
}
