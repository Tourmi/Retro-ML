using NUnit.Framework;
using Retro_ML.Game;
using Retro_ML.SuperMarioWorld.Configuration;
using Retro_ML.SuperMarioWorld.Game;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class InputSetterTest
    {
        private InputSetter? inputSetter;
        private SMWDataFetcher? dataFetcher;
        private MockEmulatorAdapter? mea;

        [SetUp]
        public void SetUp()
        {
            mea = new MockEmulatorAdapter();
            dataFetcher = new SMWDataFetcher(mea, new SMWNeuralConfig());
            inputSetter = new InputSetter(dataFetcher, new SMWNeuralConfig());
        }

        [Test]
        public void GetInputCount()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void SetInputs()
        {
            Assert.Ignore("Not implemented yet");
        }
    }
}
