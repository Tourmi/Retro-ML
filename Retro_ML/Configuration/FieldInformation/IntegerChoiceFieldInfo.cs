namespace Retro_ML.Configuration.FieldInformation
{
    public class IntegerChoiceFieldInfo : FieldInfo
    {
        public int[] PossibleValues { get; }

        public IntegerChoiceFieldInfo(string name, string readableName, int[] possibleValues) : base(name, readableName)
        {
            PossibleValues = possibleValues;
        }

        public IntegerChoiceFieldInfo(string name, string readableName, int[] possibleValues, string tooltip) : base(name, readableName, tooltip)
        {
            PossibleValues = possibleValues;
        }
    }
}
