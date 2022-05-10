using Newtonsoft.Json;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.Configuration
{
    public interface IPluginConfig
    {
        /// <summary>
        /// The available fields for this configuration.
        /// </summary>
        [JsonIgnore]
        FieldInfo[] Fields { get; }
        /// <summary>
        /// Returns or sets the value of the given field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        object this[string fieldName] { get; set; }

        /// <summary>
        /// Serializes this plugin's configuration
        /// </summary>
        /// <returns></returns>
        string Serialize();
        /// <summary>
        /// Deserializes this plugin's configuration, and sets all of its fields
        /// </summary>
        /// <param name="json"></param>
        void Deserialize(string json);
    }
}
