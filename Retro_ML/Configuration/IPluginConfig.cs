using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Configuration
{
    public interface IPluginConfig
    {
        /// <summary>
        /// The available fields for this configuration, as well as their type.
        /// </summary>
        [JsonIgnore]
        (string fieldName, Type type)[] Fields { get; }
        /// <summary>
        /// Returns or sets the value of the given field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        object this[string fieldName] { get; set; }

        /// <summary>
        /// Initializes the neural config
        /// </summary>
        /// <param name="neuralConfig"></param>
        void InitNeuralConfig(NeuralConfig neuralConfig);

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
