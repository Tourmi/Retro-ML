using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Metroid.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.Metroid.Neural.Scoring;
internal class HealthScoreFactor : IScoreFactor
{
    private double currScore;
    private bool isInit = false;
    private int currHealth;

    public string Name => "Health";
    public string Tooltip => "Reward given when the AI's health changes.";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public bool ShouldStop => false;

    public double ScoreMultiplier { get; set; }
    public double GainedHealthMultiplier { get; set; } = 1.0;
    public double LostHealthMultiplier { get; set; } = -1.0;

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new DoubleFieldInfo(nameof(LostHealthMultiplier), "Lost Health Multiplier", double.MinValue, double.MaxValue, 1.0, "Multiplier applied on top of regular multiplier. Should be negative."),
        new DoubleFieldInfo(nameof(GainedHealthMultiplier), "Gained Health Multiplier", double.MinValue, double.MaxValue, 1.0, "Multiplier applied on top of regular multiplier. Should be positive.")
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(LostHealthMultiplier) => LostHealthMultiplier,
            nameof(GainedHealthMultiplier) => GainedHealthMultiplier,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(LostHealthMultiplier): LostHealthMultiplier = (double)value; break;
                case nameof(GainedHealthMultiplier): GainedHealthMultiplier = (double)value; break;
            }
        }
    }

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        isInit = false;
    }

    public void Update(IDataFetcher dataFetcher) => Update((MetroidDataFetcher)dataFetcher);

    private void Update(MetroidDataFetcher df)
    {
        int newHealth = df.GetSamusHealth();

        if (!isInit)
        {
            currHealth = newHealth;
            isInit = true;
        }

        var diff = (newHealth - currHealth);

        currScore += ScoreMultiplier * Math.Abs(diff) * (diff < 0 ? LostHealthMultiplier : GainedHealthMultiplier);

        currHealth = newHealth;
    }

    public IScoreFactor Clone() => new HealthScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier,
        IsDisabled = IsDisabled,
        LostHealthMultiplier = LostHealthMultiplier,
        GainedHealthMultiplier = GainedHealthMultiplier
    };
}
