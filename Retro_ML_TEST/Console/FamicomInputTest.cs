using NUnit.Framework;
using Retro_ML.Famicom;

namespace Retro_ML_TEST.Console
{
    [TestFixture]
    internal class FamicomInputTest
    {
        [Test]
        public void Constructor()
        {
            var input = new FamicomInput();

            Assert.False(input.GetButtonState(FamicomInput.Buttons.A));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.B));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.Left));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.Right));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.Up));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.Down));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.Start));
            Assert.False(input.GetButtonState(FamicomInput.Buttons.Select));

            input.FromString("ABlrudSs");

            Assert.True(input.GetButtonState(FamicomInput.Buttons.A));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.B));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Left));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Right));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Up));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Down));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Start));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Select));

            input.FromString("rls");
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Select));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Left));
            Assert.True(input.GetButtonState(FamicomInput.Buttons.Right));
        }

        [Test]
        public void ButtonState()
        {
            var input = new FamicomInput();

            input.SetButtonState(FamicomInput.Buttons.A, true);
            Assert.True(input.GetButtonState(FamicomInput.Buttons.A));
            input.SetButtonState(FamicomInput.Buttons.A, false);
            Assert.False(input.GetButtonState(FamicomInput.Buttons.A));
        }

        [Test]
        public void GetButtonBytes()
        {
            var input = new FamicomInput();
            Assert.AreEqual(new byte[] { 0b00000000, 0b00000000 }, input.ToArduinoBytes());

            input.FromString("ABlrudSs");
            Assert.AreEqual(new byte[] { 0b11110011, 0b00001100 }, input.ToArduinoBytes());
        }

        [Test]
        public void GetString()
        {
            var input = new FamicomInput();
            Assert.AreEqual("P1()", input.GetString());

            input.FromString("sSdurlBA");
            Assert.AreEqual("P1(ABlrudSs)", input.GetString());
        }
    }
}
