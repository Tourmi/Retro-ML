using NUnit.Framework;
using Retro_ML.SNES;

namespace Retro_ML_TEST.Console
{
    [TestFixture]
    internal class SNESInputTest
    {
        [Test]
        public void Constructor()
        {
            var input = new SNESInput();

            Assert.False(input.GetButtonState(SNESInput.Buttons.A));
            Assert.False(input.GetButtonState(SNESInput.Buttons.B));
            Assert.False(input.GetButtonState(SNESInput.Buttons.X));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Y));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Left));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Right));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Up));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Down));
            Assert.False(input.GetButtonState(SNESInput.Buttons.LeftShoulder));
            Assert.False(input.GetButtonState(SNESInput.Buttons.RightShoulder));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Start));
            Assert.False(input.GetButtonState(SNESInput.Buttons.Select));

            input.FromString("ABXYlrudLRSs");

            Assert.True(input.GetButtonState(SNESInput.Buttons.A));
            Assert.True(input.GetButtonState(SNESInput.Buttons.B));
            Assert.True(input.GetButtonState(SNESInput.Buttons.X));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Y));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Left));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Right));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Up));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Down));
            Assert.True(input.GetButtonState(SNESInput.Buttons.LeftShoulder));
            Assert.True(input.GetButtonState(SNESInput.Buttons.RightShoulder));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Start));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Select));

            input.FromString("rlsR");
            Assert.True(input.GetButtonState(SNESInput.Buttons.Select));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Left));
            Assert.True(input.GetButtonState(SNESInput.Buttons.Right));
            Assert.True(input.GetButtonState(SNESInput.Buttons.RightShoulder));
        }

        [Test]
        public void ButtonState()
        {
            var input = new SNESInput();

            input.SetButtonState(SNESInput.Buttons.X, true);
            Assert.True(input.GetButtonState(SNESInput.Buttons.X));
            input.SetButtonState(SNESInput.Buttons.X, false);
            Assert.False(input.GetButtonState(SNESInput.Buttons.X));
        }

        [Test]
        public void GetButtonBytes()
        {
            var input = new SNESInput();
            Assert.AreEqual(new byte[] { 0b00000000, 0b00000000 }, input.ToArduinoBytes());

            input.FromString("ABXYlrudLRSs");
            Assert.AreEqual(new byte[] { 0b11111111, 0b00001111 }, input.ToArduinoBytes());

            input.FromString("BYuRs");
            Assert.AreEqual(new byte[] { 0b01001010, 0b00001010 }, input.ToArduinoBytes());
        }

        [Test]
        public void GetString()
        {
            var input = new SNESInput();
            Assert.AreEqual("P1()", input.GetString());

            input.FromString("sSRLdurlYXBA");
            Assert.AreEqual("P1(ABXYlrudLRSs)", input.GetString());
        }
    }
}
