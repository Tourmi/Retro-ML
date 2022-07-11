using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperBomberman3.Game;
//using Retro_ML.SuperBomberman3.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.SuperBomberman3.Configuration
{
    internal class SB3PluginConfig : IGamePluginConfig
    {
        private static readonly bool[] DEFAULT_ENABLED_STATES = new bool[]
        {
            true, //Player Position

            false, //Internal Clock
            true, //Bias

            true, //A
            true, //B
            true, //Y
            true, //Left
            true, //Right
            true, //Up
            true, //Down
        };

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1),
            new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 }),
            new IntegerFieldInfo(nameof(FrameSkip), "Frames to skip", 0, 15, 1, "Amount of frames to skip for every AI evaluation"),
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
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
                    case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                    case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                    case nameof(FrameSkip): FrameSkip = (int)value; break;
                }
            }
        }

        public int InternalClockLength { get; set; } = 4;
        /// <summary>
        /// The amount of frames before the clock moves to the next state.
        /// </summary>
        public int InternalClockTickLength { get; set; } = 2;

        /// <summary>
        /// Skips this amount of frames for every neural network updates.
        /// </summary>
        public int FrameSkip { get; set; } = 0;

        public List<IScoreFactor> ScoreFactors { get; set; }

        public SB3PluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                //new DiedScoreFactor() { IsDisabled=false, ScoreMultiplier=-5 },
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            SB3PluginConfig cfg = JsonConvert.DeserializeObject<SB3PluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

            InternalClockLength = cfg.InternalClockLength;
            InternalClockTickLength = cfg.InternalClockTickLength;
            ScoreFactors = cfg.ScoreFactors;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {
            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != DEFAULT_ENABLED_STATES.Length)
            {
                neuralConfig.EnabledStates = DEFAULT_ENABLED_STATES;
            }
            neuralConfig.InputNodes.Clear();

            neuralConfig.InputNodes.Add(new InputNode("Bomberman Position", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetBomberManPositionInLevel(), (int) SB3DataFetcher.LEVEL_WIDTH, (int) SB3DataFetcher.LEVEL_HEIGHT));

            neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
            neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

            neuralConfig.OutputNodes.Clear();
            neuralConfig.OutputNodes.Add(new OutputNode("A", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("B", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Y", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Left", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Right", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Up", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Down", neuralConfig.EnabledStates[enabledIndex++]));
        }
    }
}
