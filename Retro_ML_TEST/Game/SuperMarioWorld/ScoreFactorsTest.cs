using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioWorld.Configuration;
using Retro_ML.SuperMarioWorld.Game;
using Retro_ML.SuperMarioWorld.Neural.Scoring;
using Retro_ML_TEST.Mocks;

namespace Retro_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private SMWDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new SMWDataFetcher(emu, new NeuralConfig(), new SMWPluginConfig());
        }

        [Test]
        public void Score()
        {
            Score score = new(new IScoreFactor[] { new DiedScoreFactor() { ScoreMultiplier = -20 }, new WonLevelScoreFactor() { ScoreMultiplier = 30 } });
            score.Update(df!);
            Assert.IsFalse(score.ShouldStop);
            Assert.AreEqual(1, score.GetFinalScore());
            emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x09);
            emu!.SetMemory(Addresses.Level.KeyholeTimer.Address, 0x30);
            df!.NextFrame();
            score.Update(df);
            Assert.IsTrue(score.ShouldStop);
            score.LevelDone();
            Assert.AreEqual(11, score.GetFinalScore());

            emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x00);
            emu!.SetMemory(Addresses.Level.KeyholeTimer.Address, 0);
            df.NextState();
            score.Update(df);
            Assert.IsFalse(score.ShouldStop);
            emu!.SetMemory(Addresses.Level.EndLevelTimer.Address, 0xFF);
            df.NextFrame();
            score.Update(df);
            Assert.IsTrue(score.ShouldStop);
            score.LevelDone();
            Assert.AreEqual(41, score.GetFinalScore());

            score = new(new IScoreFactor[] { new DiedScoreFactor() { ScoreMultiplier = -0.5 } });
            emu.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x09);
            df.NextFrame();
            score.Update(df);
            score.LevelDone();
            Assert.AreEqual(1 / 1.5, score.GetFinalScore());
            score.Update(df);
            score.LevelDone();
            Assert.AreEqual(1 / 2.0, score.GetFinalScore());

        }

        [Test]
        public void CoinsScoreFactor()
        {
            CoinsScoreFactor sf = new() { ScoreMultiplier = 5 };
            emu!.SetMemory(Addresses.Counters.Coins.Address, 98);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore(), "The score should start at zero, even if the AI already has coins.");
            emu.SetMemory(Addresses.Counters.Coins.Address, 99);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(5, sf.GetFinalScore());
            emu.SetMemory(Addresses.Counters.Coins.Address, 0);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(10, sf.GetFinalScore(), "The score should go up even if the player colects 100 coins");
        }

        [Test]
        public void DiedScoreFactor()
        {
            DiedScoreFactor sf = new() { ScoreMultiplier = -50 };
            emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x00);
            sf.Update(df!);
            Assert.IsFalse(sf.ShouldStop);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x09);
            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            Assert.AreEqual(-50, sf.GetFinalScore());
            df.NextState();
            sf.LevelDone();
            sf.Update(df);
            Assert.AreEqual(-100, sf.GetFinalScore());
        }

        [Test]
        public void DistanceScoreFactor()
        {
            DistanceScoreFactor sf = new() { ScoreMultiplier = 10 };
            emu!.SetMemory(Addresses.Player.PositionX.Address, 0x1F, 0x2E);
            emu.SetMemory(Addresses.Player.IsOnGround.Address, 0x01);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore(), "Distance score should start at 0, even if the starting position isn't 0");
            emu!.SetMemory(Addresses.Player.PositionX.Address, 0x1F + 8, 0x2E + 4);
            df!.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((0.5 + 4 * 16) * 10, sf.GetFinalScore());

            emu.SetMemory(Addresses.Player.PositionX.Address, 0, 0);
            df.NextState();
            sf.Update(df);
            emu.SetMemory(Addresses.Player.PositionX.Address, 0x20, 0);
            df.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((2 + 0.5 + 0x40) * 10, sf.GetFinalScore());

            emu.SetMemory(Addresses.Player.PositionX.Address, 0, 0);
            emu.SetMemory(Addresses.Player.IsOnGround.Address, 0x00);
            df.NextState();
            sf.Update(df);
            emu.SetMemory(Addresses.Player.PositionX.Address, 0x20, 0);
            df.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((2 + 0.5 + 0x40) * 10, sf.GetFinalScore(), "The distance traveled in the air should not have counted.");

            emu.SetMemory(Addresses.Player.PositionX.Address, 0, 0);
            emu.SetMemory(Addresses.Player.IsOnGround.Address, 0x00);
            emu.SetMemory(Addresses.Player.IsInWater.Address, 0x01);
            df.NextState();
            sf.Update(df);
            emu.SetMemory(Addresses.Player.PositionX.Address, 0x20, 0);
            df.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((4 + 0.5 + 0x40) * 10, sf.GetFinalScore(), "The distance traveled in the water should have counted.");
        }

        [Test]
        public void HighScoreScoreFactor()
        {
            HighScoreScoreFactor sf = new() { ScoreMultiplier = 5 };
            emu!.SetMemory(Addresses.Counters.Score.Address, 0x1F, 0x2E, 0x3D);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore(), "Score should start at zero even if the high score isn't.");
            emu.SetMemory(Addresses.Counters.Score.Address, 0x1F + 2, 0x2E + 3, 0x3D + 4);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual((0x02 + 0x0300 + 0x040000) * 5.0 / 100.0, sf.GetFinalScore());
            emu.SetMemory(Addresses.Counters.Score.Address, 0, 0, 0);
            df.NextState();
            sf.LevelDone();
            sf.Update(df);
            Assert.AreEqual((0x02 + 0x0300 + 0x040000) * 5.0 / 100.0, sf.GetFinalScore());
            emu.SetMemory(Addresses.Counters.Score.Address, 1, 0, 0);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual((1 + 0x02 + 0x0300 + 0x040000) * 5.0 / 100.0, sf.GetFinalScore());
        }

        [Test]
        public void OneUpsScoreFactor()
        {
            OneUpsScoreFactor sf = new() { ScoreMultiplier = 150 };
            emu!.SetMemory(Addresses.Counters.Lives.Address, 5);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu.SetMemory(Addresses.Counters.Lives.Address, 7);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(300, sf.GetFinalScore());
            emu.SetMemory(Addresses.Counters.Lives.Address, 0);
            df.NextState();
            sf.LevelDone();
            sf.Update(df);
            emu.SetMemory(Addresses.Counters.Lives.Address, 2);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(600, sf.GetFinalScore());
        }

        [Test]
        public void PowerUpScoreFactor()
        {
            PowerUpScoreFactor sf = new() { ScoreMultiplier = 4 };
            emu!.SetMemory(Addresses.Player.PowerUp.Address, 0);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu.SetMemory(Addresses.Player.PowerUp.Address, 1);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(4, sf.GetFinalScore());
            emu.SetMemory(Addresses.Player.PowerUp.Address, 2);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12, sf.GetFinalScore());
            emu.SetMemory(Addresses.Player.PowerUp.Address, 2);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12, sf.GetFinalScore());
            emu.SetMemory(Addresses.Player.PowerUp.Address, 3);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12, sf.GetFinalScore());
            emu.SetMemory(Addresses.Player.PowerUp.Address, 1);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12, sf.GetFinalScore());

            df.NextState();
            sf.LevelDone();
            Assert.AreEqual(12, sf.GetFinalScore());
        }

        [Test]
        public void SpeedScoreFactor()
        {
            SpeedScoreFactor sf = new() { ScoreMultiplier = 5 };
            emu!.SetMemory(Addresses.Player.IsOnGround.Address, 0x01);
            emu!.SetMemory(Addresses.Player.PositionX.Address, 0x10, 0x00);
            sf.Update(df!);
            emu.SetMemory(Addresses.Player.PositionX.Address, 0x30, 0x00);
            df!.NextFrame();
            sf.Update(df);
            sf.Update(df);
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual(2.0 * 5.0 / (4.0 / 60.0), sf.GetFinalScore());
            df.NextState();
            emu.SetMemory(Addresses.Player.PositionX.Address, 0, 0);
            sf.Update(df);
            emu.SetMemory(Addresses.Player.PositionX.Address, 0x30, 0);
            df.NextFrame();
            sf.Update(df);
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual(2.0 * 5.0 / (4.0 / 60.0) + 3.0 * 5.0 / (3.0 / 60.0), sf.GetFinalScore());
        }

        [Test]
        public void StopMovingScoreFactor()
        {
            StopMovingScoreFactor sf = new() { ScoreMultiplier = -30 };
            emu!.SetMemory(Addresses.Player.PositionX.Address, 0x10, 0x00);
            sf.Update(df!);
            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df!);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-30, sf.GetFinalScore());
            df!.NextState();
            emu.SetMemory(Addresses.Player.PositionX.Address, 0x00, 0x00);
            sf.Update(df);
            ushort currPos = 0x0000;
            for (int i = 0; i < 1200; i++)
            {
                emu.SetMemory(Addresses.Player.PositionX.Address, (byte)(currPos % 0x0100), (byte)(currPos / 0x0100));
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Should not stop while the player is moving forwards");

                currPos += 5;
            }

            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-60, sf.GetFinalScore());
        }

        [Test]
        public void TakenDamageScoreFactor()
        {
            TakenDamageScoreFactor sf = new() { ScoreMultiplier = -1 };
            emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu.SetMemory(Addresses.Player.PlayerAnimationState.Address, 1);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(-1, sf.GetFinalScore());
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(-1, sf.GetFinalScore());
        }

        [Test]
        public void TimeTakenScoreFactor()
        {
            TimeTakenScoreFactor sf = new() { ScoreMultiplier = -100 };
            for (int i = 0; i < Retro_ML.SuperMarioWorld.Neural.Scoring.TimeTakenScoreFactor.MAX_TRAINING_FRAMES; i++)
            {
                sf.Update(df!);
                Assert.IsFalse(sf.ShouldStop, "The score factor shouldn't have stopped yet");
            }
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-100, sf.GetFinalScore());

            for (int i = 0; i < Retro_ML.SuperMarioWorld.Neural.Scoring.TimeTakenScoreFactor.MAX_TRAINING_FRAMES; i++)
            {
                sf.Update(df!);
                Assert.IsFalse(sf.ShouldStop, "The score factor shouldn't have stopped yet");
            }
            sf.Update(df!);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-200, sf.GetFinalScore());
        }

        [Test]
        public void WonLevelScoreFactor()
        {
            WonLevelScoreFactor sf = new() { ScoreMultiplier = 1000 };
            sf.Update(df!);
            Assert.IsFalse(sf.ShouldStop);
            emu!.SetMemory(Addresses.Level.EndLevelTimer.Address, 10);
            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(1000, sf.GetFinalScore());
            emu.SetMemory(Addresses.Level.EndLevelTimer.Address, 0);
            df.NextState();
            Assert.IsFalse(sf.ShouldStop);
            emu.SetMemory(Addresses.Level.KeyholeTimer.Address, 0x30);
            df.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(2000, sf.GetFinalScore());
        }

        [Test]
        public void YoshiCoinsScoreFactor()
        {
            YoshiCoinsScoreFactor sf = new() { ScoreMultiplier = 10 };
            emu!.SetMemory(Addresses.Counters.YoshiCoinCollected.Address, 3);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu.SetMemory(Addresses.Counters.YoshiCoinCollected.Address, 5);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(20, sf.GetFinalScore());
            sf.LevelDone();
            emu.SetMemory(Addresses.Counters.YoshiCoinCollected.Address, 0);
            df.NextState();
            sf.Update(df);
            emu.SetMemory(Addresses.Counters.YoshiCoinCollected.Address, 3);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(50, sf.GetFinalScore());
        }
    }
}
