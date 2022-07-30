using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.SuperMario64.Game;

namespace Retro_ML.SuperMario64.Neural.Scoring;
internal class CoinScoreFactor : IScoreFactor
{
    private bool isInit;
    private ushort prevCoins;
    private double currScore;

    public FieldInfo[] Fields => Array.Empty<FieldInfo>();

    public object this[string fieldName] { get => 0; set { } }
    public bool ShouldStop => false;
    public double ScoreMultiplier { get; set; }

    public string Name => "Coins collected";
    public string Tooltip => "Reward applied for each coin collected by the AI.";

    public bool CanBeDisabled => true;

    public bool IsDisabled { get; set; }

    public double GetFinalScore() => currScore;

    public void Update(IDataFetcher dataFetcher) => Update((SM64DataFetcher)dataFetcher);

    private void Update(SM64DataFetcher df)
    {
        var currCoins = df.GetCoinCount();
        if (!isInit)
        {
            prevCoins = currCoins;
            isInit = true;
        }

        if (currCoins > prevCoins)
        {
            currScore += ScoreMultiplier * (currCoins - prevCoins);
        }

        prevCoins = currCoins;
    }

    public void LevelDone()
    {
        isInit = false;
    }

    public IScoreFactor Clone() => new CoinScoreFactor()
    {
        IsDisabled = IsDisabled,
        ScoreMultiplier = ScoreMultiplier
    };
}
