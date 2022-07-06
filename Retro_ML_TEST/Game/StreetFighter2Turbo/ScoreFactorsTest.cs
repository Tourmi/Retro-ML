using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.StreetFighter2Turbo.Configuration;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.StreetFighter2Turbo.Neural.Scoring;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.StreetFighter2Turbo
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private SF2TDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new SF2TDataFetcher(emu, new NeuralConfig(), new SF2TPluginConfig());
        }

        [Test]
        public void StopFightingScoreFactor()
        {
            StopFightingScoreFactor sf = new() { ScoreMultiplier = -50 };
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x05);
            sf.Update(df!);
            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df!);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-50, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0xB0);
            var curHP = 0xB0;
            sf.Update(df);
            for (int i = 0; i < 170; i++)
            {
                curHP = curHP--;
                emu!.SetMemory(Addresses.Player2Addresses.HP.Address, (byte)curHP);
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Should not stop while the player is attacking the enemy");
            }

            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-100, sf.GetFinalScore());
        }

        [Test]
        public void EndRoundScoreFactor()
        {
            EndRoundScoreFactor sf = new() { ScoreMultiplier = 100 };
            emu!.SetMemory(Addresses.Player1Addresses.HP.Address, 0x01);
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x05);
            emu!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            Assert.AreEqual(-10.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.Player1Addresses.HP.Address, 0x05);
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x01);
            emu!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            Assert.AreEqual(90.0, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.Player1Addresses.HP.Address, 0x01);
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x01);
            emu!.SetMemory(Addresses.Player2Addresses.EndRoundStatus.Address, 0x01);
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            Assert.AreEqual(89.0, sf.GetFinalScore());
        }

        [Test]
        public void CombatScoreFactor()
        {
            CombatScoreFactor sf = new() { ScoreMultiplier = 100 };
            emu!.SetMemory(Addresses.Player1Addresses.HP.Address, 0x10);
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x05);
            emu!.SetMemory(Addresses.GameAddresses.RoundTimer.Address, 0x09);
            sf.Update(df!);
            sf.LevelDone();
            double firstScore = (df!.GetRoundTimerNormalized() * 0.50 + (1.0 - 0.5)) * 100 * (df!.GetPlayer1HpNormalized() - df!.GetPlayer2HpNormalized());
            Assert.AreEqual(firstScore, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.Player1Addresses.HP.Address, 0x02);
            emu!.SetMemory(Addresses.Player2Addresses.HP.Address, 0x0D);
            emu!.SetMemory(Addresses.GameAddresses.RoundTimer.Address, 0xA2);
            sf.Update(df!);
            sf.LevelDone();
            double secondScore = firstScore + (df!.GetRoundTimerNormalized() * 0.50 + (1.0 - 0.5)) * 100 * (df!.GetPlayer1HpNormalized() - df!.GetPlayer2HpNormalized());
            Assert.AreEqual(secondScore, sf.GetFinalScore());
        }
    }


}
