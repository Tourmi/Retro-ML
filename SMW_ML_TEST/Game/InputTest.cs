using NUnit.Framework;
using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML_TEST.Game
{
    [TestFixture]
    internal class InputTest
    {
        [Test]
        public void Constructor()
        {
            var input = new Input("");

            Assert.False(input.GetButtonState(Input.Buttons.A));
            Assert.False(input.GetButtonState(Input.Buttons.B));
            Assert.False(input.GetButtonState(Input.Buttons.X));
            Assert.False(input.GetButtonState(Input.Buttons.Y));
            Assert.False(input.GetButtonState(Input.Buttons.Left));
            Assert.False(input.GetButtonState(Input.Buttons.Right));
            Assert.False(input.GetButtonState(Input.Buttons.Up));
            Assert.False(input.GetButtonState(Input.Buttons.Down));
            Assert.False(input.GetButtonState(Input.Buttons.LeftShoulder));
            Assert.False(input.GetButtonState(Input.Buttons.RightShoulder));
            Assert.False(input.GetButtonState(Input.Buttons.Start));
            Assert.False(input.GetButtonState(Input.Buttons.Select));

            input = new Input("ABXYlrudLRSs");

            Assert.True(input.GetButtonState(Input.Buttons.A));
            Assert.True(input.GetButtonState(Input.Buttons.B));
            Assert.True(input.GetButtonState(Input.Buttons.X));
            Assert.True(input.GetButtonState(Input.Buttons.Y));
            Assert.True(input.GetButtonState(Input.Buttons.Left));
            Assert.True(input.GetButtonState(Input.Buttons.Right));
            Assert.True(input.GetButtonState(Input.Buttons.Up));
            Assert.True(input.GetButtonState(Input.Buttons.Down));
            Assert.True(input.GetButtonState(Input.Buttons.LeftShoulder));
            Assert.True(input.GetButtonState(Input.Buttons.RightShoulder));
            Assert.True(input.GetButtonState(Input.Buttons.Start));
            Assert.True(input.GetButtonState(Input.Buttons.Select));

            input = new Input("rlsR");
            Assert.True(input.GetButtonState(Input.Buttons.Select));
            Assert.True(input.GetButtonState(Input.Buttons.Left));
            Assert.True(input.GetButtonState(Input.Buttons.Right));
            Assert.True(input.GetButtonState(Input.Buttons.RightShoulder));
        }

        [Test]
        public void ButtonState()
        {
            var input = new Input("");

            input.SetButtonState(Input.Buttons.X, true);
            Assert.True(input.GetButtonState(Input.Buttons.X));
            input.SetButtonState(Input.Buttons.X, false);
            Assert.False(input.GetButtonState(Input.Buttons.X));
        }

        [Test]
        public void GetButtonBytes()
        {
            var input = new Input("");
            Assert.AreEqual(new byte[] { 0b00000000, 0b00000000 }, input.GetButtonBytes());

            input = new Input("ABXYlrudLRSs");
            Assert.AreEqual(new byte[] { 0b11111111, 0b00001111 }, input.GetButtonBytes());

            input = new Input("BYuRs");
            Assert.AreEqual(new byte[] { 0b01001010, 0b00001010 }, input.GetButtonBytes());
        }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        [Test]
        public void ToString()
        {
            var input = new Input("");
            Assert.AreEqual("", input.ToString());

            input = new Input("sSRLdurlYXBA");
            Assert.AreEqual("ABXYlrudLRSs", input.ToString());
        }
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword

        [Test]
        public void IndexToButton()
        {
            int currIndex = 0;
            Assert.AreEqual(Input.Buttons.A, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.B, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.X, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Y, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Left, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Right, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Up, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Down, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.LeftShoulder, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.RightShoulder, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Start, Input.IndexToButton(currIndex++));
            Assert.AreEqual(Input.Buttons.Select, Input.IndexToButton(currIndex++));
        }
    }
}
