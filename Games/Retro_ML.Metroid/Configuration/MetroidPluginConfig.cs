using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Metroid.Game;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.Metroid.Configuration
{
    internal class MetroidPluginConfig : IGamePluginConfig
    {
        public FieldInfo[] Fields => new FieldInfo[]
        {
            new BoolFieldInfo(nameof(UseGrid), "Use Vision Grid"),
            new IntegerFieldInfo(nameof(GridDistanceX), "Vision Grid Horizontal Distance", 1, 10, 1),
            new IntegerFieldInfo(nameof (GridDistanceY), "Vision Grid Vertical Distance", 1, 10, 1),
            new IntegerFieldInfo(nameof(RayDistance), "Vision Ray Distance", 1, 32, 1),
            new IntegerChoiceFieldInfo(nameof(Raycount), "Raycast count", new int[] { 4, 8, 16, 32, 64 }),
            new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1),
            new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 }),
            new IntegerFieldInfo(nameof(FrameSkip), "Frames to skip", 0, 15, 1)
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(UseGrid) => UseGrid,
                    nameof(GridDistanceX) => GridDistanceX,
                    nameof(GridDistanceY) => GridDistanceY,
                    nameof(RayDistance) => RayDistance,
                    nameof(Raycount) => Raycount,
                    nameof(InternalClockLength) => InternalClockLength,
                    nameof(InternalClockTickLength) => InternalClockTickLength,
                    nameof(FrameSkip) => FrameSkip,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(UseGrid): UseGrid = (bool)value; break;
                    case nameof(GridDistanceX): GridDistanceX = (int)value; break;
                    case nameof(GridDistanceY): GridDistanceY = (int)value; break;
                    case nameof(RayDistance): RayDistance = (int)value; break;
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
        /// For any inputs that use a grid, the x distance it should be.
        /// </summary>
        public int GridDistanceX { get; set; } = 4;
        /// <summary>
        /// For any inputs that use a grid, the y distance it should be.
        /// </summary>
        public int GridDistanceY { get; set; } = 4;

        /// <summary>
        /// How many tiles ahead we can see
        /// </summary>
        public int RayDistance { get; set; } = 8;
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
        public int FrameSkip { get; set; } = 0;

        public List<IScoreFactor> ScoreFactors { get; set; }

        public MetroidPluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                new TimeTakenScoreFactor() { ScoreMultiplier = -1, IsDisabled = false }
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            MetroidPluginConfig cfg = JsonConvert.DeserializeObject<MetroidPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

            ScoreFactors = cfg.ScoreFactors;

            UseGrid = cfg.UseGrid;
            GridDistanceX = cfg.GridDistanceX;
            GridDistanceY = cfg.GridDistanceY;
            RayDistance = cfg.RayDistance;
            Raycount = cfg.Raycount;
            InternalClockLength = cfg.InternalClockLength;
            InternalClockTickLength = cfg.InternalClockTickLength;
            FrameSkip = cfg.FrameSkip;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {
            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 2 + 8)
            {
                neuralConfig.EnabledStates = new bool[]
                {
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
            }
            neuralConfig.InputNodes.Clear();
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
}
