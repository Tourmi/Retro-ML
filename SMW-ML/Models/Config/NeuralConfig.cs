using Newtonsoft.Json;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMW_ML.Models.Config
{
    /// <summary>
    /// Configuration of the neural network. Modifying these values makes previous neural network incompatible with the new settings.
    /// </summary>
    internal class NeuralConfig
    {
        /// <summary>
        /// The input nodes to use by the neural network
        /// </summary>
        [JsonIgnore]
        public List<InputNode> InputNodes { get; }
        /// <summary>
        /// The output nodes used by the neural network. 
        /// </summary>
        [JsonIgnore]
        public List<OutputNode> OutputNodes { get; }

        /// <summary>
        /// Stores whether or not an input or output node is enabled, based on the index of them.
        /// </summary>
        public bool[] EnabledStates { get; set; }

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
        public int RaysWidth => RayCount / Raycast.OUTPUT_HEIGHT;
        /// <summary>
        /// The height of the ray inputs
        /// </summary>
        [JsonIgnore]
        public int RaysHeight => Raycast.OUTPUT_HEIGHT;

        public NeuralConfig()
        {
            InputNodes = new List<InputNode>();
            OutputNodes = new List<OutputNode>();
            EnabledStates = Enumerable.Repeat(true, 15 + 8).Concat(Enumerable.Repeat(false, 4)).ToArray();

            InitNodes();
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

        /// <summary>
        /// Initializes all of the input and output nodes so that they're ready to use.
        /// </summary>
        public void InitNodes()
        {
            int enabledIndex = 0;
            InputNodes.Clear();

            Func<DataFetcher, int, int, bool[,]> tilesFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetWalkableTilesAroundPosition(gridDistX, gridDistY);
            Func<DataFetcher, int, int, bool[,]> dangersFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetDangerousTilesAroundPosition(gridDistX, gridDistY);
            Func<DataFetcher, int, int, bool[,]> goodiesFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetGoodTilesAroundPosition(gridDistX, gridDistY);
            Func<DataFetcher, int, int, bool[,]> waterFunc = (dataFetcher, gridDistX, gridDistY) => dataFetcher.GetWaterTilesAroundPosition(gridDistX, gridDistY);

            if (UseGrid)
            {
                InputNodes.Add(new InputNode("Grid Tiles", EnabledStates[enabledIndex++], (dataFetcher) => tilesFunc(dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                InputNodes.Add(new InputNode("Grid Dangers", EnabledStates[enabledIndex++], (dataFetcher) => dangersFunc(dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                InputNodes.Add(new InputNode("Grid Goodies", EnabledStates[enabledIndex++], (dataFetcher) => goodiesFunc(dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
                InputNodes.Add(new InputNode("Grid Water", EnabledStates[enabledIndex++], (dataFetcher) => waterFunc(dataFetcher, GridDistanceX, GridDistanceY), GridWidth, GridHeight));
            }
            else
            {
                InputNodes.Add(new InputNode("Ray Tiles", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(tilesFunc(dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                InputNodes.Add(new InputNode("Ray Dangers", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(dangersFunc(dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                InputNodes.Add(new InputNode("Ray Goodies", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(goodiesFunc(dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
                InputNodes.Add(new InputNode("Ray Water", EnabledStates[enabledIndex++], (dataFetcher) => Raycast.GetRayDistances(waterFunc(dataFetcher, RayLength, RayLength), RayLength, RayCount), RaysWidth, RaysHeight));
            }

            InputNodes.Add(new InputNode("On Ground", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.IsOnGround()));
            InputNodes.Add(new InputNode("In Water", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.IsInWater()));
            InputNodes.Add(new InputNode("Raising", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.IsRaising()));
            InputNodes.Add(new InputNode("Sinking", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.IsSinking()));
            InputNodes.Add(new InputNode("Can jump out of water", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.CanJumpOutOfWater()));
            InputNodes.Add(new InputNode("Carrying", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.IsCarryingSomething()));
            InputNodes.Add(new InputNode("Can Climb", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.CanClimb()));
            InputNodes.Add(new InputNode("Max Speed", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.IsAtMaxSpeed()));
            InputNodes.Add(new InputNode("Message Box", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.WasDialogBoxOpened()));
            InputNodes.Add(new InputNode("Internal Clock", EnabledStates[enabledIndex++], (dataFetcher) => dataFetcher.WasInternalClockTriggered()));
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

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public static NeuralConfig Deserialize(string json)
        {
            NeuralConfig cfg = JsonConvert.DeserializeObject<NeuralConfig>(json, SerializationUtils.JSON_CONFIG)!;
            cfg.InitNodes();

            return cfg;
        }
    }
}
