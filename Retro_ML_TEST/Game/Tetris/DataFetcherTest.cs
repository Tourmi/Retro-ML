using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Tetris.Configuration;
using Retro_ML.Tetris.Game;
using Retro_ML_TEST.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML_TEST.Game.Tetris
{
    [TestFixture]
    internal class DataFetcherTest
    {
        private TetrisDataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new TetrisDataFetcher(mockEmulatorAdapter, new NeuralConfig(), new TetrisPluginConfig());
            dataFetcher.NextFrame();
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
        public void GetCurrentBlock()
        {
            var expected = new bool[,] { { true, false, false, false, false, false, false } };

            Assert.AreEqual(expected, dataFetcher!.GetCurrentBlockType());

            mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.Type.Address, 0x04);

            expected[0, 1] = true;
            expected[0, 0] = false;

            dataFetcher.NextFrame();
            Assert.AreEqual(expected, dataFetcher!.GetCurrentBlockType());
        }

        [Test]
        public void GetNextBlock()
        {
            var expected = new bool[,] { { true, false, false, false, false, false, false } };

            Assert.AreEqual(expected, dataFetcher!.GetNextBlockType());

            mockEmulatorAdapter!.SetMemory(Addresses.NextBlock.Type.Address, 0x08);

            expected[0, 2] = true;
            expected[0, 0] = false;

            dataFetcher.NextFrame();
            Assert.AreEqual(expected, dataFetcher!.GetNextBlockType());
        }

        [Test]
        public void GetCurrentBlockRotation()
        {
            var expected = new bool[,] { { true, false, false, false } };

            Assert.AreEqual(expected, dataFetcher!.GetCurrentBlockRotation());

            mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.Type.Address, 0x06);

            expected[0, 2] = true;
            expected[0, 0] = false;

            dataFetcher!.NextFrame();
            Assert.AreEqual(expected, dataFetcher!.GetCurrentBlockRotation());
        }

        [Test]
        public void GetNextBlockRotation()
        {
            var expected = new bool[,] { { true, false, false, false } };

            Assert.AreEqual(expected, dataFetcher!.GetNextBlockRotation());

            mockEmulatorAdapter!.SetMemory(Addresses.NextBlock.Type.Address, 0x09);

            expected[0, 1] = true;
            expected[0, 0] = false;

            dataFetcher!.NextFrame();
            Assert.AreEqual(expected, dataFetcher!.GetNextBlockRotation());
        }

        [Test]
        public void CountHoles()
        {
            bool[,] tiles = new bool[,] { { false, false, false, false, false, false, false, false, false, false },
                                        { false, false, true, false, false, false, false, false, false, false },
                                        { true, true, true, true, true, true, false, false, false, false },
                                        { false, false, true, false, false, true, false, false, false, false },
                                        { true, true, true, true, true, false, false, false, false, false },
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true},
                                        { true, true, true, true, true, true, true, true, true, true}};

            int holes = dataFetcher!.CountHoles(tiles);

            Assert.AreEqual(2, holes);
        }

        [Test]
        public void GetPositionY()
        {
            Assert.AreEqual(-3, dataFetcher!.GetPositionY() * 17);

            mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.PosY.Address, 24);

            dataFetcher.NextFrame();
            Assert.AreEqual(0, dataFetcher!.GetPositionY() * 17);

            mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.PosY.Address, 32);

            dataFetcher.NextFrame();
            Assert.AreEqual(1, dataFetcher!.GetPositionY() * 17);
        }

        [Test]
        public void GetPositionX()
        {
            Assert.AreEqual(-3, dataFetcher!.GetPositionX() * 10);

            mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.PosX.Address, 63);

            dataFetcher.NextFrame();
            Assert.AreEqual(4, dataFetcher!.GetPositionX() * 10);

            mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.PosX.Address, 55);

            dataFetcher.NextFrame();
            Assert.AreEqual(3, dataFetcher!.GetPositionX() * 10);
        }

        [Test]
        public void IsGameOver()
        {
            Assert.AreEqual(false, dataFetcher!.IsGameOver());

            mockEmulatorAdapter!.SetMemory(Addresses.GameStatus.Address, 0x0D);

            dataFetcher.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsGameOver());

            mockEmulatorAdapter!.SetMemory(Addresses.GameStatus.Address, 0x04);

            dataFetcher.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsGameOver());

            mockEmulatorAdapter!.SetMemory(Addresses.GameStatus.Address, 0x01);

            dataFetcher.NextFrame();
            Assert.AreEqual(true, dataFetcher!.IsGameOver());
        }

        [Test]
        public void GetSingles()
        {
            Assert.AreEqual(0, dataFetcher!.GetSingles());

            mockEmulatorAdapter!.SetMemory(Addresses.Score.Single.Address, 5);
            dataFetcher.NextFrame();
            Assert.AreEqual(5, dataFetcher!.GetSingles());
        }

        [Test]
        public void GetDoubles()
        {
            Assert.AreEqual(0, dataFetcher!.GetDoubles());

            mockEmulatorAdapter!.SetMemory(Addresses.Score.Double.Address, 5);
            dataFetcher.NextFrame();
            Assert.AreEqual(5, dataFetcher!.GetDoubles());
        }

        [Test]
        public void GetTriples()
        {
            Assert.AreEqual(0, dataFetcher!.GetTriples());

            mockEmulatorAdapter!.SetMemory(Addresses.Score.Triple.Address, 5);
            dataFetcher.NextFrame();
            Assert.AreEqual(5, dataFetcher!.GetTriples());
        }

        [Test]
        public void GetTetrises()
        {
            Assert.AreEqual(0, dataFetcher!.GetTetrises());

            mockEmulatorAdapter!.SetMemory(Addresses.Score.Tetris.Address, 5);
            dataFetcher.NextFrame();
            Assert.AreEqual(5, dataFetcher!.GetTetrises());
        }
    }
}
