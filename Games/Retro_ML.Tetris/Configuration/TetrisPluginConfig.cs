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
             new IntegerFieldInfo(nameof(VisibleRows), "Visible Rows", 4, 17, 1),
        };

        public List<IScoreFactor> ScoreFactors { get; set; }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(VisibleRows) => VisibleRows,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(VisibleRows): VisibleRows = (int)value; break;
                }
            }
        }

        public int VisibleRows { get; set; } = 4;

        public TetrisPluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                new LineClearedScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = 1
                },
                new GameOverScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = -10
                },
                new TimeTakenScoreFactor()
                {
                    IsDisabled = false,
                    ScoreMultiplier = 0.01
                }
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            TetrisPluginConfig cfg = JsonConvert.DeserializeObject<TetrisPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;
            ScoreFactors = cfg.ScoreFactors;
            VisibleRows = cfg.VisibleRows;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {

            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 15)
            {
                neuralConfig.EnabledStates = Enumerable.Repeat(true, 5 + 4).Concat(Enumerable.Repeat(false, 6)).ToArray();
            }

            neuralConfig.InputNodes.Clear();
            neuralConfig.InputNodes.Add(new InputNode("Tiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetSolidTiles(), 10, VisibleRows));
            neuralConfig.InputNodes.Add(new InputNode("Current Block", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetCurrentBlock(), 4, 7));
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
