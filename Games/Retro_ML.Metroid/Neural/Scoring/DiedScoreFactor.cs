using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Metroid.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.Metroid.Neural.Scoring;
internal class DiedScoreFactor : IScoreFactor
{
    private double currScore;

    public string Name => "Died";

    public string Tooltip => "Reward given when the AI dies.";

    public bool CanBeDisabled => false;

    public bool IsDisabled { get; set; }

    private bool shouldStop = false;
    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }

    public ExtraField[] ExtraFields { get; set; }

    public FieldInfo[] Fields => Array.Empty<FieldInfo>();

    public DiedScoreFactor() => ExtraFields = Array.Empty<ExtraField>();

    public object this[string fieldName]
    {
        get => 0;
        set { }
    }

    public double GetFinalScore() => currScore;

    public void LevelDone() => shouldStop = false;

    public void Update(IDataFetcher dataFetcher) => Update((MetroidDataFetcher)dataFetcher);

    private void Update(MetroidDataFetcher df)
    {
        if (df.GetSamusHealth() == 0)
        {
            currScore += ScoreMultiplier;
            shouldStop = true;
        }
    }

    public IScoreFactor Clone() => new HealthScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier,
        IsDisabled = IsDisabled
    };
}
