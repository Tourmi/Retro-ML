namespace Retro_ML.Configuration.FieldInformation
{
    public class IntegerChoiceFieldInfo : FieldInfo
    {
        public int[] PossibleValues { get; }

        public IntegerChoiceFieldInfo(string name, string readableName, int[] possibleValues) : base(name, readableName)
        {
            PossibleValues = possibleValues;
        }
    }
}
