using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Configuration;
using Retro_ML.SuperMarioKart.Game;
using Retro_ML.SuperMarioKart.Neural.Scoring;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.SuperMarioKart
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private SMKDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new SMKDataFetcher(emu, new NeuralConfig(), new SMKPluginConfig());
        }

        [Test]
        public void CheckpointReachedScoreFactorTest()
        {
            var sf = new CheckpointReachedScoreFactor() { ScoreMultiplier = 12 }.Clone();
            Assert.IsAssignableFrom<CheckpointReachedScoreFactor>(sf);
            emu!.SetMemory(Addresses.Race.CheckpointCount.Address, 20);
            emu!.SetMemory(Addresses.Racers.CurrentCheckpointNumber.Address, 10);
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128 + 2);
            df!.NextState();
            sf.Update(df);
            Assert.AreEqual(0, sf.GetFinalScore(), 0.00001, "Score should have remained at zero despite starting mid-way through the race.");

            emu!.SetMemory(Addresses.Racers.CurrentCheckpointNumber.Address, 5);
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128 + 3);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12.0 * 15, sf.GetFinalScore(), 0.00001);

            sf.LevelDone();
            emu!.SetMemory(Addresses.Race.CheckpointCount.Address, 30);
            emu!.SetMemory(Addresses.Racers.CurrentCheckpointNumber.Address, 0);
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128);
            df.NextState();
            sf.Update(df);
            Assert.AreEqual(12.0 * 15, sf.GetFinalScore(), 0.00001, "Score should not have changed on save state change");

            emu!.SetMemory(Addresses.Racers.CurrentCheckpointNumber.Address, 15);
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128 + 2);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12.0 * (15 + 75), sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void CoinsScoreFactorTest()
        {
            var sf = new CoinsScoreFactor() { ScoreMultiplier = 0.5, ExtraFields = new ExtraField[] { new ExtraField(CoinsScoreFactor.LOSING_COINS_MULT, -0.3) } }.Clone();
            Assert.IsAssignableFrom<CoinsScoreFactor>(sf);
            emu!.SetMemory(Addresses.Race.Coins.Address, 20);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(0, sf.GetFinalScore(), 0.00001, "Score should have remained at zero despite starting with coins.");

            emu!.SetMemory(Addresses.Race.Coins.Address, 25);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(0.5 * 5, sf.GetFinalScore(), 0.00001);
            emu!.SetMemory(Addresses.Race.Coins.Address, 23);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(0.5 * (5 - 0.3 * 2), sf.GetFinalScore(), 0.00001);

            sf.LevelDone();
            emu!.SetMemory(Addresses.Race.Coins.Address, 0);
            df.NextState();
            sf.Update(df);
            Assert.AreEqual(0.5 * (5 - 0.3 * 2), sf.GetFinalScore(), 0.00001, "Score should not have changed between save states");

            emu!.SetMemory(Addresses.Race.Coins.Address, 10);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(0.5 * ((5 + 10 - 0.3 * 2)), sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void CollisionScoreFactorTest()
        {
            var sf = new CollisionScoreFactor() { ScoreMultiplier = -0.2, ExtraFields = new ExtraField[] { new ExtraField(CollisionScoreFactor.STOP_AFTER_X_COLLISIONS, 10) } }.Clone();
            Assert.IsAssignableFrom<CollisionScoreFactor>(sf);
            df!.NextFrame();
            sf.Update(df);

            for (int i = 0; i < 10; i++)
            {
                Assert.IsFalse(sf.ShouldStop);
                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x03);
                df!.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x01);
                df!.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x00);
                df!.NextFrame();
                sf.Update(df);
            }

            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-0.2 * 10, sf.GetFinalScore(), 0.00001);

            sf.ExtraFields = new ExtraField[] { new ExtraField(CollisionScoreFactor.STOP_AFTER_X_COLLISIONS, 15) };
            df!.NextFrame();
            sf.Update(df);

            for (int i = 0; i < 15; i++)
            {
                Assert.IsFalse(sf.ShouldStop);
                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x03);
                df!.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x01);
                df!.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x00);
                df!.NextFrame();
                sf.Update(df);
            }

            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-0.2 * (10 + 15), sf.GetFinalScore(), 0.00001);

            sf.ExtraFields = new ExtraField[] { new ExtraField(CollisionScoreFactor.STOP_AFTER_X_COLLISIONS, 0) };
            df!.NextFrame();
            sf.Update(df);

            for (int i = 0; i < 20; i++)
            {
                Assert.IsFalse(sf.ShouldStop);
                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x03);
                df!.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x01);
                df!.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.CollisionTimer.Address, 0x00);
                df!.NextFrame();
                sf.Update(df);
            }

            Assert.IsFalse(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-0.2 * (10 + 15 + 20), sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void FinishedRaceScoreFactorTest()
        {
            var sf = new FinishedRaceScoreFactor() { ScoreMultiplier = 105, ExtraFields = new ExtraField[] { new ExtraField(FinishedRaceScoreFactor.FINAL_RANKING, 0.4) } }.Clone();
            Assert.IsAssignableFrom<FinishedRaceScoreFactor>(sf);
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128 + 4);
            df!.NextFrame();
            sf.Update(df);
            Assert.IsFalse(sf.ShouldStop);

            emu!.SetMemory(Addresses.Racers.CurrentRank.Address, 0x4); //3rd place
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128 + 5);
            df.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(105 * (1 + ((8 - 2) * 0.4)), sf.GetFinalScore(), 0.00001);


            emu!.SetMemory(Addresses.Race.Type.Address, 0x4); // Time Trial
            df.NextState();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(105 * (2 + ((8 - 2) * 0.4)), sf.GetFinalScore(), 0.00001, "Should not have applied ranking multiplier since we're in a time trial");
        }

        [Test]
        public void LakituScoreFactorTest()
        {
            var sf = new LakituScoreFactor() { ScoreMultiplier = -11, ExtraFields = new ExtraField[] { new ExtraField(LakituScoreFactor.STOP_AFTER_X_FALLS, 10) } }.Clone();
            Assert.IsAssignableFrom<LakituScoreFactor>(sf);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(0, sf.GetFinalScore(), 0.00001);

            for (int i = 0; i < 10; i++)
            {
                Assert.IsFalse(sf.ShouldStop);
                emu!.SetMemory(Addresses.Racers.KartStatus.Address, 0x06);
                df.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.KartStatus.Address, 0x00);
                df.NextFrame();
                sf.Update(df);
            }

            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-11 * 10, sf.GetFinalScore(), 0.00001);

            sf.ExtraFields = new ExtraField[] { new ExtraField(LakituScoreFactor.STOP_AFTER_X_FALLS, 0) };

            df.NextFrame();
            sf.Update(df);

            for (int i = 0; i < 20; i++)
            {
                Assert.IsFalse(sf.ShouldStop);
                emu!.SetMemory(Addresses.Racers.KartStatus.Address, 0x06);
                df.NextFrame();
                sf.Update(df);

                emu!.SetMemory(Addresses.Racers.KartStatus.Address, 0x00);
                df.NextFrame();
                sf.Update(df);
            }

            Assert.IsFalse(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-11 * (10 + 20), sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void OffRoadScoreFactorTest()
        {
            var sf = new OffRoadScoreFactor() { ScoreMultiplier = -7, ExtraFields = new ExtraField[] { new ExtraField(OffRoadScoreFactor.STOP_AFTER, 1) } }.Clone();
            Assert.IsAssignableFrom<OffRoadScoreFactor>(sf);
            df!.NextFrame();
            sf.Update(df);

            emu!.SetMemory(Addresses.Racers.OnRoad.Address, 0x10);
            for (int i = 0; i < 59; i++)
            {
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop);
            }

            Assert.AreEqual(-7.0 * 59.0 / 60.0, sf.GetFinalScore(), 0.00001);
            emu!.SetMemory(Addresses.Racers.OnRoad.Address, 0x20);
            df.NextFrame();
            sf.Update(df);
            emu!.SetMemory(Addresses.Racers.OnRoad.Address, 0x10);

            for (int i = 0; i < 59; i++)
            {
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop);
            }

            df.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-7.0 * (119.0 / 60.0), sf.GetFinalScore(), 0.00001);

        }

        [Test]
        public void StoppedProgressingScoreFactorTest()
        {
            var sf = new StoppedProgressingScoreFactor() { ScoreMultiplier = -14, ExtraFields = new ExtraField[] { new ExtraField(StoppedProgressingScoreFactor.MAX_TIME_WITHOUT_PROGRESS, 2) } }.Clone();
            Assert.IsAssignableFrom<StoppedProgressingScoreFactor>(sf);
            emu!.SetMemory(Addresses.Race.CheckpointCount.Address, 20);
            emu!.SetMemory(Addresses.Racers.CurrentCheckpointNumber.Address, 0);
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128);
            emu!.SetMemory(Addresses.Race.RaceStatus.Address, 0x04);
            df!.NextFrame();
            sf.Update(df);

            for (int i = 0; i < 180; i++)
            {
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Since the race hasn't started yet, we shouldn't have stopped training");
            }
            Assert.AreEqual(0, sf.GetFinalScore(), 0.00001);
            emu!.SetMemory(Addresses.Race.RaceStatus.Address, 0x06);
            emu!.SetMemory(Addresses.Racers.CurrentCheckpointNumber.Address, 1);

            for (int i = 0; i < 119; i++)
            {
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Shouldn't have stopped training yet");
            }
            emu!.SetMemory(Addresses.Racers.CurrentLap.Address, 128 + 1);
            df.NextFrame();
            sf.Update(df);

            for (int i = 0; i < 119; i++)
            {
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Shouldn't have stopped training yet");
            }
            df.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            Assert.AreEqual(-14.0, sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void TimeTakenScoreFactorTest()
        {
            var sf = new TimeTakenScoreFactor() { ScoreMultiplier = -9, ExtraFields = new ExtraField[] { new ExtraField(TimeTakenScoreFactor.MAXIMUM_RACE_TIME, 101) } }.Clone();
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
