namespace Retro_ML.Configuration.FieldInformation
{
    public class DoubleFieldInfo : FieldInfo
    {
        /// <summary>
        /// Minimum possible value of this field
        /// </summary>
        public double MinimumValue { get; }
        /// <summary>
        /// Maximum possible value of this field
        /// </summary>
        public double MaximumValue { get; }
        /// <summary>
        /// The increment between values. Setting this to 0 gets rid of the increment arrows
        /// </summary>
        public double Increment { get; }
        
        public DoubleFieldInfo(string name, string readableName, double minimumValue, double maximumValue, double increment) : base(name, readableName)
        {
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            Increment = increment;
        }

        public DoubleFieldInfo(string name, string readableName, double minimumValue, double maximumValue, double increment, string tooltip) : base(name, readableName, tooltip)
        {
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            Increment = increment;
        }
    }
}
