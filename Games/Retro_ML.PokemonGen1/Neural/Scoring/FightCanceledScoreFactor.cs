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

    public FightCanceledScoreFactor()
    {
        ExtraFields = Array.Empty<ExtraField>();
    }

    public object this[string fieldName]
    {
        get
        {
            return fieldName switch
            {
                _ => 0,
            };
        }
        set
        {
            switch (fieldName)
            {
            }
        }
    }

    public bool ShouldStop => shouldStop;

    public double ScoreMultiplier { get; set; }

    public string Name => "Fight Cancelled";

    public string Tooltip => "Reward applied whenever the fight is cancelled(ex: teleport)";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public ExtraField[] ExtraFields { get; set; }

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
        return new FightCanceledScoreFactor() { IsDisabled = IsDisabled, ScoreMultiplier = ScoreMultiplier, ExtraFields = ExtraFields };
    }
}
