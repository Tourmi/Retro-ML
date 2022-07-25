using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.SuperBomberman3.Game;

namespace Retro_ML.SuperBomberMan3.Neural.Scoring
{
    internal class PowerupScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool isInited;
        private bool shouldStop = false;

        private double explosionExpanderLevel;
        private double extraBombLevel;
        private double acceleratorLevel;
        private bool isOnALouie;
        private bool hasKick;
        private bool hasGlove;
        private bool hasSlimeBomb;
        private bool hasPowerBomb;

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new DoubleFieldInfo(nameof(ExplosionExpanderMultiplier), "Explosion Expander Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab an explosion expander powerup"),
            new DoubleFieldInfo(nameof(ExtraBombMultiplier), "Extra Bomb Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab an extra bomb powerup"),
            new DoubleFieldInfo(nameof(AcceleratorMultiplier), "Accelerator Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab an accelerator powerup"),
            new DoubleFieldInfo(nameof(LouieMultiplier), "Louie Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab an egg"),
            new DoubleFieldInfo(nameof(KickMultiplier), "Kick Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab a kick powerup"),
            new DoubleFieldInfo(nameof(GloveMultiplier), "Glove Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab a slime bomb powerup"),
            new DoubleFieldInfo(nameof(SlimeBombMultiplier), "Slime Bomb Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab a slime bomb powerup"),
            new DoubleFieldInfo(nameof(PowerBombMultiplier), "Power Bomb Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player grab a power bomb powerup"),
        };

        public PowerupScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(ExplosionExpanderMultiplier) => ExplosionExpanderMultiplier,
                    nameof(ExtraBombMultiplier) => ExtraBombMultiplier,
                    nameof(AcceleratorMultiplier) => AcceleratorMultiplier,
                    nameof(LouieMultiplier) => LouieMultiplier,
                    nameof(KickMultiplier) => KickMultiplier,
                    nameof(GloveMultiplier) => GloveMultiplier,
                    nameof(SlimeBombMultiplier) => SlimeBombMultiplier,
                    nameof(PowerBombMultiplier) => PowerBombMultiplier,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(ExplosionExpanderMultiplier): ExplosionExpanderMultiplier = (double)value; break;
                    case nameof(ExtraBombMultiplier): ExtraBombMultiplier = (double)value; break;
                    case nameof(AcceleratorMultiplier): AcceleratorMultiplier = (double)value; break;
                    case nameof(LouieMultiplier): LouieMultiplier = (double)value; break;
                    case nameof(KickMultiplier): KickMultiplier = (double)value; break;
                    case nameof(GloveMultiplier): GloveMultiplier = (double)value; break;
                    case nameof(SlimeBombMultiplier): SlimeBombMultiplier = (double)value; break;
                    case nameof(PowerBombMultiplier): PowerBombMultiplier = (double)value; break;
                }
            }
        }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }

        public double ExplosionExpanderMultiplier { get; set; } = 1;

        public double ExtraBombMultiplier { get; set; } = 1;

        public double AcceleratorMultiplier { get; set; } = 2;

        public double LouieMultiplier { get; set; } = 10;

        public double KickMultiplier { get; set; } = 5;

        public double GloveMultiplier { get; set; } = 5;

        public double SlimeBombMultiplier { get; set; } = 5;

        public double PowerBombMultiplier { get; set; } = 5;

        public string Name => "Powerup acquired";

        public string Tooltip => "Reward AI when it claims powerup. In case of a skull, the ai will be penalized";

        public bool CanBeDisabled => true;

        public bool IsDisabled { get; set; }

        public ExtraField[] ExtraFields { get; set; }

        public double GetFinalScore() => currScore;

        public void Update(IDataFetcher dataFetcher)
        {
            Update((SB3DataFetcher)dataFetcher);
        }

        private void Update(SB3DataFetcher dataFetcher)
        {
            var extraBomb = dataFetcher.GetPlayerExtraBombPowerUpLevel();
            var explosionExpander = dataFetcher.GetPlayerExplosionExpanderPowerUpLevel();
            var accelerator = dataFetcher.GetPlayerAcceleratorPowerUpLevel();
            var louie = dataFetcher.GetPlayerLouiePowerUpState();
            var kick = dataFetcher.GetPlayerKickPowerUpState();
            var glove = dataFetcher.GetPlayerGlovePowerUpState();
            var slimeBomb = dataFetcher.GetPlayerSlimeBombPowerUpState();
            var powerBomb = dataFetcher.GetPlayerPowerBombPowerUpState();

            if (!isInited)
            {
                isInited = true;
                extraBombLevel = extraBomb;
                explosionExpanderLevel = explosionExpander;
                acceleratorLevel = accelerator;
                isOnALouie = louie;
                hasKick = kick;
                hasGlove = glove;
                hasSlimeBomb = slimeBomb;
                hasPowerBomb = powerBomb;
            }

            //Check for extra bomb. It cannot diminish.
            if (extraBomb > extraBombLevel)
            {
                currScore += ScoreMultiplier * ExtraBombMultiplier * (extraBomb - extraBombLevel);
            }

            //Check for explosion expander. It can diminish.
            if (explosionExpander > explosionExpanderLevel)
            {
                currScore += ScoreMultiplier * ExplosionExpanderMultiplier * (explosionExpander - explosionExpanderLevel);
            }

            //Check for accelerator. It can diminish.
            if (accelerator > acceleratorLevel)
            {
                currScore += ScoreMultiplier * AcceleratorMultiplier * (accelerator - acceleratorLevel);
            }

            //Check for louie. Player can lose it and get it back multiple times.
            if (isOnALouie == false && louie == true)
            {
                currScore += ScoreMultiplier * LouieMultiplier;
            }

            //Check for kick. Once acquired, the player cant acquire another one or lose it.
            if (hasKick == false && kick == true)
            {
                currScore += ScoreMultiplier * KickMultiplier;
            }

            //Check for glove. Once acquired, the player cant acquire another one or lose it.
            if (hasGlove == false && glove == true)
            {
                currScore += ScoreMultiplier * GloveMultiplier;
            }

            //Check for slime bomb. Once acquired, the player can lose it since it cant hold both powerBomb and slimeBomb at the same time. Once lost, the player can acquire it back.
            if (hasSlimeBomb == false && slimeBomb == true)
            {
                currScore += ScoreMultiplier * SlimeBombMultiplier;
            }

            //Check for powerbomb bomb. Once acquired, the player can lose it since it cant hold both powerBomb and slimeBomb at the same time. Once lost, the player can acquire it back.
            if (hasPowerBomb == false && powerBomb == true)
            {
                currScore += ScoreMultiplier * PowerBombMultiplier;
            }

            extraBombLevel = extraBomb;
            explosionExpanderLevel = explosionExpander;
            acceleratorLevel = accelerator;
            isOnALouie = louie;
            hasKick = kick;
            hasGlove = glove;
            hasSlimeBomb = slimeBomb;
            hasPowerBomb = powerBomb;
        }

        public void LevelDone()
        {
            shouldStop = false;
            isInited = false;
            explosionExpanderLevel = 0.0;
            extraBombLevel = 0.0;
            acceleratorLevel = 0.0;
            isOnALouie = false;
            hasKick = false;
            hasGlove = false;
            hasSlimeBomb = false;
            hasPowerBomb = false;
        }

        public IScoreFactor Clone()
        {
            return new PowerupScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                ExplosionExpanderMultiplier = ExplosionExpanderMultiplier,
                ExtraBombMultiplier = ExtraBombMultiplier,
                AcceleratorMultiplier = AcceleratorMultiplier,
                LouieMultiplier = LouieMultiplier,
                KickMultiplier = KickMultiplier,
                GloveMultiplier = GloveMultiplier,
                SlimeBombMultiplier = SlimeBombMultiplier,
                PowerBombMultiplier = PowerBombMultiplier,
            };
        }
    }
}
