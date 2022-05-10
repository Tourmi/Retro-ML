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

            Assert.DoesNotThrow(() => previewer!.SendInput(new byte[] { 0b0000_0000, 0b0000_0000 }));
            Assert.DoesNotThrow(() => previewer!.SendInput(new byte[] { 0b1111_1111, 0b0000_1111 }));
        }

        [TearDown]
        public void TearDown()
        {
            previewer?.Dispose();
        }
    }
}
