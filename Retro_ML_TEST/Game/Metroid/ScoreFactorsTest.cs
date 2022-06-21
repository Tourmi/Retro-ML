using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Metroid.Game;
using Retro_ML.Metroid.Neural.Scoring;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.Metroid
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private MetroidDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new MetroidDataFetcher(emu, new NeuralConfig(), new MetroidPluginConfig());
        }

        [Test]
        public void DiedScoreFactor()
        {
            var sf = new DiedScoreFactor() { ScoreMultiplier = -50 }.Clone();
            Assert.IsAssignableFrom<DiedScoreFactor>(sf);

            emu!.SetMemory(Addresses.Samus.Health.Address, 0x0034);
            df!.NextFrame();
            sf.Update(df);

            Assert.IsFalse(sf.ShouldStop);
            Assert.AreEqual(0, sf.GetFinalScore(), 0.00001);

            emu.SetMemory(Addresses.Samus.Health.Address, 0);
            df.NextFrame();
            sf.Update(df);

            Assert.IsTrue(sf.ShouldStop);
            Assert.AreEqual(-50, sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void HealthScoreFactor()
        {
            var sf = new HealthScoreFactor() { ScoreMultiplier = 10, GainedHealthMultiplier = 2, LostHealthMultiplier = -3 }.Clone();
            Assert.IsAssignableFrom<HealthScoreFactor>(sf);
            emu!.SetMemory(Addresses.Samus.Health.Address, 0x50, 0x00);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(0, sf.GetFinalScore(), 0.00001);

            emu!.SetMemory(Addresses.Samus.Health.Address, 0x58, 0x00);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(8 * 10 * 2, sf.GetFinalScore(), 0.00001);

            sf.Update(df);
            Assert.AreEqual(8 * 10 * 2, sf.GetFinalScore(), 0.00001);

            emu!.SetMemory(Addresses.Samus.Health.Address, 0x54, 0x00);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(8 * 10 * 2 + 4 * 10 * -3, sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void ObjectiveScoreFactor()
        {
            var sf = new ObjectiveScoreFactor() { ScoreMultiplier = 15, BossMultiplier = 30, ItemMultiplier = 1.5, DamagedBossMultiplier = 0.01, StopOnObjectiveReached = true }.Clone();
            Assert.IsAssignableFrom<ObjectiveScoreFactor>(sf);

            sf.Update(df!);
            Assert.IsFalse(sf.ShouldStop);
            Assert.AreEqual(0, sf.GetFinalScore());

            emu!.SetMemory(Addresses.Gamestate.FreezeTimer.Address, 0x10);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(15 * 1.5, sf.GetFinalScore(), 0.00001);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();

            ((ObjectiveScoreFactor)sf).StopOnObjectiveReached = false;
            emu.SetMemory(Addresses.Gamestate.FreezeTimer.Address, 0);
            df.NextFrame();
            sf.Update(df);
            Assert.IsFalse(sf.ShouldStop);

            emu.SetMemory(Addresses.Gamestate.IsMiniBossPresent.Address, 1);
            emu.SetMemory(Addresses.Sprites.Status.Address, 1);
            emu.SetMemory(Addresses.Sprites.Hitpoints.Address, 50);
            df.NextFrame();
            sf.Update(df);
            Assert.IsFalse(sf.ShouldStop);

            emu.SetMemory(Addresses.Sprites.Hitpoints.Address, 25);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(15 * (1.5 + 25 * 0.01), sf.GetFinalScore(), 0.00001);

            emu.SetMemory(Addresses.Gamestate.FreezeTimer.Address, 0x10);
            emu.SetMemory(Addresses.Gamestate.IsMiniBossPresent.Address, 0x00);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(15 * (1.5 + 25 * 0.01 + 30), sf.GetFinalScore(), 0.00001);
            Assert.IsFalse(sf.ShouldStop);

            sf.Update(df);
            Assert.AreEqual(15 * (1.5 + 25 * 0.01 + 30), sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void ProgressScoreFactor() => Assert.Ignore("Progress Score Factor test not implemented");

        [Test]
        public void TimeTakenScoreFactor()
        {
            var sf = new TimeTakenScoreFactor() { ScoreMultiplier = -9, MaximumTrainingTime = 101 }.Clone();
            Assert.IsAssignableFrom<TimeTakenScoreFactor>(sf);

            for (int i = 0; i < 60.0 * 101 - 1; i++)
            {
                df!.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop);
            }

            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-9 * 101, sf.GetFinalScore(), 0.00001);

            for (int i = 0; i < 60.0 * 101 - 1; i++)
            {
                df!.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop);
            }

            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-9 * 101 * 2, sf.GetFinalScore(), 0.00001);
        }
    }
}
