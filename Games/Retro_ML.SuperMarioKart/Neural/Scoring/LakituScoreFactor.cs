using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring;

internal class LakituScoreFactor : IScoreFactor
{
    private double currScore;
    private bool isInit;
    private byte currStatus;
    private int fellCount;

    public string Name => "Lakitu";

    public string Tooltip => "Reward applied whenever the AI puts itself in a situation to get picked up by Lakitu";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    private bool shouldStop = false;
    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }

    public FieldInfo[] Fields => new FieldInfo[]
    {
         new IntegerFieldInfo(nameof(StopAfterXFalls), "Stop after X falls", 0, int.MaxValue, 1, "Stops the current race if the AI falls off-track this amount of times"),
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(StopAfterXFalls) => StopAfterXFalls,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(StopAfterXFalls): StopAfterXFalls = (int)value; break;
            }
        }
    }

    public int StopAfterXFalls { get; set; } = 1;

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        isInit = false;
        fellCount = 0;
        shouldStop = false;
    }

    public void Update(IDataFetcher dataFetcher)
    {
        SMKDataFetcher df = (SMKDataFetcher)dataFetcher;
        if (!isInit)
        {
            isInit = true;
            currStatus = df.GetKartStatus();
        }

        byte newStatus = df.GetKartStatus();
        if (newStatus != currStatus)
        {
            currStatus = newStatus;

            if (newStatus == 0x4 || newStatus == 0x6 || newStatus == 0x8)
            {
                currScore += ScoreMultiplier;
                fellCount++;

                if (StopAfterXFalls > 0 && fellCount >= StopAfterXFalls)
                {
                    shouldStop = true;
                }
            }
        }
    }

    public IScoreFactor Clone() => new LakituScoreFactor() { ScoreMultiplier = ScoreMultiplier, IsDisabled = IsDisabled, StopAfterXFalls = StopAfterXFalls };
}
