using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;
using Retro_ML.SuperMarioKart.Game.Data;
using Retro_ML.SuperMarioKart.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.SuperMarioKart.Configuration
{
    internal class SMKPluginConfig : IGamePluginConfig
    {
        public FieldInfo[] Fields => new FieldInfo[]
        {
            new IntegerFieldInfo(nameof(ViewDistance), "View Distance", 4, 64, 1, "Distance in tiles that the AI can see"),
            new IntegerFieldInfo(nameof(ViewAngle), "View Angle (Degrees)", 10, 360, 10, "The angle at which the AI can see"),
            new IntegerChoiceFieldInfo(nameof(Raycount), "Raycast count", new int[] { 4, 8, 16, 32, 64 }, "The amount of vision rays to use"),
            new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1, "The time (in frames) that it takes for the internal clock to move to its next state"),
            new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 }, "The total amount of nodes the internal clock has"),
            new IntegerFieldInfo(nameof(FrameSkip), "Frames to skip", 0, 15, 1, "Amount of frames to skip on every AI iteration")
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(ViewDistance) => ViewDistance,
                    nameof(ViewAngle) => ViewAngle,
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
                    case nameof(ViewDistance): ViewDistance = (int)value; break;
                    case nameof(ViewAngle): ViewAngle = (int)value; break;
                    case nameof(Raycount): Raycount = (int)value; break;
                    case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                    case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                    case nameof(FrameSkip): FrameSkip = (int)value; break;
                }
            }
        }
        /// <summary>
        /// How many tiles ahead we can see
        /// </summary>
        public int ViewDistance { get; set; } = 16;
        /// <summary>
        /// The maximum angle at which the AI can see
        /// </summary>
        public int ViewAngle { get; set; } = 150;
        /// <summary>
        /// The amount of rays to send out
        /// </summary>
        public int Raycount { get; set; } = 8;

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

        public SMKPluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                new FinishedRaceScoreFactor() { IsDisabled=false, ScoreMultiplier=100 },
                new StoppedProgressingScoreFactor() { IsDisabled=false, ScoreMultiplier=-15 },
                new CheckpointReachedScoreFactor() { IsDisabled=false, ScoreMultiplier=5 },
                new TimeTakenScoreFactor() { IsDisabled=false, ScoreMultiplier=-1 },
                new OffRoadScoreFactor() { IsDisabled=false, ScoreMultiplier=-2 },
                new LakituScoreFactor() { IsDisabled=false, ScoreMultiplier=-10 },
                new CollisionScoreFactor() { IsDisabled=false, ScoreMultiplier=-1 },
                new CoinsScoreFactor() { IsDisabled=true, ScoreMultiplier=1 },
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            SMKPluginConfig cfg = JsonConvert.DeserializeObject<SMKPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

            ScoreFactors = cfg.ScoreFactors;
            InternalClockLength = cfg.InternalClockLength;
            InternalClockTickLength = cfg.InternalClockTickLength;
            ViewDistance = cfg.ViewDistance;
            ViewAngle = cfg.ViewAngle;
            Raycount = cfg.Raycount;
            FrameSkip = cfg.FrameSkip;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {
            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 11 + 12)
            {
                neuralConfig.EnabledStates = new bool[]
                {
                    true, //flowmap
                    true, //obstacles
                    false, //racers
                    true, //offroad
                    false, //solid
                    false, //pit
                    false, //goodies
                    false, //current item
                    true, //speed
                    false, //clock
                    true, //bias

                    true, //a
                    true, //b
                    false, //x
                    true, //y
                    true, //left
                    true, //right
                    false, //up
                    false, //down
                    true, //left shoulder
                    false, //right shoulder
                    false, //start
                    false //select
                };
            }
            neuralConfig.InputNodes.Clear();
            neuralConfig.InputNodes.Add(new InputNode("FlowMap direction", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetHeadingDifference()));
            neuralConfig.InputNodes.Add(new InputNode("Obstacles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetObstacleRays(ViewDistance, Raycount), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Racers", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetRacersRays(ViewDistance, Raycount), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Offroad", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetRays(ViewDistance, Raycount, TiletypeSurface.IsOffroad), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Solid", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetRays(ViewDistance, Raycount, TiletypeSurface.IsSolid), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Pit", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetRays(ViewDistance, Raycount, TiletypeSurface.IsPit), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Goodies", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetRays(ViewDistance, Raycount, TiletypeSurface.IsGoodTile), Raycount / 4, 4));
            neuralConfig.InputNodes.Add(new InputNode("Current Item", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetCurrentItem(), 3, 3));
            neuralConfig.InputNodes.Add(new InputNode("Speed", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetSpeedOutOfMaxSpeed()));
            neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SMKDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
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
