using NUnit.Framework;
using Retro_ML.Game.SuperMarioWorld;
using Retro_ML.Models.Config;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class InputSetterTest
    {
        private InputSetter? inputSetter;
        private DataFetcher? dataFetcher;
        private MockEmulatorAdapter? mea;

        [SetUp]
        public void SetUp()
        {
            mea = new MockEmulatorAdapter();
            dataFetcher = new DataFetcher(mea, new NeuralConfig());
            inputSetter = new InputSetter(dataFetcher, new NeuralConfig());
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
