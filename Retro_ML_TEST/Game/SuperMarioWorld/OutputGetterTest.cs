using NUnit.Framework;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;

namespace SMW_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class OutputGetterTest
    {
        private OutputGetter? outputGetter;

        [SetUp]
        public void SetUp()
        {
            outputGetter = new OutputGetter(new NeuralConfig());
        }

        [Test]
        public void GetControllerInput()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void GetOutputCount()
        {
            Assert.Ignore("Not implemented yet");
        }
    }
}
