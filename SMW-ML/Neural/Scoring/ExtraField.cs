using System.Linq;

namespace SMW_ML.Neural.Scoring
{
    public class ExtraField
    {
        public string Name { get; }
        public double Value { get; set; }

        public ExtraField(string name, double value)
        {
            Name = name;
            Value = value;
        }

        public static double GetValue(ExtraField[] extraFields, string name) => extraFields.Single(ex => ex.Name == name).Value;
    }
}
