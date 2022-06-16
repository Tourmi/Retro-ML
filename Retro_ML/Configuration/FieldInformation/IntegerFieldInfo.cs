namespace Retro_ML.Configuration.FieldInformation
{
    public class IntegerFieldInfo : FieldInfo
    {
        /// <summary>
        /// Minimum possible value of this field
        /// </summary>
        public int MinimumValue { get; }
        /// <summary>
        /// Maximum possible value of this field
        /// </summary>
        public int MaximumValue { get; }
        /// <summary>
        /// The increment between values. Setting this to 0 gets rid of the increment arrows
        /// </summary>
        public int Increment { get; }

        public IntegerFieldInfo(string name, string readableName, int minimumValue, int maximumValue, int increment) : base(name, readableName)
        {
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            Increment = increment;
        }

        public IntegerFieldInfo(string name, string readableName, int minimumValue, int maximumValue, int increment, string tooltip) : base(name, readableName, tooltip)
        {
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            Increment = increment;
        }

    }
}
