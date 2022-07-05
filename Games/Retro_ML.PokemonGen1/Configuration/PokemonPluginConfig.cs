using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.PokemonGen1.Game;
using Retro_ML.PokemonGen1.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.PokemonGen1.Configuration;

internal class PokemonPluginConfig : IGamePluginConfig
{
    private static readonly bool[] DEFAULT_ENABLED_STATES = new bool[]
    {
        true, //Is curr. move super effective
        true, // Opp. paralyzed
        false, //clock
        true, //bias

        true, //Move1
        true, //Move2
        true, //Move3
        true, //Move4
    };

    public FieldInfo[] Fields => new FieldInfo[]
    {

    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(InternalClockLength) => InternalClockLength,
            nameof(InternalClockTickLength) => InternalClockTickLength,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
            }
        }
    }

    /// <summary>
    /// The amount of inputs for the internal clock.
    /// </summary>
    public int InternalClockLength { get; set; } = 4;
    /// <summary>
    /// The amount of frames before the clock moves to the next state.
    /// </summary>
    public int InternalClockTickLength { get; set; } = 15;

    public List<IScoreFactor> ScoreFactors { get; set; }

    public PokemonPluginConfig() => ScoreFactors = new List<IScoreFactor>()
            {
                new WonFightScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = 10
                }
        };

    public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

    public void Deserialize(string json)
    {
        PokemonPluginConfig cfg = JsonConvert.DeserializeObject<PokemonPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

        ScoreFactors = cfg.ScoreFactors;

        InternalClockLength = cfg.InternalClockLength;
        InternalClockTickLength = cfg.InternalClockTickLength;
    }

    public void InitNeuralConfig(NeuralConfig neuralConfig)
    {
        int enabledIndex = 0;
        if (neuralConfig.EnabledStates.Length != DEFAULT_ENABLED_STATES.Length)
            neuralConfig.EnabledStates = DEFAULT_ENABLED_STATES;
        neuralConfig.InputNodes.Clear();

        neuralConfig.InputNodes.Add(new InputNode("Is curr. move super effective", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).IsSuperEffective()));
        neuralConfig.InputNodes.Add(new InputNode("Opp. paralyzed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetOpposingPokemonParalysis()));
        neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
        neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

        neuralConfig.OutputNodes.Clear();
        neuralConfig.OutputNodes.Add(new OutputNode("Move1", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Move2", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Move3", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Move4", neuralConfig.EnabledStates[enabledIndex++]));
    }
}
