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
        true, //curr. move super effective
        true, //curr. move not very super effective
        true, //move power
        true, //stab
        true, //Opp Hp.
        true, //Ennemy Sleeping
        true, // Opp. paralyzed
        true, //Frozen
        true, //Burned
        true, //Poisoned
        false, //clock
        true, //bias

        true, //current move score
    };

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new IntegerFieldInfo(nameof(NbFights), "Number of Fights", 1, 50, 1, "The number of fights the AI will do for each save states selected")
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(NbFights) => NbFights,
            nameof(InternalClockLength) => InternalClockLength,
            nameof(InternalClockTickLength) => InternalClockTickLength,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(NbFights): NbFights = (int)value; break;
                case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
            }
        }
    }

    public int NbFights { get; set; } = 1;

    /// <summary>
    /// The amount of inputs for the internal clock.
    /// </summary>
    public int InternalClockLength { get; set; } = 4;
    /// <summary>
    /// The amount of frames before the clock moves to the next state.
    /// </summary>
    public int InternalClockTickLength { get; set; } = 15;

    public List<IScoreFactor> ScoreFactors { get; set; }

    public PokemonPluginConfig()
    {
        ScoreFactors = new List<IScoreFactor>()
            {
                new WonFightScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = 10
                },
                new LostFightScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = -5
                }
        };
    }

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

        neuralConfig.InputNodes.Add(new InputNode("Move super effective", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).IsSuperEffective()));
        neuralConfig.InputNodes.Add(new InputNode("Move not very effective", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).IsNotVeryEffective()));
        neuralConfig.InputNodes.Add(new InputNode("Move Power", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).SelectedMovePower()));
        neuralConfig.InputNodes.Add(new InputNode("STAB", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).IsSTAB()));
        neuralConfig.InputNodes.Add(new InputNode("Opponent HP", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).OpposingCurrentHP()));
        neuralConfig.InputNodes.Add(new InputNode("Ennemy sleeping", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetOpposingPokemonSleep()));
        neuralConfig.InputNodes.Add(new InputNode("Opponnent paralyzed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetOpposingPokemonParalysis()));
        neuralConfig.InputNodes.Add(new InputNode("Opponnent Frozen", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetOpposingPokemonFrozen()));
        neuralConfig.InputNodes.Add(new InputNode("Opponnent Burned", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetOpposingPokemonBurned()));
        neuralConfig.InputNodes.Add(new InputNode("Opponnent Poisoned", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetOpposingPokemonPoisoned()));
        neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((PokemonDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
        neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

        neuralConfig.OutputNodes.Clear();
        neuralConfig.OutputNodes.Add(new OutputNode("Current Move Score", neuralConfig.EnabledStates[enabledIndex++]));

    }
}
