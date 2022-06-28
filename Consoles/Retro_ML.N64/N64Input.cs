using Retro_ML.Game;

namespace Retro_ML.N64;
/// <summary>
/// Input class for Nintendo 64 inputs
/// </summary>
internal class N64Input : IInput
{
    private const double JOYSTICK_MAX_VALUE = 128.0;

    private static readonly string A = "A";
    private static readonly string B = "B";
    private static readonly string C_LEFT = "Cl";
    private static readonly string C_RIGHT = "Cr";
    private static readonly string C_UP = "Cu";
    private static readonly string C_DOWN = "Cd";
    private static readonly string DPAD_LEFT = "Dl";
    private static readonly string DPAD_RIGHT = "Dr";
    private static readonly string DPAD_UP = "Du";
    private static readonly string DPAD_DOWN = "Dd";
    private static readonly string Z = "Z";
    private static readonly string LEFT_SHOULDER = "L";
    private static readonly string RIGHT_SHOULDER = "R";
    private static readonly string START = "S";

    private static readonly string[] JOYSTICKS = new string[] { "Jx", "Jy" };
    private static readonly string[] BUTTONS = new string[] {
            A,
            B,
            C_LEFT,
            C_RIGHT,
            C_UP,
            C_DOWN,
            DPAD_LEFT,
            DPAD_RIGHT,
            DPAD_UP,
            DPAD_DOWN,
            Z,
            LEFT_SHOULDER,
            RIGHT_SHOULDER,
            START,
            JOYSTICKS[0],
            JOYSTICKS[1]
    };
    private static readonly int JOYSTICK_START_INDEX = Array.IndexOf(BUTTONS, JOYSTICKS[0]);

    private readonly double[] buttonStatuses;

    public N64Input() => buttonStatuses = new double[BUTTONS.Length];

    public int ButtonCount => BUTTONS.Length;

    public void FromString(string inputString)
    {
        for (int i = 0; i < buttonStatuses.Length; i++)
        {
            buttonStatuses[i] = 0;
        }

        var currString = inputString;

        while (currString.Length > 0)
        {
            var btnIndex = Array.FindIndex(BUTTONS, b => currString.StartsWith(b));

            if (btnIndex < 0) throw new ArgumentException($"Input string '{inputString}' is not a valid Nintendo 64 string");

            double valueToSet = 1.0;
            int buttonLength = BUTTONS[btnIndex].Length;
            if (btnIndex >= JOYSTICK_START_INDEX)
            {
                int startJoystickIndex = buttonLength;
                int endJoystickIndex = currString.IndexOf(";");
                sbyte joystickValue = sbyte.Parse(currString[startJoystickIndex..endJoystickIndex]);

                valueToSet = joystickValue / JOYSTICK_MAX_VALUE;

                buttonLength = endJoystickIndex + 1;
            }

            buttonStatuses[btnIndex] = valueToSet;
            currString = currString[buttonLength..];
        }
    }

    public string GetString()
    {
        string output = string.Empty;

        for (int i = 0; i < JOYSTICK_START_INDEX; i++)
        {
            if (buttonStatuses[i] > IInput.INPUT_THRESHOLD) output += BUTTONS[i];
        }

        for (int i = 0; i < JOYSTICKS.Length; i++)
        {
            var doubleValue = buttonStatuses[i + JOYSTICK_START_INDEX] * JOYSTICK_MAX_VALUE;
            sbyte value = (sbyte)Math.Clamp(doubleValue, sbyte.MinValue, sbyte.MaxValue);
            output += JOYSTICKS[i] + value.ToString() + ";";
        }

        return $"P1({output})";
    }

    public void SetButton(int index, double value) => buttonStatuses[index] = value;

    public byte[] ToArduinoBytes()
    {
        byte[] bytes = new byte[2];

        int currIndex = 0;
        bytes[0] |= (byte)((GetButtonState(A) ? 1 : 0) << currIndex++);
        bytes[0] |= (byte)((GetButtonState(B) ? 1 : 0) << currIndex++);
        //Z -> X Button
        bytes[0] |= (byte)((GetButtonState(Z) ? 1 : 0) << currIndex++);
        //Z -> Y Button
        bytes[0] |= (byte)((GetButtonState(Z) ? 1 : 0) << currIndex++);
        //Joystick left > threshold, or dpad left -> Dpad left
        bytes[0] |= (byte)((-buttonStatuses[GetButtonIndex(JOYSTICKS[0])] > IInput.INPUT_THRESHOLD || GetButtonState(DPAD_LEFT) ? 1 : 0) << currIndex++);
        //Joystick right > threshold, or dpad right -> Dpad right
        bytes[0] |= (byte)((buttonStatuses[GetButtonIndex(JOYSTICKS[0])] > IInput.INPUT_THRESHOLD || GetButtonState(DPAD_RIGHT) ? 1 : 0) << currIndex++);
        //Joystick up > threshold, or dpad up -> Dpad up
        bytes[0] |= (byte)((-buttonStatuses[GetButtonIndex(JOYSTICKS[1])] > IInput.INPUT_THRESHOLD || GetButtonState(DPAD_UP) ? 1 : 0) << currIndex++);
        //Joystick down > threshold, or dpad down -> Dpad down
        bytes[0] |= (byte)((buttonStatuses[GetButtonIndex(JOYSTICKS[1])] > IInput.INPUT_THRESHOLD || GetButtonState(DPAD_DOWN) ? 1 : 0) << currIndex++);

        currIndex = 0;

        bytes[1] |= (byte)((GetButtonState(LEFT_SHOULDER) ? 1 : 0) << currIndex++);
        bytes[1] |= (byte)((GetButtonState(RIGHT_SHOULDER) ? 1 : 0) << currIndex++);
        bytes[1] |= (byte)((GetButtonState(START) ? 1 : 0) << currIndex++);
        //Any C buttons -> Select button
        bytes[1] |= (byte)((GetButtonState(C_LEFT) || GetButtonState(C_RIGHT) || GetButtonState(C_UP) || GetButtonState(C_DOWN) ? 1 : 0) << currIndex++);

        return bytes;
    }

    public void ValidateButtons()
    {
        if (GetButtonState(DPAD_LEFT) && GetButtonState(DPAD_RIGHT))
        {
            SetButtonState(DPAD_LEFT, false);
            SetButtonState(DPAD_RIGHT, false);
        }
        if (GetButtonState(DPAD_UP) && GetButtonState(DPAD_DOWN))
        {
            SetButtonState(DPAD_UP, false);
            SetButtonState(DPAD_DOWN, false);
        }
    }

    /// <summary>
    /// Returns the state of the button, based on the <see cref="IInput.INPUT_THRESHOLD"/>
    /// </summary>
    private bool GetButtonState(string name) => buttonStatuses[GetButtonIndex(name)] > IInput.INPUT_THRESHOLD;
    /// <summary>
    /// Sets the given button to 0 or 1, depending on the state that's given
    /// </summary>
    private void SetButtonState(string name, bool state) => SetButton(GetButtonIndex(name), state ? 1 : 0);

    /// <summary>
    /// Returns the index of the given button name
    /// </summary>
    private static int GetButtonIndex(string name) => Array.IndexOf(BUTTONS, name);
}
