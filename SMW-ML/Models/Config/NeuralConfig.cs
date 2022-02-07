using SMW_ML.Game.SuperMarioWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Models.Config
{
    internal class NeuralConfig
    {
        public List<InputNode> InputNodes { get; set; }
        public List<OutputNode> OutputNodes { get; set; }
        public int GridDistanceX { get; set; }
        public int GridDistanceY { get; set; }

        public int GridWidth => GridDistanceX * 2 + 1;
        public int GridHeight => GridDistanceY * 2 + 1;

        public NeuralConfig()
        {
            GridDistanceX = 4;
            GridDistanceY = 4;

            InputNodes = new List<InputNode>()
            {
                new InputNode("Tiles", true, (dataFetcher) => dataFetcher.GetWalkableTilesAroundPosition(GridDistanceX, GridDistanceY), GridWidth, GridHeight),
                new InputNode("Dangers", true, (dataFetcher) => dataFetcher.GetDangerousTilesAroundPosition(GridDistanceX, GridDistanceY), GridWidth, GridHeight),
                new InputNode("On Ground", true, (dataFetcher) => dataFetcher.IsOnGround()),
                new InputNode("In Water", true, (dataFetcher) => dataFetcher.IsInWater()),
                new InputNode("Raising", true, (dataFetcher) => dataFetcher.IsRaising()),
                new InputNode("Sinking", true, (dataFetcher) => dataFetcher.IsSinking()),
                new InputNode("Can jump out of water", true, (dataFetcher) => dataFetcher.CanJumpOutOfWater()),
                new InputNode("Carrying", true, (dataFetcher) => dataFetcher.IsCarryingSomething()),
                new InputNode("Can Climb", true, (dataFetcher) => dataFetcher.CanClimb()),
                new InputNode("Max Speed", true, (dataFetcher) => dataFetcher.IsAtMaxSpeed()),
                new InputNode("Message Box", true, (dataFetcher) => dataFetcher.WasDialogBoxOpened()),
                new InputNode("Internal Clock", true, (dataFetcher) => dataFetcher.WasInternalClockTriggered()),
                new InputNode("Bias", true, (dataFetcher) => true)
            };

            OutputNodes = new List<OutputNode>()
            {
                new OutputNode("A", true),
                new OutputNode("B", true),
                new OutputNode("X", true),
                new OutputNode("Y", true),
                new OutputNode("Left", true),
                new OutputNode("Right", true),
                new OutputNode("Up", true),
                new OutputNode("Down", true),
                new OutputNode("Left Shoulder", false),
                new OutputNode("Right Shoulder", false),
                new OutputNode("Start", false),
                new OutputNode("Select", false),
            };
        }
    }
}
