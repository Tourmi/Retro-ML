namespace Retro_ML.Configuration.FieldInformation
{
    public abstract class FieldInfo
    {
        /// <summary>
        /// Name of this field
        /// </summary>
        public string Name { get; }
        public string ReadableName { get; }


        public FieldInfo(string name, string readableName)
        {
            Name = name;
            ReadableName = readableName;
        }
    }
}
