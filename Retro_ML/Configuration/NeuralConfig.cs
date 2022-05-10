using Newtonsoft.Json;
using Retro_ML.Neural;
using Retro_ML.Utils;

namespace Retro_ML.Configuration
{
    /// <summary>
    /// Configuration of the neural network. Modifying these values makes previous neural network incompatible with the new settings.
    /// </summary>
    public class NeuralConfig
    {
        /// <summary>
        /// The input nodes to use by the neural network
        /// </summary>
        [JsonIgnore]
        public List<InputNode> InputNodes { get; protected set; }
        /// <summary>
        /// The output nodes used by the neural network. 
        /// </summary>
        [JsonIgnore]
        public List<OutputNode> OutputNodes { get; protected set; }

        /// <summary>
        /// Stores whether or not an input or output node is enabled, based on the index of them.
        /// </summary>
        public bool[] EnabledStates { get; set; }

        public NeuralConfig()
        {
            InputNodes = new List<InputNode>();
            OutputNodes = new List<OutputNode>();
            EnabledStates = new bool[0];
        }

        /// <summary>
        /// Returns the total amount of enabled input nodes, including all of the nodes of inputs that use a grid.
        /// </summary>
        /// <returns></returns>
        public int GetInputCount()
        {
            int count = 0;

            foreach (var input in InputNodes)
            {
                if (input.ShouldUse) count += input.TotalWidth * input.TotalHeight;
            }

            return count;
        }

        /// <summary>
        /// Returns the total amount of enabled output nodes.
        /// </summary>
        /// <returns></returns>
        public int GetOutputCount()
        {
            int count = 0;
            foreach (var output in OutputNodes)
            {
                if (output.ShouldUse) count++;
            }

            return count;
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public static NeuralConfig Deserialize(string json)
        {
            NeuralConfig cfg = JsonConvert.DeserializeObject<NeuralConfig>(json, SerializationUtils.JSON_CONFIG)!;

            return cfg;
        }
    }
}
