using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.SuperBomberman3.Game;

namespace Retro_ML.SuperBomberMan3.Neural.Scoring
{
    internal class BombScoreFactor : IScoreFactor
    {
        private double currScore;
        private bool shouldStop = false;
        private int enemyEliminated;
        private int wallsDestroyed;

        public FieldInfo[] Fields => new FieldInfo[]
        {
            new DoubleFieldInfo(nameof(EnemyEliminationMultiplier), "Enemy Elimination Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player kills an enemy"),
            new DoubleFieldInfo(nameof(WallDestructiondMultiplier), "Wall Destruction Multiplier", double.MinValue, double.MaxValue, 1, "Multiplier applied on top of the regular multiplier when the player destroy a destructible wall"),
        };

        public BombScoreFactor()
        {
            ExtraFields = Array.Empty<ExtraField>();
        }

        public object this[string fieldName]
        {
            get
            {
                return fieldName switch
                {
                    nameof(EnemyEliminationMultiplier) => EnemyEliminationMultiplier,
                    nameof(WallDestructiondMultiplier) => WallDestructiondMultiplier,
                    _ => 0,
                };
            }
            set
            {
                switch (fieldName)
                {
                    case nameof(EnemyEliminationMultiplier): EnemyEliminationMultiplier = (double)value; break;
                    case nameof(WallDestructiondMultiplier): WallDestructiondMultiplier = (double)value; break;
                }
            }
        }

        public bool ShouldStop => shouldStop;

        public double ScoreMultiplier { get; set; }

        public double EnemyEliminationMultiplier { get; set; } = 25;

        public double WallDestructiondMultiplier { get; set; } = 1;

        public string Name => "Bomb score";

        public string Tooltip => "Reward AI when it destroy walls or eliminate enemies with his bombs";

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
            enemyEliminated = dataFetcher.GetEnemyEliminated();
            wallsDestroyed = dataFetcher.GetWallsDestroyed();
        }

        public void LevelDone()
        {
            shouldStop = false;

            currScore += ScoreMultiplier * ((enemyEliminated * EnemyEliminationMultiplier) + (wallsDestroyed * WallDestructiondMultiplier));

            wallsDestroyed = 0;
            enemyEliminated = 0;
        }

        public IScoreFactor Clone()
        {
            return new BombScoreFactor()
            {
                ScoreMultiplier = ScoreMultiplier,
                IsDisabled = IsDisabled,
                EnemyEliminationMultiplier = EnemyEliminationMultiplier,
                WallDestructiondMultiplier = WallDestructiondMultiplier,
            };
        }
    }
}
