using NUnit.Framework;
using Retro_ML.Arduino;

namespace Retro_ML_TEST.Arduino
{
    [TestFixture]
    internal class ArduinoPreviewerTest
    {
        private ArduinoPreviewer? previewer;

        [SetUp]
        public void SetUp()
        {
            if (!ArduinoPreviewer.ArduinoAvailable("COM4"))
            {
                Assert.Ignore("No arduinos are currently available on a serial port");
            }

            previewer = new ArduinoPreviewer("COM4");
        }

        [Test]
        public void SendInput()
        {
            Assert.IsNotNull(previewer);

            Assert.DoesNotThrow(() => previewer!.SendInput(new("")));
            Assert.DoesNotThrow(() => previewer!.SendInput(new("ABXYlrudLRSs")));
        }

        [TearDown]
        public void TearDown()
        {
            previewer?.Dispose();
        }
    }
}
