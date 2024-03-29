﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Metroid.Game;
using Retro_ML.Metroid.Game.Navigation;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.Metroid.Neural.Scoring;

internal class ProgressScoreFactor : IScoreFactor
{
    private const byte DOOR_ENTER_Y = 113;
    private const byte DOOR_ENTER_Y_MORPH_BALL = 120;

    private double currScore;
    private double maxScore;
    private bool isInit = false;
    private byte previousX;
    private byte previousY;
    private int framesWithoutProgress;
    public string Name => "Progression";
    public string Tooltip => "Reward given for each tile traversed towards the current navigation direction";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    private bool shouldStop = false;
    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }
    public double MaximumTimeWithoutProgress { get; set; } = 10.0;

    public FieldInfo[] Fields => new FieldInfo[]
    {
        new DoubleFieldInfo(nameof(MaximumTimeWithoutProgress), "Maximum time without progress", 0, double.MaxValue, 1.0, "Stops the current save state after this amount of consecutive seconds without any progress.")
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(MaximumTimeWithoutProgress) => MaximumTimeWithoutProgress,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(MaximumTimeWithoutProgress): MaximumTimeWithoutProgress = (double)value; break;
            }
        }
    }

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        shouldStop = false;
        isInit = false;
    }

    public void Update(IDataFetcher dataFetcher) => Update((MetroidDataFetcher)dataFetcher);

    private void Update(MetroidDataFetcher df)
    {
        byte currX = df.GetSamusXPosition();
        byte currY = df.GetSamusYPosition();

        if (!isInit)
        {
            previousX = currX;
            previousY = currY;

            framesWithoutProgress = 0;

            isInit = true;
        }

        //If Samus is near door, pretend she was on the ground, and don't count progress.
        if (df.IsSamusInDoor() || df.IsInFrontOfDoor())
        {
            previousY = df.IsSamusInMorphBall() ? DOOR_ENTER_Y_MORPH_BALL : DOOR_ENTER_Y;
            currY = previousY;
            previousX = currX;
        }

        //If samus isn't grounded, don't count vertical progress
        if (!df.IsSamusGrounded())
        {
            currY = previousY;
        }

        double distX = currX - previousX;
        double distY = currY - previousY;

        //correct the distance if there was an overflow/underflow
        if (distX > 128) distX -= byte.MaxValue;
        if (distX < -128) distX += byte.MaxValue;
        if (distY > 128) distY -= byte.MaxValue;
        if (distY < -128) distY += byte.MaxValue;

        var navigationDirection = df.GetNavigationDirection();

        if (df.GetNavigationObjective() == Objectives.Obtain)
        {
            var powerup = df.GetPowerups().Where(p => (p[3] == 0) == df.IsSamusInFirstScreen()).SingleOrDefault();
            if (powerup != null)
            {
                var dirr = df.GetDirectionToNearestGoodTile();
                navigationDirection[0, 0] = dirr[0, 0];
                navigationDirection[0, 1] = 0;
            }
        }

        currScore += ScoreMultiplier / 16.0 * (distX * navigationDirection[0, 0] + distY * navigationDirection[0, 1]);

        if (df.CanSamusAct())
        {
            framesWithoutProgress = currScore <= maxScore ? framesWithoutProgress + 1 : 0;
        }

        maxScore = Math.Max(currScore, maxScore);

        if (MaximumTimeWithoutProgress != 0 && framesWithoutProgress > MaximumTimeWithoutProgress * 60.0)
        {
            shouldStop = true;
        }

        previousX = currX;
        previousY = currY;
    }

    public IScoreFactor Clone() => new ProgressScoreFactor()
    {
        ScoreMultiplier = ScoreMultiplier,
        IsDisabled = IsDisabled,
        MaximumTimeWithoutProgress = MaximumTimeWithoutProgress
    };
}
