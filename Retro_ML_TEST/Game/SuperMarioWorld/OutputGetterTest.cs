using NUnit.Framework;
using Retro_ML.Game;
using Retro_ML.SuperMarioWorld.Configuration;

namespace Retro_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class OutputGetterTest
    {
        private OutputGetter? outputGetter;

        [SetUp]
        public void SetUp()
        {
            outputGetter = new OutputGetter(new SMWNeuralConfig());
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
