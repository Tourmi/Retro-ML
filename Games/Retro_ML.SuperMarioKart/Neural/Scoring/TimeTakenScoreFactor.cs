﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.SuperMarioKart.Neural.Scoring;

internal class TimeTakenScoreFactor : IScoreFactor
{
    private bool shouldStop = false;
    private double currScore;
    private int levelFrames = 0;

    public FieldInfo[] Fields => new FieldInfo[]
    {
         new DoubleFieldInfo(nameof(MaximumRaceTime), "Maximum Race Time", 30.0, double.MaxValue, 1.0, "Maximum time the race can take, in seconds")
    };

    public object this[string fieldName]
    {
        get
        {
            return fieldName switch
            {
                nameof(MaximumRaceTime) => MaximumRaceTime,
                _ => 0,
            };
        }
        set
        {
            switch (fieldName)
            {
                case nameof(MaximumRaceTime): MaximumRaceTime = (double)value; break;
            }
        }
    }

    public double MaximumRaceTime { get; set; } = 240;

    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Time taken";

    public string Tooltip => "Reward applied every second of the current race";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher)
    {
        levelFrames++;
        currScore += ScoreMultiplier / 60.0;
        if (levelFrames >= MaximumRaceTime * 60)
        {
            shouldStop = true;
        }
    }

    public void LevelDone()
    {
        shouldStop = false;
        levelFrames = 0;
    }

    public IScoreFactor Clone()
    {
        return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, MaximumRaceTime = MaximumRaceTime };
    }
}
