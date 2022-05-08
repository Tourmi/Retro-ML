using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.SuperMarioWorld.Game;
using Retro_ML.Utils;

namespace Retro_ML.SuperMarioWorld.Configuration
{
    internal class SMWPluginConfig : IGamePluginConfig
    {
        public FieldInfo[] Fields => new FieldInfo[]
        {
            new BoolFieldInfo(nameof(UseGrid), "Use Vision Grid"),
            new IntegerFieldInfo(nameof(GridDistanceX), "Vision Grid Horizontal Distance", 1, 10, 1),
            new IntegerFieldInfo(nameof (GridDistanceY), "Vision Grid Vertical Distance", 1, 10, 1),
            new IntegerFieldInfo(nameof(RayLength), "Vision Ray Distance", 1, 10, 1),
            new IntegerChoiceFieldInfo(nameof(RayCount), "Vision Ray Count", new int[] { 4, 8, 16, 32, 64 }),
            new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1),
            new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 })
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
                    nameof(RayLength) => RayLength,
                    nameof(RayCount) => RayCount,
                    nameof(InternalClockLength) => InternalClockLength,
                    nameof(InternalClockTickLength) => InternalClockTickLength,
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
                    case nameof(RayLength): RayLength = (int)value; break;
                    case nameof(RayCount): RayCount = (int)value; break;
                    case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                    case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                }
            }
        }
        /// <summary>
        /// Whether or not to use a grid for multiple inputs.
        /// </summary>
        public bool UseGrid { get; set; } = true;
        /// <summary>
        /// For any inputs that use a grid, the x distance it should be.
        /// </summary>
        public int GridDistanceX { get; set; } = 4;
        /// <summary>
        /// For any inputs that use a grid, the y distance it should be.
        /// </summary>
        public int GridDistanceY { get; set; } = 4;

        /// <summary>
        /// Whether or not to use vision rays.
        /// </summary>
        [JsonIgnore]
        public bool UseRays => !UseGrid;
        /// <summary>
        /// For inputs that use vision rays, the maximum distance the ray can detect, in tiles.
        /// </summary>
        public int RayLength { get; set; } = 4;
        /// <summary>
        /// The amount of directions to throw rays towards. Should always be a power of 2 bigger or equal to 4.
        /// </summary>
        public int RayCount { get; set; } = 16;

        /// <summary>
        /// The amount of inputs for the internal clock.
        /// </summary>
        public int InternalClockLength { get; set; } = 4;
        /// <summary>
        /// The amount of frames before the clock moves to the next state.
        /// </summary>
        public int InternalClockTickLength { get; set; } = 2;

        /// <summary>
        /// For any inputs that use a grid, the total width of the grid based on the X distance.
        /// </summary>
        [JsonIgnore]
        public int GridWidth => GridDistanceX * 2 + 1;
        /// <summary>
        /// For any inputs that use a grid, the total height of the grid based on the Y distances.
        /// </summary>
        [JsonIgnore]
        public int GridHeight => GridDistanceY * 2 + 1;

        /// <summary>
        /// The width of the ray inputs. 
        /// </summary>
        [JsonIgnore]
        public int RaysWidth => RayCount / 4;
        /// <summary>
        /// The height of the ray inputs
        /// </summary>
        [JsonIgnore]
        public int RaysHeight => 4;

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            SMWPluginConfig cfg = JsonConvert.DeserializeObject<SMWPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

            UseGrid = cfg.UseGrid;
            GridDistanceX = cfg.GridDistanceX;
            GridDistanceY = cfg.GridDistanceY;
            RayLength = cfg.RayLength;
            RayCount = cfg.RayCount;
            InternalClockLength = cfg.InternalClockLength;
            InternalClockTickLength = cfg.InternalClockTickLength;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {

            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 27)
            {
                neuralConfig.EnabledStates = Enumerable.Repeat(true, 15 + 8).Concat(Enumerable.Repeat(false, 4)).ToArray();
            }
            neuralConfig.InputNodes.Clear();

            Func<SMWDataFetcher, int, int, bool[,]> tilesFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetWalkableTilesAroundPosition(gridDistX, gridDistY);
            Func<SMWDataFetcher, int, int, bool[,]> dangersFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetDangerousTilesAroundPosition(gridDistX, gridDistY);
            Func<SMWDataFetcher, int, int, bool[,]> goodiesFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetGoodTilesAroundPosition(gridDistX, gridDistY);
            Func<SMWDataFetcher, int, int, bool[,]> waterFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetWaterTilesAroundPosition(gridDistX, gridDistY);

            if (UseGrid)
            {
                neuralConfig.InputNodes.Add(new InputNode("Tiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => tilesFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                neuralConfig.InputNodes.Add(new InputNode("Dangers", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => dangersFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                neuralConfig.InputNodes.Add(new InputNode("Goodies", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => goodiesFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                neuralConfig.InputNodes.Add(new InputNode("Water", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => waterFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
            }
            else
            {
                neuralConfig.InputNodes.Add(new InputNode("Tiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(tilesFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                neuralConfig.InputNodes.Add(new InputNode("Dangers", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(dangersFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                neuralConfig.InputNodes.Add(new InputNode("Goodies", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(goodiesFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                neuralConfig.InputNodes.Add(new InputNode("Water", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(waterFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
            }

            neuralConfig.InputNodes.Add(new InputNode("On Ground", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsOnGround()));
            neuralConfig.InputNodes.Add(new InputNode("In Water", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsInWater()));
            neuralConfig.InputNodes.Add(new InputNode("Raising", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsRaising()));
            neuralConfig.InputNodes.Add(new InputNode("Sinking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsSinking()));
            neuralConfig.InputNodes.Add(new InputNode("Can jump out of water", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).CanJumpOutOfWater()));
            neuralConfig.InputNodes.Add(new InputNode("Carrying", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsCarryingSomething()));
            neuralConfig.InputNodes.Add(new InputNode("Can Climb", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).CanClimb()));
            neuralConfig.InputNodes.Add(new InputNode("Max Speed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsAtMaxSpeed()));
            neuralConfig.InputNodes.Add(new InputNode("Message Box", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).WasDialogBoxOpened()));
            neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
            neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

            neuralConfig.OutputNodes.Clear();
            neuralConfig.OutputNodes.Add(new OutputNode("A", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("B", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("X", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Y", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Left", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Right", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Up", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Down", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Left Shoulder", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Right Shoulder", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Start", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Select", neuralConfig.EnabledStates[enabledIndex++]));
        }
    }
}
