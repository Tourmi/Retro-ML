using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMario64.Game;

namespace Retro_ML.SuperMario64.Neural.Scoring;
internal class StarScoreFactor : IScoreFactor
{
    private bool isInit;
    private ushort prevStars;
    private double currScore;
    private bool shouldStop;

    public FieldInfo[] Fields => Array.Empty<FieldInfo>();

    public StarScoreFactor() => ExtraFields = Array.Empty<ExtraField>();

    public object this[string fieldName] { get => 0; set { } }
    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Stars collected";
    public string Tooltip => "Reward applied for each coin collected by the AI. Collecting a star stops the current save state";

    public bool CanBeDisabled => false;

    public bool IsDisabled { get; set; }

    public ExtraField[] ExtraFields { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher) => Update((SM64DataFetcher)dataFetcher);

    private void Update(SM64DataFetcher df)
    {
        var currStars = df.GetStarCount();
        if (!isInit)
        {
            prevStars = currStars;
            isInit = true;
        }

        if (currStars > prevStars)
        {
            currScore += ScoreMultiplier * (currStars - prevStars);
            shouldStop = true;
        }

        prevStars = currStars;
    }

    public void LevelDone()
    {
        shouldStop = false;
        isInit = false;
    }

    public IScoreFactor Clone() => new StarScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier
    };
}
