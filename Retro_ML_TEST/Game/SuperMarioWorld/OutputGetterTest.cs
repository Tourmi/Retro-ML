﻿using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.Game;

namespace Retro_ML_TEST.Game.SuperMarioWorld
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
