using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring;

internal class FinishedRaceScoreFactor : IScoreFactor
{
    private double currScore;

    public string Name => "Finished race";

    public string Tooltip => "Reward applied when the driver finishes a race";

    public bool CanBeDisabled => false;

    public bool IsDisabled { get => false; set { } }

    private bool shouldStop = false;
    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }
    public ExtraField[] ExtraFields { get; set; }

    public FieldInfo[] Fields => new FieldInfo[]
    {
         new DoubleFieldInfo(nameof(RankingMult), "Ranking multiplier", double.MinValue, double.MaxValue, 0.25, "Gives additional points in Grand Prix mode depending on the AI's placing at the end of the race"),
    };

    public FinishedRaceScoreFactor()
    {
        ExtraFields = Array.Empty<ExtraField>();
    }

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(RankingMult) => RankingMult,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(RankingMult): RankingMult = (double)value; break;
            }
        }
    }


    public double RankingMult { get; set; } = 0.5;

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        shouldStop = false;
    }

    public void Update(IDataFetcher dataFetcher)
    {
        var df = (SMKDataFetcher)dataFetcher;
        if (df.GetCurrentLap() >= 5)
        {
            shouldStop = true;
            double rankingMultiplier = 0;
            if (df.IsRace())
            {
                rankingMultiplier = (8 - df.GetRanking()) * RankingMult;
            }
            currScore += ScoreMultiplier * (1 + rankingMultiplier);
        }
    }

    public IScoreFactor Clone()
    {
        return new FinishedRaceScoreFactor() { ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields, RankingMult = RankingMult };
    }
}
