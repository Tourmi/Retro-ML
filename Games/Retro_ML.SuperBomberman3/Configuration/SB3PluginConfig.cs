using Newtonsoft.Json;
using Retro_ML.Configuration;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperBomberman3.Game;
using Retro_ML.SuperBomberMan3.Neural.Scoring;
using Retro_ML.Utils;

namespace Retro_ML.SuperBomberman3.Configuration
{
    internal class SB3PluginConfig : IGamePluginConfig
    {
        //Enemy count can vary from 1 to 4. We trained using 3 enemies.
        private const int enemyCount = 3;

        private static readonly bool[] DEFAULT_ENABLED_STATES = new bool[]
        {
            true, //Tile Map
            true, //Dangers

            true, //Player X Pos
            true, //Player Y Pos
            true, //Enemies X Distance
            true, //Enemies Y Distance
            true, //Closest Powerup X Distance
            true, //Closest Powerup Y Distance
            true, //Number of Bomb Planted
            true, //Round Timer

            true, //Extra bomb level
            true, //Explosion Expander level
            true, //Accelerator level
            true, //Has Kick
            true, //Has Glove
            true, //Has Sticky Bomb
            true, //Has Power Bomb
            true, //On a Louie
            true, //Yellow Louie
            true, //Brown Louie
            true, //Pink Louie
            true, //Green Louie
            true, //Blue Louie

            false, //Internal Clock
            true, //Bias

            true, //A
            true, //X
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
                new IdleScoreFactor() { IsDisabled=false, ScoreMultiplier=-5 },
                new PowerupScoreFactor() { IsDisabled=false, ScoreMultiplier=1 },
                new EndRoundScoreFactor() { IsDisabled=false, ScoreMultiplier=100 },
                new TimeTakenScoreFactor() { IsDisabled=false, ScoreMultiplier=0.25 },
                new BombScoreFactor() { IsDisabled=false, ScoreMultiplier=1 },
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

            neuralConfig.InputNodes.Add(new InputNode("Tile Map", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).DrawTiles(), (int)SB3DataFetcher.DESIRED_LEVEL_WIDTH, (int)SB3DataFetcher.DESIRED_LEVEL_HEIGHT));
            neuralConfig.InputNodes.Add(new InputNode("Dangers", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).DrawDangers(), (int)SB3DataFetcher.DESIRED_LEVEL_WIDTH, (int)SB3DataFetcher.DESIRED_LEVEL_HEIGHT));

            neuralConfig.InputNodes.Add(new InputNode("Player X", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerXPositionNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Player Y", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerYPositionNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Enemies X Distance", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetEnemiesXDistanceToThePlayer(), enemyCount, 1));
            neuralConfig.InputNodes.Add(new InputNode("Enemies Y Distance", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetEnemiesYDistanceToThePlayer(), enemyCount, 1));
            neuralConfig.InputNodes.Add(new InputNode("Closest Powerup X Distance", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetClosestPowerupToMainPlayerXPosNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Closest Powerup Y Distance", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetClosestPowerupToMainPlayerYPosNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Number Of Bomb Planted", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerBombsPlantedNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Round Timer", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetRemainingRoundTimeNormalized()));

            neuralConfig.InputNodes.Add(new InputNode("Extra bomb level", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerExtraBombPowerUpLevelNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Explosion Expander level ", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerExplosionExpanderPowerUpLevelNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Accelerator level", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerAcceleratorPowerUpLevelNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Has Kick", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerKickUpgradeState()));
            neuralConfig.InputNodes.Add(new InputNode("Has Gloves", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerGloveUpgradeState()));
            neuralConfig.InputNodes.Add(new InputNode("Has Sticky Bomb", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerSlimeBombUpgradeState()));
            neuralConfig.InputNodes.Add(new InputNode("Has Power Bomb", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetMainPlayerPowerBombUpgradeState()));
            neuralConfig.InputNodes.Add(new InputNode("On a Louie", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).IsMainPlayerOnLouie()));
            neuralConfig.InputNodes.Add(new InputNode("Yellow Louie", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).IsLouieColourYellow()));
            neuralConfig.InputNodes.Add(new InputNode("Brown Louie", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).IsLouieColourBrown()));
            neuralConfig.InputNodes.Add(new InputNode("Pink Louie", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).IsLouieColourPink()));
            neuralConfig.InputNodes.Add(new InputNode("Green Louie", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).IsLouieColourGreen()));
            neuralConfig.InputNodes.Add(new InputNode("Blue Louie", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).IsLouieColourBlue()));
            neuralConfig.InputNodes.Add(new InputNode("Internal Clock", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SB3DataFetcher)dataFetcher).GetInternalClockState(), Math.Min(8, InternalClockLength), Math.Max(1, InternalClockLength / 8)));
            neuralConfig.InputNodes.Add(new InputNode("Bias", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => true));

            neuralConfig.OutputNodes.Clear();
            neuralConfig.OutputNodes.Add(new OutputNode("A", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("X", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Y", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Left", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Right", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Up", neuralConfig.EnabledStates[enabledIndex++]));
            neuralConfig.OutputNodes.Add(new OutputNode("Down", neuralConfig.EnabledStates[enabledIndex++]));
        }
    }
}
