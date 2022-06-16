using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.Metroid.Neural.Scoring;

internal class TimeTakenScoreFactor : IScoreFactor
{
    public const string MAXIMUM_TRAINING_TIME = "Maximum Training Time";

    private bool shouldStop = false;
    private double currScore;
    private int levelFrames = 0;

    public FieldInfo[] Fields => new FieldInfo[]
    {
         new DoubleFieldInfo(nameof(MaximumTrainingTime), "Maximum Training Time", 30.0, double.MaxValue, 1.0, "Stop the current save state after this amount of seconds.")
    };

    public TimeTakenScoreFactor()
    {
        ExtraFields = Array.Empty<ExtraField>();
    }

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(MaximumTrainingTime) => MaximumTrainingTime,
            _ => 0,
        };

        set
        {
            switch (fieldName)
            {
                case nameof(MaximumTrainingTime): MaximumTrainingTime = (double)value; break;
            }
        }
    }

    /// <summary>
    /// Time in seconds before stopping the current save state
    /// </summary>
    public double MaximumTrainingTime { get; set; } = 300;

    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Time taken";
    public string Tooltip => "Reward applied for each second spent in the current save state.";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public ExtraField[] ExtraFields { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher)
    {
        levelFrames++;
        currScore += ScoreMultiplier / 60.0;
        if (levelFrames >= MaximumTrainingTime * 60)
        {
            shouldStop = true;
        }
    }

    public void LevelDone()
    {
        shouldStop = false;
        levelFrames = 0;
    }

    public IScoreFactor Clone() => new TimeTakenScoreFactor()
    {
        IsDisabled = IsDisabled,
        ScoreMultiplier = ScoreMultiplier,
        MaximumTrainingTime = MaximumTrainingTime
    };
}
