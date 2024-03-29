﻿using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring;

internal class CollisionScoreFactor : IScoreFactor
{
    private double currScore;
    private bool isInit;
    private bool wasInCollision;
    private int collisionCount;

    public string Name => "Collisions";

    public string Tooltip => "Reward applied whenever the AI collides with a wall, another racer, etc";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    private bool shouldStop = false;
    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }

    public FieldInfo[] Fields => new FieldInfo[]
    {
         new IntegerFieldInfo(nameof(StopAfterXCollisions), "Maximum collisions", 0, int.MaxValue, 1, "Stops the current race if the total collisions reach this number"),
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(StopAfterXCollisions) => StopAfterXCollisions,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(StopAfterXCollisions): StopAfterXCollisions = (int)value; break;
            }
        }
    }

    public int StopAfterXCollisions { get; set; } = 5;

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        isInit = false;
        collisionCount = 0;
        shouldStop = false;
    }

    public void Update(IDataFetcher dataFetcher)
    {
        SMKDataFetcher df = (SMKDataFetcher)dataFetcher;
        if (!isInit)
        {
            isInit = true;
            wasInCollision = df.GetCollisionTimer() > 0;
        }

        bool isInCollision = df.GetCollisionTimer() > 0;
        if (isInCollision != wasInCollision)
        {
            wasInCollision = isInCollision;

            if (isInCollision)
            {
                currScore += ScoreMultiplier;
                collisionCount++;

                if (StopAfterXCollisions > 0 && collisionCount >= StopAfterXCollisions)
                {
                    shouldStop = true;
                }
            }
        }
    }

    public IScoreFactor Clone() => new CollisionScoreFactor() { ScoreMultiplier = ScoreMultiplier, IsDisabled = IsDisabled, StopAfterXCollisions = StopAfterXCollisions };
}
