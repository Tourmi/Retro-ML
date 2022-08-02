using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Metroid.Game;
using Retro_ML.Metroid.Neural.Scoring;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;
using Retro_ML.Utils.Game;

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
        true, //grounded
        true, //in morph ball
        true, //invincible
        true, //pass under
        true, //door
        true, //missile door
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
        new BoolFieldInfo(nameof(UseGrid), "Use Vision Grid", "Whether or not to use the Vision Grid instead of a Raycast"),
        new BoolFieldInfo(nameof(DoublePrecision), "Double precision", "Doubles the precision of tiles, sprites, etc. being fed to the AI by using 8x8 tiles instead of 16x16 tiles. NOT RECOMMENDED IF USING VISION GRID"),
        new BoolFieldInfo(nameof(UseDirectionToGoodie), "Use Direction to goodie", "Whether to just give the direction to the nearest pickup/powerup (2 nodes), or go with the Raycast/Vision grid"),
        new IntegerFieldInfo(nameof(ViewDistance), "View distance", 1, 8, 1, "Distance the AI can see around it"),
        new IntegerChoiceFieldInfo(nameof(Raycount), "Raycast count", new int[] { 4, 8, 16, 32, 64 }, "Amount of vision rays to cast around Samus"),
        new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1, "Length of a single tick for the internal clock"),
        new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 }, "Amount of nodes for the internal clock"),
        new IntegerFieldInfo(nameof(FrameSkip), "Frames to skip", 0, 15, 1, "Amount of frames to skip for every AI evaluation"),

        new BoolFieldInfo(nameof(ShowBombs), "Bombs in Equipment", "Allows the AI to see if it has bombs"),
        new BoolFieldInfo(nameof(ShowHighJump), "High Jump in Equipment", "Allows the AI to see if it has the high jump"),
        new BoolFieldInfo(nameof(ShowLongBeam), "Long Beam in Equipment", "Allows the AI to see if it has the long beam"),
        new BoolFieldInfo(nameof(ShowScrew), "Screw Attack in Equipment", "Allows the AI to see if it has the screw attack"),
        new BoolFieldInfo(nameof(ShowMorphBall), "Morph Ball (Maru Mari) in Equipment", "Allows the AI to see if it has the morph ball (maru mari)"),
        new BoolFieldInfo(nameof(ShowVaria), "Varia Suit in Equipment", "Allows the AI to see if it has the varia suit"),
        new BoolFieldInfo(nameof(ShowWaveBeam), "Wave Beam in Equipment", "Allows the AI to see if it has the wave beam"),
        new BoolFieldInfo(nameof(ShowIceBeam), "Ice Beam in Equipment", "Allows the AI to see if it has the ice beam"),
        new BoolFieldInfo(nameof(ShowMissiles), "Missiles in Equipment", "Allows the AI to see if it gathered at least one missile upgrade"),
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(UseGrid) => UseGrid,
            nameof(DoublePrecision) => DoublePrecision,
            nameof(UseDirectionToGoodie) => UseDirectionToGoodie,
            nameof(ViewDistance) => ViewDistance,
            nameof(Raycount) => Raycount,
            nameof(InternalClockLength) => InternalClockLength,
            nameof(InternalClockTickLength) => InternalClockTickLength,
            nameof(FrameSkip) => FrameSkip,
            nameof(ShowBombs) => ShowBombs,
            nameof(ShowHighJump) => ShowHighJump,
            nameof(ShowIceBeam) => ShowIceBeam,
            nameof(ShowLongBeam) => ShowLongBeam,
            nameof(ShowMissiles) => ShowMissiles,
            nameof(ShowMorphBall) => ShowMorphBall,
            nameof(ShowScrew) => ShowScrew,
            nameof(ShowVaria) => ShowVaria,
            nameof(ShowWaveBeam) => ShowWaveBeam,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(UseGrid): UseGrid = (bool)value; break;
                case nameof(DoublePrecision): DoublePrecision = (bool)value; break;
                case nameof(UseDirectionToGoodie): UseDirectionToGoodie = (bool)value; break;
                case nameof(ViewDistance): ViewDistance = (int)value; break;
                case nameof(Raycount): Raycount = (int)value; break;
                case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                case nameof(FrameSkip): FrameSkip = (int)value; break;
                case nameof(ShowBombs): ShowBombs = (bool)value; break;
                case nameof(ShowHighJump): ShowHighJump = (bool)value; break;
                case nameof(ShowIceBeam): ShowIceBeam = (bool)value; break;
                case nameof(ShowLongBeam): ShowLongBeam = (bool)value; break;
                case nameof(ShowMissiles): ShowMissiles = (bool)value; break;
                case nameof(ShowMorphBall): ShowMorphBall = (bool)value; break;
                case nameof(ShowScrew): ShowScrew = (bool)value; break;
                case nameof(ShowVaria): ShowVaria = (bool)value; break;
                case nameof(ShowWaveBeam): ShowWaveBeam = (bool)value; break;
            }
        }
    }

    /// <summary>
    /// Whether or not to use a grid for multiple inputs.
    /// </summary>
    public bool UseGrid { get; set; } = false;
    /// <summary>
    /// Doubles the precision of tiles, sprites, etc. being fed to the AI by using 8x8 tiles instead of 16x16 tiles.
    /// </summary>
    public bool DoublePrecision { get; set; } = true;
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
    /// <summary>
    /// Whether or not the AI knows if it has the morph ball in its equipment 
    /// </summary>
    public bool ShowMorphBall { get; set; } = true;
    /// <summary>
    /// Whether or not the AI knows if it has missiles in its equipment 
    /// </summary>
    public bool ShowMissiles { get; set; } = false;
    /// <summary>
    /// Whether or not the AI knows if it has the screw attack in its equipment 
    /// </summary>
    public bool ShowScrew { get; set; } = true;
    /// <summary>
    /// Whether or not the AI knows if it has the high jump in its equipment 
    /// </summary>
    public bool ShowHighJump { get; set; } = true;
    /// <summary>
    /// Whether or not the AI knows if it has bombs in its equipment 
    /// </summary>
    public bool ShowBombs { get; set; } = true;
    /// <summary>
    /// Whether or not the AI knows if it has the varia suit in its equipment 
    /// </summary>
    public bool ShowVaria { get; set; } = false;
    /// <summary>
    /// Whether or not the AI knows if it has the ice beam in its equipment 
    /// </summary>
    public bool ShowIceBeam { get; set; } = true;
    /// <summary>
    /// Whether or not the AI knows if it has the wave beam in its equipment 
    /// </summary>
    public bool ShowWaveBeam { get; set; } = false;
    /// <summary>
    /// Whether or not the AI knows if it has the long beam in its equipment 
    /// </summary>
    public bool ShowLongBeam { get; set; } = false;

    public List<IScoreFactor> ScoreFactors { get; set; }
    /// <summary>
    /// Whether or not each equipment is shown to the AI. In order:
    /// <code>
    /// Bombs
    /// High Jump
    /// Long Beam
    /// Screw Attack
    /// Morph Ball
    /// Varia Suit
    /// Wave Beam
    /// Ice Beam
    /// Missiles
    /// </code>
    /// </summary>
    [JsonIgnore]
    public bool[] EquipmentStatus => new bool[] { ShowBombs, ShowHighJump, ShowLongBeam, ShowScrew, ShowMorphBall, ShowVaria, ShowWaveBeam, ShowIceBeam, ShowMissiles };
    /// <summary>
    /// The current amount of equipment that's shown to the AI.
    /// </summary>
    [JsonIgnore]
    public int EquipmentCount => EquipmentStatus.Count(equip => equip);

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
        DoublePrecision = cfg.DoublePrecision;
        UseDirectionToGoodie = cfg.UseDirectionToGoodie;
        ViewDistance = cfg.ViewDistance;
        Raycount = cfg.Raycount;
        InternalClockLength = cfg.InternalClockLength;
        InternalClockTickLength = cfg.InternalClockTickLength;
        FrameSkip = cfg.FrameSkip;

        ShowBombs = cfg.ShowBombs;
        ShowHighJump = cfg.ShowHighJump;
        ShowIceBeam = cfg.ShowIceBeam;
        ShowLongBeam = cfg.ShowLongBeam;
        ShowMissiles = cfg.ShowMissiles;
        ShowMorphBall = cfg.ShowMorphBall;
        ShowScrew = cfg.ShowScrew;
        ShowVaria = cfg.ShowVaria;
        ShowWaveBeam = cfg.ShowWaveBeam;
    }

    public void InitNeuralConfig(NeuralConfig neuralConfig)
    {
        int enabledIndex = 0;
        if (neuralConfig.EnabledStates.Length != DEFAULT_ENABLED_STATES.Length)
        {
            neuralConfig.EnabledStates = DEFAULT_ENABLED_STATES;
        }
        neuralConfig.InputNodes.Clear();

        var viewDistance = ViewDistance * (DoublePrecision ? 2 : 1);
        if (UseGrid)
        {
            neuralConfig.InputNodes.Add(new InputNode("Solid Tiles",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSolidTilesAroundPosition(), viewDistance * 2 + 1, viewDistance * 2 + 1));
            neuralConfig.InputNodes.Add(new InputNode("Dangers",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetDangerousTilesAroundPosition(), viewDistance * 2 + 1, viewDistance * 2 + 1));
        }
        else
        {
            neuralConfig.InputNodes.Add(new InputNode("Solid Tiles",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => Raycast.GetRayDistances(((MetroidDataFetcher)dataFetcher).GetSolidTilesAroundPosition(), viewDistance, Raycount), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Dangers",
                neuralConfig.EnabledStates[enabledIndex++],
                (dataFetcher) => Raycast.GetRayDistances(((MetroidDataFetcher)dataFetcher).GetDangerousTilesAroundPosition(), viewDistance, Raycount), Raycount / 4, 4));
        }

        neuralConfig.InputNodes.Add((UseDirectionToGoodie, UseGrid) switch
        {
            (true, _) => new InputNode("Goodies",
                                neuralConfig.EnabledStates[enabledIndex++],
                                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetDirectionToNearestGoodTile(), 2, 1),
            (false, true) => new InputNode("Goodies",
                                neuralConfig.EnabledStates[enabledIndex++],
                                (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetGoodTilesAroundPosition(), viewDistance * 2 + 1, viewDistance * 2 + 1),
            _ => new InputNode("Goodies",
                                neuralConfig.EnabledStates[enabledIndex++],
                                (dataFetcher) => Raycast.GetRayDistances(((MetroidDataFetcher)dataFetcher).GetGoodTilesAroundPosition(), viewDistance, Raycount), Raycount / 4, 4)
        });

        neuralConfig.InputNodes.Add(new InputNode("Health", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusHealthRatio()));
        neuralConfig.InputNodes.Add(new InputNode("Missiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetCurrentMissileRatio()));
        neuralConfig.InputNodes.Add(new InputNode("X Speed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusXSpeedRatio()));
        neuralConfig.InputNodes.Add(new InputNode("Y Speed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusYSpeedRatio()));
        neuralConfig.InputNodes.Add(new InputNode("Look Direction", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).SamusLookDirection()));
        neuralConfig.InputNodes.Add(new InputNode("Grounded", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusGrounded()));
        neuralConfig.InputNodes.Add(new InputNode("In Morph Ball", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusInMorphBall()));
        neuralConfig.InputNodes.Add(new InputNode("Invincible", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetSamusInvincibilityTimerRatio()));
        neuralConfig.InputNodes.Add(new InputNode("Can pass under", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).CanPassUnder()));
        neuralConfig.InputNodes.Add(new InputNode("Door", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsInFrontOfDoor()));
        neuralConfig.InputNodes.Add(new InputNode("Missile Door", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsInFrontOfMissileDoor()));
        neuralConfig.InputNodes.Add(new InputNode("On Elevator", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusOnElevator()));
        neuralConfig.InputNodes.Add(new InputNode("Using Missiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).IsSamusUsingMissiles()));
        neuralConfig.InputNodes.Add(new InputNode("Equipment", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((MetroidDataFetcher)dataFetcher).GetEquipment(), EquipmentCount, 1));
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
