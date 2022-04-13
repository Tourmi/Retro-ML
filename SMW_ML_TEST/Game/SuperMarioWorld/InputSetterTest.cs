using NUnit.Framework;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using SMW_ML_TEST.Emulator;

namespace SMW_ML_TEST.Game.SuperMarioWorld
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
