using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMario64.Game;
using Retro_ML.SuperMario64.Neural.Scoring;
using Retro_ML.Utils;
using Retro_ML.Utils.Game;

namespace Retro_ML.SuperMario64.Configuration;

internal class SM64PluginConfig : IGamePluginConfig
{
    private static readonly bool[] DEFAULT_ENABLED_STATES = new bool[]
    {
        true, //collision
        true, //dangers
        true, //goodies
        false, //clock
        true, //bias

        true, //a
        true, //b
        false, //c-left
        false, //c-right
        false, //c-up
        false, //c-down
        false, //dpad-left
        false, //dpad-right
        false, //dpad-up
        false, //dpad-down
        true, //z
        false, //left shoulder
        false, //right shoulder
        false, //start
        true, //joystick x
        true, //joystick y
    };

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new IntegerFieldInfo(nameof(ViewDistance), "View distance", 1, 10_000, 100, "Distance the AI can see around itself"),
        new IntegerChoiceFieldInfo(nameof(SolidVerticalRays), "Solid Vertical Rays", new int[] { 1, 3, 5, 7, 9, 11, 13, 15 }, "How many rows of rays to throw for solid tris"),
        new IntegerFieldInfo(nameof(SolidVerticalViewAngle), "Solid Vertical View Angle", 1, 180, 15, "Vertical angle at which the AI can see solid tris"),
        new IntegerChoiceFieldInfo(nameof(SolidHorizontalRays), "Solid Horizontal Rays", new int[] { 1, 3, 5, 7, 9, 11, 13, 15 }, "How many column of rays to throw for solid tris"),
        new IntegerFieldInfo(nameof(SolidHorizontalViewAngle), "Solid Horizontal View Angle", 1, 360, 15, "Horizontal angle at which the AI can see solid tris"),
        new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1, "Length of a single tick for the internal clock"),
        new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 }, "Amount of nodes for the internal clock"),
        new IntegerFieldInfo(nameof(FrameSkip), "Frames to skip", 0, 15, 1, "Amount of frames to skip for every AI evaluation"),
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(ViewDistance) => ViewDistance,
            nameof(SolidVerticalRays) => SolidVerticalRays,
            nameof(SolidVerticalViewAngle) => SolidVerticalViewAngle,
            nameof(SolidHorizontalRays) => SolidHorizontalRays,
            nameof(SolidHorizontalViewAngle) => SolidHorizontalViewAngle,
            nameof(InternalClockLength) => InternalClockLength,
            nameof(InternalClockTickLength) => InternalClockTickLength,
            nameof(FrameSkip) => FrameSkip,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(ViewDistance): ViewDistance = (int)value; break;
                case nameof(SolidVerticalRays): SolidVerticalRays = (int)value; break;
                case nameof(SolidVerticalViewAngle): SolidVerticalViewAngle = (int)value; break;
                case nameof(SolidHorizontalRays): SolidHorizontalRays = (int)value; break;
                case nameof(SolidHorizontalViewAngle): SolidHorizontalViewAngle = (int)value; break;
                case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                case nameof(FrameSkip): FrameSkip = (int)value; break;
            }
        }
    }

    /// <summary>
    /// How many tiles ahead we can see
    /// </summary>
    public int ViewDistance { get; set; } = 500;

    public int SolidVerticalRays { get; set; } = 5;
    public int SolidVerticalViewAngle { get; set; } = 165;
    public int SolidHorizontalRays { get; set; } = 5;
    public int SolidHorizontalViewAngle { get; set; } = 180;

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
    public int FrameSkip { get; set; } = 5;

    public List<IScoreFactor> ScoreFactors { get; set; }

    public SM64PluginConfig() => ScoreFactors = new List<IScoreFactor>()
    {
        new TimeTakenScoreFactor() { ScoreMultiplier = -0.1, IsDisabled = false },
    };

    public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

    public void Deserialize(string json)
    {
        SM64PluginConfig cfg = JsonConvert.DeserializeObject<SM64PluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

        ScoreFactors = cfg.ScoreFactors;

        ViewDistance = cfg.ViewDistance;
        SolidVerticalRays = cfg.SolidVerticalRays;
        SolidVerticalViewAngle = cfg.SolidVerticalViewAngle;
        SolidHorizontalRays = cfg.SolidHorizontalRays;
        SolidHorizontalViewAngle = cfg.SolidHorizontalViewAngle;
        InternalClockLength = cfg.InternalClockLength;
        InternalClockTickLength = cfg.InternalClockTickLength;
        FrameSkip = cfg.FrameSkip;
    }

    public void InitNeuralConfig(NeuralConfig neuralConfig)
    {
        int enabledIndex = 0;
        if (neuralConfig.EnabledStates.Length != DEFAULT_ENABLED_STATES.Length)
            neuralConfig.EnabledStates = DEFAULT_ENABLED_STATES;
        neuralConfig.InputNodes.Clear();

        neuralConfig.InputNodes.Add(new InputNode(
            "Solid collision",
            neuralConfig.EnabledStates[enabledIndex++],
            (dataFetcher) => Raycast3D.GetRayDistances(SolidVerticalRays,
                                                       SolidHorizontalRays,
                                                       SolidVerticalViewAngle / 360f,
                                                       SolidHorizontalViewAngle / 360f,
                                                       ViewDistance,
                                                       ((SM64DataFetcher)dataFetcher).GetMarioForwardRay(),
                                                       ((SM64DataFetcher)dataFetcher).GetCollision()),
            SolidHorizontalRays,
            SolidVerticalRays));
        neuralConfig.InputNodes.Add(new InputNode(
            "Dangers",
            neuralConfig.EnabledStates[enabledIndex++],
            (dataFetcher) => Raycast3D.GetRayDistances(SolidVerticalRays,
                                                       SolidHorizontalRays,
                                                       SolidVerticalViewAngle / 360f,
                                                       SolidHorizontalViewAngle / 360f,
                                                       ViewDistance,
                                                       ((SM64DataFetcher)dataFetcher).GetMarioForwardRay(),
                                                       ((SM64DataFetcher)dataFetcher).GetEnemyHitboxes()),
            SolidHorizontalRays,
            SolidVerticalRays));
        neuralConfig.InputNodes.Add(new InputNode(
            "Goodies",
            neuralConfig.EnabledStates[enabledIndex++],
            (dataFetcher) => Raycast3D.GetRayDistances(SolidVerticalRays,
                                                       SolidHorizontalRays,
                                                       SolidVerticalViewAngle / 360f,
                                                       SolidHorizontalViewAngle / 360f,
                                                       ViewDistance,
                                                       ((SM64DataFetcher)dataFetcher).GetMarioForwardRay(),
                                                       ((SM64DataFetcher)dataFetcher).GetGoodieHitboxes()),
            SolidHorizontalRays,
            SolidVerticalRays));
        neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SM64DataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
        neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

        neuralConfig.OutputNodes.Clear();
        neuralConfig.OutputNodes.Add(new OutputNode("A", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("B", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("C-Left", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("C-Right", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("C-Up", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("C-Down", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Dpad Left", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Dpad Right", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Dpad Up", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Dpad Down", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Z", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Left Shoulder", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Right Shoulder", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Start", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Joystick X", neuralConfig.EnabledStates[enabledIndex++]));
        neuralConfig.OutputNodes.Add(new OutputNode("Joystick Y", neuralConfig.EnabledStates[enabledIndex++]));
    }
}
