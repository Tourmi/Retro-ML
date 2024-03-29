﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperBomberman3.Game;

namespace Retro_ML.SuperBomberMan3.Neural.Scoring;

internal class TimeTakenScoreFactor : IScoreFactor
{
    private bool shouldStop = false;
    private double currScore;
    private int maxTime;
    private bool isInit;
    private int timeRemaining;
    private bool hasWon;

    public FieldInfo[] Fields => Array.Empty<FieldInfo>();

    public object this[string fieldName]
    {
        get => 0;
        set { }
    }

    public bool ShouldStop => shouldStop;
    public double ScoreMultiplier { get; set; }

    public string Name => "Time taken";

    public string Tooltip => "Reward applied for every second that the ai survived";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher)
    {
        Update((SB3DataFetcher)dataFetcher);
    }

    private void Update(SB3DataFetcher dataFetcher)
    {
        if (!isInit)
        {
            isInit = true;
            maxTime = dataFetcher.GetRemainingRoundTime();
        }

        timeRemaining = dataFetcher.GetRemainingRoundTime();
        hasWon = dataFetcher.IsRoundWon();
    }

    public void LevelDone()
    {
        shouldStop = false;
        isInit = false;

        //If the player win, reward him for the win + encourage him to win in the shortest time possible.
        if (hasWon)
        {
            currScore += ScoreMultiplier * (maxTime + timeRemaining);
        }

        //Else, reward him for surviving.
        else
        {
            currScore += ScoreMultiplier * (maxTime - timeRemaining);
        }
    }

    public IScoreFactor Clone()
    {
        return new TimeTakenScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier };
    }
}
