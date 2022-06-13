using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperMarioBros.Configuration;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML.SuperMarioBros.Neural.Scoring;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.SuperMarioBros
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private SMBDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new SMBDataFetcher(emu, new NeuralConfig(), new SMBPluginConfig());
        }

        //[Test]
        //public void Score()
        //{
        //    Score score = new(new IScoreFactor[] { new DiedScoreFactor() { ScoreMultiplier = -20 }, new WonLevelScoreFactor() { ScoreMultiplier = 30 } });
        //    score.Update(df!);
        //    Assert.IsFalse(score.ShouldStop);
        //    Assert.AreEqual(1, score.GetFinalScore());
        //    emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x09);
        //    emu!.SetMemory(Addresses.Level.KeyholeTimer.Address, 0x30);
        //    df!.NextFrame();
        //    score.Update(df);
        //    Assert.IsTrue(score.ShouldStop);
        //    score.LevelDone();
        //    Assert.AreEqual(11, score.GetFinalScore());

        //    emu!.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x00);
        //    emu!.SetMemory(Addresses.Level.KeyholeTimer.Address, 0);
        //    df.NextState();
        //    score.Update(df);
        //    Assert.IsFalse(score.ShouldStop);
        //    emu!.SetMemory(Addresses.Level.EndLevelTimer.Address, 0xFF);
        //    df.NextFrame();
        //    score.Update(df);
        //    Assert.IsTrue(score.ShouldStop);
        //    score.LevelDone();
        //    Assert.AreEqual(41, score.GetFinalScore());

        //    score = new(new IScoreFactor[] { new DiedScoreFactor() { ScoreMultiplier = -0.5 } });
        //    emu.SetMemory(Addresses.Player.PlayerAnimationState.Address, 0x09);
        //    df.NextFrame();
        //    score.Update(df);
        //    score.LevelDone();
        //    Assert.AreEqual(1 / 1.5, score.GetFinalScore());
        //    score.Update(df);
        //    score.LevelDone();
        //    Assert.AreEqual(1 / 2.0, score.GetFinalScore());

        //}

        [Test]
        public void CoinsScoreFactor()
        {
            CoinsScoreFactor sf = new() { ScoreMultiplier = 5 };
            emu!.SetMemory(Addresses.GameAddresses.Coins.Address, 98);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore(), "The score should start at zero, even if the AI already has coins.");
            emu.SetMemory(Addresses.GameAddresses.Coins.Address, 99);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(5, sf.GetFinalScore());
            emu.SetMemory(Addresses.GameAddresses.Coins.Address, 0);
            df.NextFrame();
            sf.Update(df);
            Assert.AreEqual(10, sf.GetFinalScore(), "The score should go up even if the player colects 100 coins");
        }

        [Test]
        public void DiedScoreFactor()
        {
            DiedScoreFactor sf = new() { ScoreMultiplier = -50 };
            emu!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x08);
            sf.Update(df!);
            Assert.IsFalse(sf.ShouldStop);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x0B);
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
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0xAA);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x00);
            emu!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x00);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore(), "Distance score should start at 0, even if the starting position isn't 0");
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0xAF);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x00);
            df!.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual(5 / 16 * 10, sf.GetFinalScore());

            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x00);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x00);
            df.NextState();
            sf.Update(df);
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x20);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x00);
            df.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((2 + 0.5 + 0x40) * 10, sf.GetFinalScore());

            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x00);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x01);
            emu!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x01);
            df.NextState();
            sf.Update(df);
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x20);
            df.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((2 + 0.5 + 0x40) * 10, sf.GetFinalScore(), "The distance traveled in the air should not have counted.");

            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x00);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x01);
            emu!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x00);
            emu!.SetMemory(Addresses.PlayerAddresses.IsSwimming.Address, 0x00);
            df.NextState();
            sf.Update(df);
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x20);
            df.NextFrame();
            sf.Update(df);
            sf.LevelDone();
            Assert.AreEqual((4 + 0.5 + 0x40) * 10, sf.GetFinalScore(), "The distance traveled in the water should have counted.");
        }

        [Test]
        public void PowerUpScoreFactor()
        {
            PowerUpScoreFactor sf = new() { ScoreMultiplier = 4 };
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPowerupState.Address, 0);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPowerupState.Address, 1);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(4, sf.GetFinalScore());
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPowerupState.Address, 2);
            df!.NextFrame();
            sf.Update(df);
            Assert.AreEqual(12, sf.GetFinalScore());
            df.NextState();
            sf.LevelDone();
            Assert.AreEqual(12, sf.GetFinalScore());
        }

        [Test]
        public void StopMovingScoreFactor()
        {
            StopMovingScoreFactor sf = new() { ScoreMultiplier = -30 };
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0xAA);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x01);
            sf.Update(df!);
            for (int i = 0; i < 1000 && !sf.ShouldStop; i++)
            {
                sf.Update(df!);
            }
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(-30, sf.GetFinalScore());
            df!.NextState();
            emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x00);
            emu!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x00);
            sf.Update(df);
            byte currPos = 0x01;
            for (int i = 0; i < 254; i++)
            {
                emu!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, (byte)(0x00 + currPos));
                df.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop, "Should not stop while the player is moving forwards");
                currPos += 0x01;
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
            emu!.SetMemory(Addresses.GameAddresses.WonCondition.Address, 0x02);
            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(1000, sf.GetFinalScore());
        }
    }
}
