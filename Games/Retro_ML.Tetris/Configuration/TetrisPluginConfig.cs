using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Game;
using Retro_ML.Tetris.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.Tetris.Configuration
{
    internal class TetrisPluginConfig : IGamePluginConfig
    {
        public FieldInfo[] Fields => new FieldInfo[]
        {
             new IntegerFieldInfo(nameof(VisibleRows), "Visible Rows", 4, 17, 1, "Number of rows the AI can see"),
             new IntegerFieldInfo(nameof(NbAttempts), "Number of Attempts", 1, 50, 1, "The number of attempts the AI will do for each save states selected"),
             new BoolFieldInfo(nameof(UseNormalizedHeights), "Use Normalized Heights", "When this option is enabled, the AIs will be given the height of each columns normalized "),
             new IntegerFieldInfo(nameof(FrameSkip), "Frame Skip", 1, 20, 1,"")
        };

        public List<IScoreFactor> ScoreFactors { get; set; }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(VisibleRows) => VisibleRows,
                    nameof(NbAttempts) => NbAttempts,
                    nameof(UseNormalizedHeights) => UseNormalizedHeights,
                    nameof(FrameSkip) => FrameSkip,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(VisibleRows): VisibleRows = (int)value; break;
                    case nameof(NbAttempts): NbAttempts = (int)value; break;
                    case nameof(UseNormalizedHeights): UseNormalizedHeights = (bool)value; break;
                    case nameof(FrameSkip): FrameSkip = (int)value; break;
                }
            }
        }

        public int VisibleRows { get; set; } = 4;
        public int NbAttempts { get; set; } = 1;
        public bool UseNormalizedHeights { get; set; } = true;
        public int FrameSkip { get; set; } = 1;

        public TetrisPluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                new LineClearedScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = 10
                },
                new GameOverScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = -10
                },
                new TimeTakenScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = 0.1
                },
                new HoleScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = -2
                },
                new ColumnHeightScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = -1
                }
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            TetrisPluginConfig cfg = JsonConvert.DeserializeObject<TetrisPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;
            ScoreFactors = cfg.ScoreFactors;
            VisibleRows = cfg.VisibleRows;
            NbAttempts = cfg.NbAttempts;
            FrameSkip = cfg.FrameSkip;
            UseNormalizedHeights = cfg.UseNormalizedHeights;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {

            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 16)
            {
                neuralConfig.EnabledStates = Enumerable.Repeat(true, 6 + 4).Concat(Enumerable.Repeat(false, 6)).ToArray();
            }

            neuralConfig.InputNodes.Clear();

            if (UseNormalizedHeights)
                neuralConfig.InputNodes.Add(new InputNode("Tiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetNormalizedHeight(), 10, 1));
            else
                neuralConfig.InputNodes.Add(new InputNode("Tiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetSolidTiles(), 10, VisibleRows));

            neuralConfig.InputNodes.Add(new InputNode("Current Block Type", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetCurrentBlockType(), 7, 1));
            neuralConfig.InputNodes.Add(new InputNode("Current Block Rotation", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetCurrentBlockType(), 4, 1));
            neuralConfig.InputNodes.Add(new InputNode("Current Pos X", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetPositionX()));
            neuralConfig.InputNodes.Add(new InputNode("Current Pos Y", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetPositionY()));
            neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

            neuralConfig.OutputNodes.Clear();
            neuralConfig.OutputNodes.Add(new OutputNode("A", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("B", neuralConfig.EnabledStates[enabledIndex++])); ;
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
