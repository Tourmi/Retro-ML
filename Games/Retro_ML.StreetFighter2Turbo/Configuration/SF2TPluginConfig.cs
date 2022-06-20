using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.StreetFighter2Turbo.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.StreetFighter2Turbo.Configuration
{
    internal class SF2TPluginConfig : IGamePluginConfig
    {
        public FieldInfo[] Fields => new FieldInfo[]
        {
            new IntegerFieldInfo(nameof(InternalClockTickLength), "Internal Clock Tick Length (Frames)", 1, 3600, 1),
            new IntegerChoiceFieldInfo(nameof(InternalClockLength), "Internal Clock Length", new int[] {1,2,3,4,5,6,7,8,16 })
        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(InternalClockLength) => InternalClockLength,
                    nameof(InternalClockTickLength) => InternalClockTickLength,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(InternalClockLength): InternalClockLength = (int)value; break;
                    case nameof(InternalClockTickLength): InternalClockTickLength = (int)value; break;
                }
            }
        }

        /// <summary>
        /// The amount of inputs for the internal clock.
        /// </summary>
        public int InternalClockLength { get; set; } = 4;
        /// <summary>
        /// The amount of frames before the clock moves to the next state.
        /// </summary>
        public int InternalClockTickLength { get; set; } = 2;

        public List<IScoreFactor> ScoreFactors { get; set; }

        public SF2TPluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                new DiedScoreFactor() { IsDisabled=false, ScoreMultiplier=-5 },
                new WonRoundlScoreFactor() { IsDisabled = false, ScoreMultiplier=500 },
                new IsInHittingDistanceScoreFactor() { IsDisabled = false, ScoreMultiplier=1 },
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            SF2TPluginConfig cfg = JsonConvert.DeserializeObject<SF2TPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

            InternalClockLength = cfg.InternalClockLength;
            InternalClockTickLength = cfg.InternalClockTickLength;
            ScoreFactors = cfg.ScoreFactors;
        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {

            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 8 + 10)
            {
                neuralConfig.EnabledStates = Enumerable.Repeat(true, 8 + 10).ToArray();
            }
            neuralConfig.InputNodes.Clear();

            neuralConfig.InputNodes.Add(new InputNode("Player Crouched", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).isPlayer1Crouched()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Crouched", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).isPlayer2Crouched()));
            neuralConfig.InputNodes.Add(new InputNode("Player Airborn", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).isPlayer1Airborn()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Airborn", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).isPlayer1Airborn()));
            neuralConfig.InputNodes.Add(new InputNode("Player X Position", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer1AbsoluteXPosition()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy X Position", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer2AbsoluteXPosition()));
            neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
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
        }
    }
}
