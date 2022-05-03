using System.Linq;

namespace Retro_ML.Neural.Scoring
{
    /// <summary>
    /// An extra field is used to add additional configurable parameters to Score Factors.
    /// </summary>
    public class ExtraField
    {
        /// <summary>
        /// Unique name of the extra field
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Value of the extra field. Usually a multiplier
        /// </summary>
        public double Value { get; set; }

        public ExtraField(string name, double value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Returns the value of the specified extra field in the given extrafield array.
        /// </summary>
        /// <param name="extraFields"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static double GetValue(ExtraField[] extraFields, string name) => extraFields.Single(ex => ex.Name == name).Value;
    }
}
