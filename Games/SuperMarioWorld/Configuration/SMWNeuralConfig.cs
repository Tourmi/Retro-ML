using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Neural;
using Retro_ML.SuperMarioWorld.Game;

namespace Retro_ML.SuperMarioWorld.Configuration
{
    internal class SMWNeuralConfig : NeuralConfig
    {
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

        public override void InitNodes()
        {
            int enabledIndex = 0;
            InputNodes.Clear();

            Func<SMWDataFetcher, int, int, bool[,]> tilesFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetWalkableTilesAroundPosition(gridDistX, gridDistY);
            Func<SMWDataFetcher, int, int, bool[,]> dangersFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetDangerousTilesAroundPosition(gridDistX, gridDistY);
            Func<SMWDataFetcher, int, int, bool[,]> goodiesFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetGoodTilesAroundPosition(gridDistX, gridDistY);
            Func<SMWDataFetcher, int, int, bool[,]> waterFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetWaterTilesAroundPosition(gridDistX, gridDistY);

            if (UseGrid)
            {
                InputNodes.Add(new InputNode("Grid Tiles", EnabledStates[enabledIndex++], (dataFetcher) => tilesFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                InputNodes.Add(new InputNode("Grid Dangers", EnabledStates[enabledIndex++], (dataFetcher) => dangersFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                InputNodes.Add(new InputNode("Grid Goodies", EnabledStates[enabledIndex++], (dataFetcher) => goodiesFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                InputNodes.Add(new InputNode("Grid Water", EnabledStates[enabledIndex++], (dataFetcher) => waterFunc((SMWDataFetcher)dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
            }
            else
            {
                InputNodes.Add(new InputNode("Ray Tiles", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(tilesFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                InputNodes.Add(new InputNode("Ray Dangers", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(dangersFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                InputNodes.Add(new InputNode("Ray Goodies", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(goodiesFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                InputNodes.Add(new InputNode("Ray Water", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(waterFunc((SMWDataFetcher)dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
            }

            InputNodes.Add(new InputNode("On Ground", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsOnGround()));
            InputNodes.Add(new InputNode("In Water", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsInWater()));
            InputNodes.Add(new InputNode("Raising", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsRaising()));
            InputNodes.Add(new InputNode("Sinking", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsSinking()));
            InputNodes.Add(new InputNode("Can jump out of water", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).CanJumpOutOfWater()));
            InputNodes.Add(new InputNode("Carrying", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsCarryingSomething()));
            InputNodes.Add(new InputNode("Can Climb", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).CanClimb()));
            InputNodes.Add(new InputNode("Max Speed", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).IsAtMaxSpeed()));
            InputNodes.Add(new InputNode("Message Box", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).WasDialogBoxOpened()));
            InputNodes.Add(new InputNode("Internal Clock", EnabledStates[enabledIndex++], (dataFetcher) => ((SMWDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
            InputNodes.Add(new InputNode("Bias", EnabledStates[enabledIndex++], (dataFetcher) => true));

            OutputNodes.Clear();
            OutputNodes.Add(new OutputNode("A", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("B", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("X", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Y", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Left", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Right", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Up", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Down", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Left Shoulder", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Right Shoulder", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Start", EnabledStates[enabledIndex++]));
            OutputNodes.Add(new OutputNode("Select", EnabledStates[enabledIndex++]));
        }
    }
}
