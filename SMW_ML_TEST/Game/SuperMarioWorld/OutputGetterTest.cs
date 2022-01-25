using NUnit.Framework;
using SMW_ML.Game.SuperMarioWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML_TEST.Game.SuperMarioWorld
{
    [TestFixture]
    internal class OutputGetterTest
    {
        private OutputGetter? outputGetter;

        [SetUp]
        public void SetUp()
        {
            outputGetter = new OutputGetter();
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
