using Newtonsoft.Json;
using Retro_ML.Neural;
using Retro_ML.Utils;

namespace Retro_ML.Configuration;

/// <summary>
/// Configuration of the neural network. Modifying these values makes previous neural network incompatible with the new settings.
/// </summary>
public class NeuralConfig
{
    public int ShortTermMemoryNodeCount { get; set; } = 2;
    public int LongTermMemoryNodeCount { get; set; } = 3;
    public int PermanentMemoryNodeCount { get; set; } = 4;
    public double MaximumMemoryNodeValue { get; set; } = 1.0;

    /// <summary>
    /// The input nodes to use by the neural network
    /// </summary>
    [JsonIgnore]
    public List<InputNode> InputNodes { get; protected set; }
    /// <summary>
    /// The input nodes to use by the neural network, including the memory nodes
    /// </summary>
    [JsonIgnore]
    public List<InputNode> AllInputNodes => InputNodes.Concat(InputMemoryNodes).ToList();
    /// <summary>
    /// The output nodes used by the neural network. 
    /// </summary>
    [JsonIgnore]
    public List<OutputNode> OutputNodes { get; protected set; }
    /// <summary>
    /// The input nodes used by the neural network, including the memory nodes
    /// </summary>
    [JsonIgnore]
    public List<OutputNode> AllOutputNodes => OutputNodes.Concat(OutputMemoryNodes).ToList();

    /// <summary>
    /// Stores whether or not an input or output node is enabled, based on the index of them.
    /// </summary>
    public bool[] EnabledStates { get; set; }

    private readonly List<InputNode> InputMemoryNodes;
    private readonly List<OutputNode> OutputMemoryNodes;

    public NeuralConfig()
    {
        InputNodes = new();
        OutputNodes = new();
        EnabledStates = Array.Empty<bool>();
        InputMemoryNodes = new();
        OutputMemoryNodes = new();

        InitMemoryNodes();
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

        count += ShortTermMemoryNodeCount;
        count += LongTermMemoryNodeCount;
        count += PermanentMemoryNodeCount;

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

        count += ShortTermMemoryNodeCount;
        count += LongTermMemoryNodeCount * 2;
        count += PermanentMemoryNodeCount * 2;

        return count;
    }

    private void InitMemoryNodes()
    {
        OutputMemoryNodes.Add(new OutputNode("Short Term Memory", ShortTermMemoryNodeCount, 1, usesActivationThreshold: false));
        OutputMemoryNodes.Add(new OutputNode("Long Term Memory", LongTermMemoryNodeCount, 2, usesActivationThreshold: true, isHalfActivationThreshold: true));
        OutputMemoryNodes.Add(new OutputNode("Permanent Memory", PermanentMemoryNodeCount, 2, usesActivationThreshold: true, isHalfActivationThreshold: true));
        InputMemoryNodes.Add(new InputNode("Short Term Memory", ShortTermMemoryNodeCount > 0, ShortTermMemoryNodeCount, 1));
        InputMemoryNodes.Add(new InputNode("Long Term Memory", LongTermMemoryNodeCount > 0, LongTermMemoryNodeCount, 1));
        InputMemoryNodes.Add(new InputNode("Permanent Memory", PermanentMemoryNodeCount > 0, PermanentMemoryNodeCount, 1));
    }

    public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

    public static NeuralConfig Deserialize(string json)
    {
        NeuralConfig cfg = JsonConvert.DeserializeObject<NeuralConfig>(json, SerializationUtils.JSON_CONFIG)!;
        cfg.InitMemoryNodes();
        return cfg;
    }
}
