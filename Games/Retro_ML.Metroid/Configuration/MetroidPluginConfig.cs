using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Metroid.Game;
using Retro_ML.Metroid.Neural.Scoring;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.Metroid.Configuration;

internal class MetroidPluginConfig : IGamePluginConfig
{
    private static readonly bool[] DEFAULT_ENABLED_STATES = new bool[]
    {
        true, //tiles
        true, //dangers
        true, //goodies
        true, //health
        true, //missiles
        true, //x speed
        true, //y speed
        true, //look direction
        true, //in morph ball
        true, //invincible
        true, //on elevator
        true, //using missiles
        true, //equipment
        true, //navigation
        false, //clock
        true, //bias

        true, //a
        true, //b
        true, //left
        true, //right
        true, //up
        true, //down
        false, //start
        true //select
    };

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new BoolFieldInfo(nameof(UseGrid), "Use Vision Grid"),
        new BoolFieldInfo(nameof(UseDirectionToGoodie), "Use Direction to goodie"),
        new IntegerFieldInfo(nameof(ViewDistance), "View distance", 1, 8, 1),
        new IntegerChoiceFieldInfo(nameof(Raycount), "Raycast count", new int[] { 4, 8, 16, 32, 64 }),
        new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1),
        new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 }),
        new IntegerFieldInfo(nameof(FrameSkip), "Frames to skip", 0, 15, 1)
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(UseGrid) => UseGrid,
            nameof(UseDirectionToGoodie) => UseDirectionToGoodie,
            nameof(ViewDistance) => ViewDistance,
            nameof(Raycount) => Raycount,
            nameof(InternalClockLength) => InternalClockLength,
            nameof(InternalClockTickLength) => InternalClockTickLength,
            nameof(FrameSkip) => FrameSkip,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(UseGrid): UseGrid = (bool)value; break;
                case nameof(UseDirectionToGoodie): UseDirectionToGoodie = (bool)value; break;
                case nameof(ViewDistance): ViewDistance = (int)value; break;
                case nameof(Raycount): Raycount = (int)value; break;
                case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                case nameof(FrameSkip): FrameSkip = (int)value; break;
            }
        }
    }

    /// <summary>
    /// Whether or not to use a grid for multiple inputs.
    /// </summary>
    public bool UseGrid { get; set; } = false;
    /// <summary>
    /// Whether or not to simply use the x,y direction from the nearest goodie, instead of rays or a grid
    /// </summary>
    public bool UseDirectionToGoodie { get; set; } = true;

    /// <summary>
    /// How many tiles ahead we can see
    /// </summary>
    public int ViewDistance { get; set; } = 8;
    /// <summary>
    /// The amount of rays to send out
    /// </summary>
    public int Raycount { get; set; } = 16;

    /// <summary>
    /// The amount of inputs for the internal clock.
    /// </summary>
    public int InternalClockLength { get; set; } = 4;
    /// <summary>
    /// The amount of frames before the clock moves to the next state.
    /// </summary>
    public int InternalClockTickLength { get; set; } = 15;
    /// <summary>
    /// Skips this amount of frames for every neural network updates.
    /// </summary>
    public int FrameSkip { get; set; } = 1;

    public List<IScoreFactor> ScoreFactors { get; set; }

    public MetroidPluginConfig() => ScoreFactors = new List<IScoreFactor>()
    {
        new DiedScoreFactor() { ScoreMultiplier = -10, IsDisabled = false },
        new TimeTakenScoreFactor() { ScoreMultiplier = -0.1, IsDisabled = false },
        new ObjectiveScoreFactor() { ScoreMultiplier = 100, IsDisabled = false },
        new ProgressScoreFactor() { ScoreMultiplier = 1, IsDisabled = false },
        new HealthScoreFactor() { ScoreMultiplier = 0.1, IsDisabled = true }
    };

    public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

    public void Deserialize(string json)
    {
        MetroidPluginConfig cfg = JsonConvert.DeserializeObject<MetroidPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

        ScoreFactors = cfg.ScoreFactors;

        UseGrid = cfg.UseGrid;
        UseDirectionToGoodie = cfg.UseDirectionToGoodie;
        ViewDistance = cfg.ViewDistance;
        Raycount = cfg.Raycount;
        InternalClockLength = cfg.InternalClockLength;
        InternalClockTickLength = cfg.InternalClockTickLength;
        FrameSkip = cfg.FrameSkip;
    }

    public void InitNeuralConfig(NeuralConfig neuralConfig)
    {
        int enabledIndex = 0;
        if (neuralConfig.EnabledStates.Length != DEFAULT_ENABLED_STATES.Length)
        {
            neuralConfig.EnabledStates = DEFAULT_ENABLED_STATES;
        }
        neuralConfig.InputNodes.Clear();
        if (UseGrid)
        {
            neuralConfig.InputNodes.Add(new InputNode("Solid Tiles",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSolidTilesAroundPosition(ViewDistance, ViewDistance), ViewDistance * 2 + 1, ViewDistance * 2 + 1));
            neuralConfig.InputNodes.Add(new InputNode("Dangers",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetDangerousTilesAroundPosition(ViewDistance, ViewDistance), ViewDistance * 2 + 1, ViewDistance * 2 + 1));
        }
        else
        {
            neuralConfig.InputNodes.Add(new InputNode("Solid Tiles",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => Raycast.GetRayDistances(((MetroidDataFetcher)dataFetcher).GetSolidTilesAroundPosition(ViewDistance, ViewDistance), ViewDistance, Raycount), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Dangers",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => Raycast.GetRayDistances(((MetroidDataFetcher)dataFetcher).GetDangerousTilesAroundPosition(ViewDistance, ViewDistance), ViewDistance, Raycount), Raycount / 4, 4));
        }

        neuralConfig.InputNodes.Add((UseDirectionToGoodie, UseGrid) switch
        {
            (true, _) => new InputNode("Goodies",
                                neuralConfig.EnabledStates[enabledIndex++],
                                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetDirectionToNearestGoodTile(), 2, 1),
            (false, true) => new InputNode("Goodies",
                                neuralConfig.EnabledStates[enabledIndex++],
                                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetGoodTilesAroundPosition(ViewDistance, ViewDistance), ViewDistance * 2 + 1, ViewDistance * 2 + 1),
            _ => new InputNode("Goodies",
                                neuralConfig.EnabledStates[enabledIndex++],
                                (dataFetcher) => Raycast.GetRayDistances(((MetroidDataFetcher)dataFetcher).GetGoodTilesAroundPosition(ViewDistance, ViewDistance), ViewDistance, Raycount), Raycount / 4, 4)
        });

        neuralConfig.InputNodes.Add(new InputNode("Health", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusHealthRatio()));
        neuralConfig.InputNodes.Add(new InputNode("Missiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetCurrentMissiles()));
        neuralConfig.InputNodes.Add(new InputNode("X Speed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusHorizontalSpeed()));
        neuralConfig.InputNodes.Add(new InputNode("Y Speed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusVerticalSpeed()));
        neuralConfig.InputNodes.Add(new InputNode("Look Direction", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).SamusLookDirection()));
        neuralConfig.InputNodes.Add(new InputNode("In Morph Ball", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusInMorphBall()));
        neuralConfig.InputNodes.Add(new InputNode("Invincible", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).SamusInvincibilityTimer()));
        neuralConfig.InputNodes.Add(new InputNode("On Elevator", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusOnElevator()));
        neuralConfig.InputNodes.Add(new InputNode("Using Missiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusUsingMissiles()));
        neuralConfig.InputNodes.Add(new InputNode("Equipment", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetEquipment(), 3, 3));
        neuralConfig.InputNodes.Add(new InputNode("Navigation", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetNavigationDirection(), 2, 2));
        neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
        neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

        neuralConfig.OutputNodes.Clear();
        neuralConfig.OutputNodes.Add(new OutputNode("A", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("B", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Left", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Right", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Up", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Down", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Start", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Select", neuralConfig.EnabledStates[enabledIndex++]));
    }
}
