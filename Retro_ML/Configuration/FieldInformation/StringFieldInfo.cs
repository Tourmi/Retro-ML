namespace Retro_ML.Configuration.FieldInformation
{
    public class StringFieldInfo : FieldInfo
    {
        public StringFieldInfo(string name, string readableName) : base(name, readableName) { }

        public StringFieldInfo(string name, string readableName, string tooltip) : base(name, readableName, tooltip) { }
    }
}
