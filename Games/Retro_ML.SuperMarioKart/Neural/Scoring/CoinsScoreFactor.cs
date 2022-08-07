using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMarioKart.Game;

namespace Retro_ML.SuperMarioKart.Neural.Scoring;

internal class CoinsScoreFactor : IScoreFactor
{
    private double currScore;
    private bool isInit;
    private byte currCoins;

    public string Name => "Coins";

    public string Tooltip => "Reward applied whenever the AI picks up coins";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public bool ShouldStop => false;

    public double ScoreMultiplier { get; set; }

    public FieldInfo[] Fields => new FieldInfo[]
    {
         new DoubleFieldInfo(nameof(LosingCoinsMult), "Losing coins multiplier", double.MinValue, double.MaxValue, 0.25, "Multiplier to apply if the AI loses coins"),
    };

    public object this[string fieldName]
    {
        get => fieldName switch
        {
            nameof(LosingCoinsMult) => LosingCoinsMult,
            _ => 0,
        };
        set
        {
            switch (fieldName)
            {
                case nameof(LosingCoinsMult): LosingCoinsMult = (double)value; break;
            }
        }
    }

    public double LosingCoinsMult { get; set; } = -0.5;

    public double GetFinalScore() => currScore;

    public void LevelDone()
    {
        isInit = false;
    }

    public void Update(IDataFetcher dataFetcher)
    {
        var df = (SMKDataFetcher)dataFetcher;
        if (!isInit)
        {
            isInit = true;
            currCoins = df.GetCoins();
        }

        var newCoins = df.GetCoins();
        if (newCoins > currCoins)
        {
            currScore += (newCoins - currCoins) * ScoreMultiplier;
            currCoins = newCoins;
        }
        else if (newCoins < currCoins)
        {
            currScore += (currCoins - newCoins) * ScoreMultiplier * LosingCoinsMult;
            currCoins = newCoins;
        }
    }

    public IScoreFactor Clone() => new CoinsScoreFactor() { ScoreMultiplier = ScoreMultiplier, IsDisabled = IsDisabled, LosingCoinsMult = LosingCoinsMult };
}
