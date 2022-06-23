namespace Retro_ML.Configuration.FieldInformation
{
    public abstract class FieldInfo
    {
        /// <summary>
        /// Name of this field
        /// </summary>
        public string Name { get; }
        public string ReadableName { get; }
        public string? Tooltip { get; }

        public FieldInfo(string name, string readableName): this(name, readableName, null) { }

        public FieldInfo(string name, string readableName, string? tooltip)
        {
            Name = name;
            ReadableName = readableName;
            Tooltip = tooltip;
        }
    }
}
