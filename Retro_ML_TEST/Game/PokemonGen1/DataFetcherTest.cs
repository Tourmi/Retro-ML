using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.PokemonGen1.Configuration;
using Retro_ML.PokemonGen1.Game;
using Retro_ML_TEST.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML_TEST.Game.PokemonGen1
{
    [TestFixture]
    internal class DataFetcherTest
    {
        private PokemonDataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new PokemonDataFetcher(mockEmulatorAdapter, new NeuralConfig(), new PokemonPluginConfig());
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
        public void GetOpposingPokemonSleep()
        {
            Assert.AreEqual(0, dataFetcher!.GetOpposingPokemonSleep());

            //mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.PosY.Address, 24);

            //dataFetcher.NextFrame();
            //Assert.AreEqual(0, dataFetcher!.GetPositionY() * 17);

            //mockEmulatorAdapter!.SetMemory(Addresses.CurrentBlock.PosY.Address, 32);

            //dataFetcher.NextFrame();
            //Assert.AreEqual(1, dataFetcher!.GetPositionY() * 17);
        }

    }
}
