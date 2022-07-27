using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMario64.Game;
using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML.SuperMario64.Neural.Scoring;
internal class DistanceToStarScoreFactor : IScoreFactor
{
    private bool isInit;
    private double currScore;
    private float prevDistance;
    private float minDistance;
    private int noProgressFrames;
    private bool shouldStop;
    private Vector starPos;

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new DoubleFieldInfo(nameof(GettingFurtherMult), "Getting further away multiplier", double.NegativeInfinity, double.PositiveInfinity, 0.5, "Multiplier for when the AI gets further away from a star."),
        new DoubleFieldInfo(nameof(DistanceTimeout), "Distance Timeout", 0, double.PositiveInfinity, 1, "Amount of time in seconds before timing out an AI that hasn't gotten closer to the star")
    };

    public DistanceToStarScoreFactor() => ExtraFields = Array.Empty<ExtraField>();

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(GettingFurtherMult) => GettingFurtherMult,
            nameof(DistanceTimeout) => DistanceTimeout,
            _ => 0
        };
        set
        {
            switch (fieldName)
            {
                case nameof(GettingFurtherMult): GettingFurtherMult = (double)value; break;
                case nameof(DistanceTimeout): DistanceTimeout = (double)value; break;
            }
        }
    }

    public double GettingFurtherMult { get; set; } = 0.5;
    public double DistanceTimeout { get; set; } = 5;
    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Distance to star";
    public string Tooltip => "Reward applied for each unit that the AI gets closer by to the current mission's star.";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public ExtraField[] ExtraFields { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher) => Update((SM64DataFetcher)dataFetcher);

    private void Update(SM64DataFetcher df)
    {
        var currStarPos = df.GetMissionStarPos();
        if (!isInit)
        {
            if (!float.IsFinite(currStarPos.SquaredLength)) return;
            starPos = currStarPos;
            var initDist = (starPos - df.GetMarioPos()).Length;
            prevDistance = initDist;
            minDistance = initDist;
            isInit = true;
            noProgressFrames = 0;
        }
        var currDist = (starPos - df.GetMarioPos()).Length;

        bool gotCloser = currDist < minDistance;
        bool gotFurther = currDist > prevDistance;
        if (gotCloser)
        {
            currScore += (minDistance - currDist) * ScoreMultiplier;
        }
        else if (gotFurther)
        {
            currScore += (prevDistance - currDist) * ScoreMultiplier * GettingFurtherMult;
        }

        if (!gotCloser)
        {
            noProgressFrames++;

            if (DistanceTimeout > 0 && noProgressFrames / 60f > DistanceTimeout)
            {
                shouldStop = true;
            }
        }

        prevDistance = currDist;
        minDistance = MathF.Min(minDistance, currDist);
    }

    public void LevelDone()
    {
        isInit = false;
        shouldStop = false;
    }

    public IScoreFactor Clone() => new DistanceToStarScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier,
        IsDisabled = IsDisabled,
        GettingFurtherMult = GettingFurtherMult,
        DistanceTimeout = DistanceTimeout
    };
}
