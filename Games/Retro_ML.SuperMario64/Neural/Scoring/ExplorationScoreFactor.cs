using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMario64.Game;
using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML.SuperMario64.Neural.Scoring;
internal class ExplorationScoreFactor : IScoreFactor
{
    private int framesWithoutProgress;
    private double currScore;
    private bool shouldStop;
    private int spheresAddedWhileAirborn;
    public OctTree sceneVisited;

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new IntegerFieldInfo(nameof(MinimumDistance), "Distance to cross", 100, int.MaxValue, 100, "Distance in units that the AI should have traversed to get its reward"),
        new DoubleFieldInfo(nameof(DistanceTimeout), "Exploration Timer", 0, double.PositiveInfinity, 1, "Seconds before timing out an AI that hasn't explored the level"),
        new DoubleFieldInfo(nameof(EliminationMult), "Elimination multiplier", double.NegativeInfinity, double.PositiveInfinity, 1, "Reward multiplier applied when the AI stops exploring")
    };

    public ExplorationScoreFactor()
    {
        ExtraFields = Array.Empty<ExtraField>();
        sceneVisited = InitTree();
    }

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(MinimumDistance) => MinimumDistance,
            nameof(DistanceTimeout) => DistanceTimeout,
            nameof(EliminationMult) => EliminationMult,
            _ => (object)0,
        };

        set
        {
            switch (fieldName)
            {
                case nameof(MinimumDistance): MinimumDistance = (int)value; break;
                case nameof(DistanceTimeout): DistanceTimeout = (double)value; break;
                case nameof(EliminationMult): EliminationMult = (double)value; break;
            }
        }
    }

    public double DistanceTimeout { get; set; } = 2;
    public double EliminationMult { get; set; } = -1000;
    public int MinimumDistance { get; set; } = 1000;
    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Exploration";
    public string Tooltip => "Reward applied every time the AI traverses the specified distance.";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public ExtraField[] ExtraFields { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher) => Update((SM64DataFetcher)dataFetcher);

    private void Update(SM64DataFetcher df)
    {
        bool isMarioGrounded = df.IsMarioGrounded();
        if (isMarioGrounded) spheresAddedWhileAirborn = 0;
        var marioPos = df.GetMarioPos();
        if (sceneVisited.Contains(marioPos))
        {
            framesWithoutProgress++;
        }
        else
        {
            framesWithoutProgress = 0;
            sceneVisited.AddObject(new Sphere(marioPos, MinimumDistance));
            if (isMarioGrounded) spheresAddedWhileAirborn++;
        }

        if (DistanceTimeout > 0 && framesWithoutProgress / 60f > DistanceTimeout)
        {
            shouldStop = true;
        }
    }

    public void LevelDone()
    {
        currScore += (sceneVisited.Count - spheresAddedWhileAirborn) * ScoreMultiplier;
        if (shouldStop) currScore += ScoreMultiplier * EliminationMult;
        shouldStop = false;
        framesWithoutProgress = 0;
        sceneVisited = InitTree();
        spheresAddedWhileAirborn = 0;
    }

    public IScoreFactor Clone() => new ExplorationScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier,
        IsDisabled = IsDisabled,
        MinimumDistance = MinimumDistance,
        DistanceTimeout = DistanceTimeout
    };

    private static OctTree InitTree() => new(new Vector(7005.25f, 7000.65f, -7005.85f), 32_000f, 1f, 8, 8);
}
