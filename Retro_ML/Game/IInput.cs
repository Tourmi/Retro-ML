namespace Retro_ML.Game
{
    public interface IInput
    {
        /// <summary>
        /// Returns the amount of buttons or axis for the input method.
        /// </summary>
        int ButtonCount { get; }

        /// <summary>
        /// Sets the given button based on the index and given value.
        /// </summary>
        void SetButton(int index, double value);
        /// <summary>
        /// Validates and corrects buttons if their configuration is invalid (Such as pressing left and right on a DPad)
        /// </summary>
        void ValidateButtons();

        /// <summary>
        /// Parses the given string into this input
        /// </summary>
        void FromString(string value);
        /// <summary>
        /// Returns the string representation of this input
        /// </summary>
        string GetString();
        /// <summary>
        /// <br>Returns two bitmaps based on the input, the first one containing, in order, the status of the A B X Y left right up down key. </br>
        /// <br>The second one contains the status of LShoulder RShoulder Start Select.                                                       </br>
        /// <br>Used for Serial port communications.                                                                                          </br>
        /// </summary>
        byte[] ToArduinoBytes();
    }
}
