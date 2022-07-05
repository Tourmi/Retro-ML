using NUnit.Framework;
using Retro_ML.N64;

namespace Retro_ML_TEST.Console;
internal class N64InputTest
{
    [Test]
    public void GetButtonBytes()
    {
        var input = new N64Input();
        input.SetButton(0, 1.0); //A
        input.SetButton(10, 1.0); //Z
        input.SetButton(7, 1.0); // Dpad right
        input.SetButton(15, -1.0); //Joystick Y
        input.SetButton(3, 1.0); //C-right

        Assert.AreEqual(new byte[] { 0b0110_1101, 0b0000_1000 }, input.ToArduinoBytes());
    }

    [Test]
    public void GetString()
    {
        var input = new N64Input();
        input.SetButton(14, -64 / 128.0); //Joystick X
        input.SetButton(1, 0.3); //B
        input.SetButton(0, 1.0); //A

        Assert.AreEqual("P1(AJx-64;Jy0;)", input.GetString());
    }

    [Test]
    public void FromString()
    {
        var input = new N64Input();

        Assert.AreEqual("P1(Jx0;Jy0;)", input.GetString());

        input.FromString("ABZJy-128;LRSClCrCuJx127;CdDlDrDuDd");
        Assert.AreEqual("P1(ABClCrCuCdDlDrDuDdZLRSJx127;Jy-128;)", input.GetString());
    }

    [Test]
    public void ValidateButtons()
    {
        var input = new N64Input();
        input.SetButton(6, 1.0);
        input.SetButton(7, 1.0);

        Assert.AreEqual("P1(DlDrJx0;Jy0;)", input.GetString());

        input.ValidateButtons();
        Assert.AreEqual("P1(Jx0;Jy0;)", input.GetString());
    }
}
