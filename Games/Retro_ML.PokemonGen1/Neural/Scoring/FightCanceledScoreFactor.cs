using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.PokemonGen1.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.PokemonGen1.Neural.Scoring;
internal class FightCanceledScoreFactor : IScoreFactor
{
    private bool shouldStop = false;
    private double currScore;

    public FieldInfo[] Fields => new FieldInfo[]
    {
    };

    public object this[string fieldName] { 
        get => 0; 
        set { } 
    }

    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }

    public string Name => "Fight Cancelled";

    public string Tooltip => "Reward applied whenever the fight is cancelled(ex: teleport)";

    public bool CanBeDisabled => false;

    public bool IsDisabled { get => false; set { } }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher)
    {
        Update((PokemonDataFetcher)dataFetcher);
    }

    private void Update(PokemonDataFetcher dataFetcher)
    {
        if (!dataFetcher.InFight())
        {
            shouldStop = true;
            currScore += ScoreMultiplier;
        }
    }

    public void LevelDone()
    {
        shouldStop = false;
    }

    public IScoreFactor Clone()
    {
        return new FightCanceledScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier};
    }
}
