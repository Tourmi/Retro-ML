﻿using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Tetris.Game;
using Retro_ML.Utils;

namespace Retro_ML.Tetris.Configuration
{
    internal class TetrisPluginConfig : IGamePluginConfig
    {
        public FieldInfo[] Fields => new FieldInfo[]
        {

        };

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {

                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {

                }
            }
        }


        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            TetrisPluginConfig cfg = JsonConvert.DeserializeObject<TetrisPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

        }

        public void InitNeuralConfig(NeuralConfig neuralConfig)
        {

            int enabledIndex = 0;
            if (neuralConfig.EnabledStates.Length != 12)
            {
                neuralConfig.EnabledStates = Enumerable.Repeat(true, 2 + 6).Concat(Enumerable.Repeat(false, 4)).ToArray();
            }

            neuralConfig.InputNodes.Clear();
            neuralConfig.InputNodes.Add(new InputNode("Tiles", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((TetrisDataFetcher)dataFetcher).GetBackgroundTiles(), 32, 32));
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
