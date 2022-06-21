using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Metroid.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.Metroid.Neural.Scoring;
internal class ObjectiveScoreFactor : IScoreFactor
{
    private double currScore;
    private bool isInit = false;
    private bool wasSamusFrozen;
    private bool wasBossPresent;
    private byte currentBossHP;

    public string Name => "Objective Complete";
    public string Tooltip => "Reward given when the AI kills a boss or collects an item.";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    private bool shouldStop = false;
    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }
    public double BossMultiplier { get; set; } = 5.0;
    public double DamagedBossMultiplier { get; set; } = 0.1;
    public double ItemMultiplier { get; set; } = 1.0;
    public bool StopOnObjectiveReached { get; set; } = false;

    public ExtraField[] ExtraFields { get; set; }

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new BoolFieldInfo(nameof(StopOnObjectiveReached), "Stop on objective reached", "Whether or not to stop the current save state once an objective is completed"),
        new DoubleFieldInfo(nameof(ItemMultiplier), "Item Multiplier", double.MinValue, double.MaxValue, 1.0, "Multiplier applied on top of the regular multiplier when collecting an item."),
        new DoubleFieldInfo(nameof(BossMultiplier), "Boss Multiplier", double.MinValue, double.MaxValue, 1.0, "Multiplier applied on top of the regular multiplier when a boss was killed."),
        new DoubleFieldInfo(nameof(DamagedBossMultiplier), "Damaged Boss Multiplier", double.MinValue, double.MaxValue, 1.0, "Multiplier applied on top of the regular multiplier for each point of damage a boss takes."),
    };

    public ObjectiveScoreFactor() => ExtraFields = Array.Empty<ExtraField>();

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(StopOnObjectiveReached) => StopOnObjectiveReached,
            nameof(ItemMultiplier) => ItemMultiplier,
            nameof(BossMultiplier) => BossMultiplier,
            nameof(DamagedBossMultiplier) => DamagedBossMultiplier,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(StopOnObjectiveReached): StopOnObjectiveReached = (bool)value; break;
                case nameof(ItemMultiplier): ItemMultiplier = (double)value; break;
                case nameof(BossMultiplier): BossMultiplier = (double)value; break;
                case nameof(DamagedBossMultiplier): DamagedBossMultiplier = (double)value; break;
            }
        }
    }

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        shouldStop = false;
        isInit = false;
        currentBossHP = 0;
    }

    public void Update(IDataFetcher dataFetcher) => Update((MetroidDataFetcher)dataFetcher);

    private void Update(MetroidDataFetcher df)
    {
        bool isFrozen = df.IsSamusFrozen();
        bool isBossPresent = df.IsBossPresent();

        if (!wasBossPresent && isBossPresent)
        {
            currentBossHP = df.GetEnemies().First().Health;
        }

        if (!isInit)
        {
            wasSamusFrozen = isFrozen;
            wasBossPresent = isBossPresent;

            isInit = true;
        }

        if (isBossPresent)
        {
            var newBossHP = df.GetEnemies().First().Health;
            currScore += ScoreMultiplier * DamagedBossMultiplier * (currentBossHP - newBossHP);
            currentBossHP = newBossHP;
        }

        if (isFrozen && !wasSamusFrozen)
        {
            //TODO : Test if boss mult. works
            currScore += ScoreMultiplier * (wasBossPresent ? BossMultiplier : ItemMultiplier);
            shouldStop = StopOnObjectiveReached;
        }

        wasSamusFrozen = isFrozen;
        wasBossPresent = isBossPresent;
    }

    public IScoreFactor Clone() => new ObjectiveScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier,
        IsDisabled = IsDisabled,
        StopOnObjectiveReached = StopOnObjectiveReached,
        ItemMultiplier = ItemMultiplier,
        BossMultiplier = BossMultiplier,
        DamagedBossMultiplier = DamagedBossMultiplier
    };
}
