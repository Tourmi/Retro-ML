using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Tetris.Configuration;
using Retro_ML.Tetris.Game;
using Retro_ML.Tetris.Neural.Scoring;
using Retro_ML_TEST.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML_TEST.Game.Tetris
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private TetrisDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new TetrisDataFetcher(emu, new NeuralConfig(), new TetrisPluginConfig());
        }

        [Test]
        public void GameOverScoreFactor()
        {
            var sf = new GameOverScoreFactor() { ScoreMultiplier = -10 }.Clone();
            Assert.IsAssignableFrom<GameOverScoreFactor>(sf);
            emu!.SetMemory(Addresses.GameStatus.Address, 0x00);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.GameStatus.Address, 0x01);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(-10, sf.GetFinalScore());
        }

        [Test]
        public void HoleScoreFactor()
        {
            var sf = new HoleScoreFactor() { ScoreMultiplier = -2 }.Clone();
            Assert.IsAssignableFrom<HoleScoreFactor>(sf);
            Assert.AreEqual(0, sf.GetFinalScore());
        }

        [Test]
        public void TimeTakenScoreFactor()
        {
            var sf = new TimeTakenScoreFactor() { ScoreMultiplier = .1 }.Clone();
            Assert.IsAssignableFrom<TimeTakenScoreFactor>(sf);
            for (int i = 0; i < 60.0 * 600 - 1; i++)
            {
                df!.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop);
            }

            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(0.1 * 600, sf.GetFinalScore(), 0.00001);

            for (int i = 0; i < 60.0 * 600 - 1; i++)
            {
                df!.NextFrame();
                sf.Update(df);
                Assert.IsFalse(sf.ShouldStop);
            }

            df!.NextFrame();
            sf.Update(df);
            Assert.IsTrue(sf.ShouldStop);
            sf.LevelDone();
            Assert.AreEqual(0.1 * 600 * 2, sf.GetFinalScore(), 0.00001);
        }

        [Test]
        public void LineClearScoreFactor()
        {
            var sf = new LineClearedScoreFactor() { ScoreMultiplier = 10 }.Clone();
            Assert.IsAssignableFrom<LineClearedScoreFactor>(sf);
            emu!.SetMemory(Addresses.Score.Single.Address, 0x00);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());

            emu!.SetMemory(Addresses.Score.Single.Address, 1);
            df!.NextFrame();
            sf.Update(df!);

            emu!.SetMemory(Addresses.Score.Single.Address, 2);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(20, sf.GetFinalScore());

            emu!.SetMemory(Addresses.Score.Double.Address, 1);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(40, sf.GetFinalScore());

            emu!.SetMemory(Addresses.Score.Triple.Address, 1);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(80, sf.GetFinalScore());

            emu!.SetMemory(Addresses.Score.Tetris.Address, 1);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(240, sf.GetFinalScore());
        }

    }
}
