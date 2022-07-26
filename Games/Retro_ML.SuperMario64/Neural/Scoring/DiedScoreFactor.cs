using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMario64.Game;

namespace Retro_ML.SuperMario64.Neural.Scoring;
internal class DiedScoreFactor : IScoreFactor
{
    private bool shouldStop;
    private double currScore;

    public FieldInfo[] Fields => Array.Empty<FieldInfo>();

    public DiedScoreFactor() => ExtraFields = Array.Empty<ExtraField>();

    public object this[string fieldName] { get => 0; set { } }
    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Death";
    public string Tooltip => "Reward applied on AI death.";

    public bool CanBeDisabled => false;

    public bool IsDisabled { get; set; }

    public ExtraField[] ExtraFields { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher) => Update((SM64DataFetcher)dataFetcher);

    private void Update(SM64DataFetcher df)
    {
        var health = df.GetMarioHealth();
        if (health == 0 || df.HasMarioFallenOff())
        {
            currScore += ScoreMultiplier;
            shouldStop = true;
        }
    }

    public void LevelDone()
    {
        shouldStop = false;
    }

    public IScoreFactor Clone() => new DiedScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier
    };
}
