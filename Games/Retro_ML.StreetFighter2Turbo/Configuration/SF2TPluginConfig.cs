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
        private static readonly bool[] DEFAULT_ENABLED_STATES = new bool[]
        {
            true, //Player Crouched
            true, //Enemy Crouched
            true, //Player Jumping
            true, //Enemy Jumping
            true, //Player Attacking
            true, //Enemy Attacking
            true, //Player Punching
            true, //Enemy Punching
            true, //Player Kicking
            true, //Enemy Kicking
            true, //Player Throwing
            true, //Enemy Throwing
            true, //Player Blocking
            true, //Enemy Blocking
            true, //Player Staggered
            true, //Enemy Staggered
            true, //Player Attack Strength
            true, //Enemy Attack Strength
            true, //Player X
            true, //Enemy X
            true, //X Delta
            true, //Y Delta
            true, //Enemy Direction
            true, //Player Health
            true, //Enemy Health
            true, //Time Left
            false, //Internal Clock
            true, //Bias

            true, //A
            true, //B
            true, //X
            true, //Y
            true, //Left
            true, //Right
            true, //Up
            true, //Down
            true, //Left Shoulder
            true, //Right Shoulder
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
        /// <summary>
        /// The amount of inputs for the internal clock.
        /// </summary>
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

        public SF2TPluginConfig()
        {
            ScoreFactors = new List<IScoreFactor>()
            {
                new EndRoundScoreFactor() { IsDisabled=false, ScoreMultiplier = 1000 },
                new StopFightingScoreFactor() { IsDisabled = false, ScoreMultiplier = -50 },
                new CombatScoreFactor() { IsDisabled = false, ScoreMultiplier = 176 },
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this, SerializationUtils.JSON_CONFIG);

        public void Deserialize(string json)
        {
            SF2TPluginConfig cfg = JsonConvert.DeserializeObject<SF2TPluginConfig>(json, SerializationUtils.JSON_CONFIG)!;

            InternalClockLength = cfg.InternalClockLength;
            InternalClockTickLength = cfg.InternalClockTickLength;
            FrameSkip = cfg.FrameSkip;
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

            neuralConfig.InputNodes.Add(new InputNode("Player Crouched", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Crouched()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Crouched", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Crouched()));
            neuralConfig.InputNodes.Add(new InputNode("Player Airborn", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Jumping()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Airborn", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Jumping()));
            neuralConfig.InputNodes.Add(new InputNode("Player Attacking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Attacking()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Attacking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Attacking()));
            neuralConfig.InputNodes.Add(new InputNode("Player Punching", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Punching()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Punching", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Punching()));
            neuralConfig.InputNodes.Add(new InputNode("Player Kicking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Kicking()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Kicking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Kicking()));
            neuralConfig.InputNodes.Add(new InputNode("Player Throwing", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Throwing()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Throwing", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Throwing()));
            neuralConfig.InputNodes.Add(new InputNode("Player Blocking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Blocking()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Blocking", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Blocking()));
            neuralConfig.InputNodes.Add(new InputNode("Player Staggered", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer1Staggered()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Staggered", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).IsPlayer2Staggered()));
            neuralConfig.InputNodes.Add(new InputNode("Player Attack Strength", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer1AttackStrength()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Attack Strength", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer2AttackStrength()));
            neuralConfig.InputNodes.Add(new InputNode("Player X", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer1XPosNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy X", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer2XPosNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("X Delta", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetHorizontalDistanceBetweenPlayers()));
            neuralConfig.InputNodes.Add(new InputNode("Y Delta", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetVerticalDistanceBetweenPlayers()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Direction", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetEnemyDirection()));
            neuralConfig.InputNodes.Add(new InputNode("Player Health", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer1HpNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Enemy Health", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetPlayer2HpNormalized()));
            neuralConfig.InputNodes.Add(new InputNode("Time Left", neuralConfig.EnabledStates[enabledIndex++], (dataFetcher) => ((SF2TDataFetcher)dataFetcher).GetRoundTimerNormalized()));
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
