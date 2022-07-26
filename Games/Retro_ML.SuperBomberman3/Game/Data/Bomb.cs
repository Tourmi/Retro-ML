namespace Retro_ML.SuperBomberman3.Game.Data;
internal class Bomb
{
    public uint BombTimer { get; set; } = uint.MaxValue;
    public bool IsExpired { get; set; } = true;
    public bool IsPlantedByPlayer = false;
    public int BombIndex = int.MaxValue;
    public uint XTilePos = uint.MaxValue;
    public uint YTilePos = uint.MaxValue;
    public uint SetToExpire = uint.MaxValue;
    public uint SetToKill = uint.MaxValue;
    public uint SetToDestroy = uint.MaxValue;
    public Bomb()
    {

    }
    public Bomb(int bombIndex, uint bombTimer, uint setToExpire, uint setToKill, uint setToDestroy, bool isExpired, uint xTilePos, uint yTilePos)
    {
        BombTimer = bombTimer;
        IsExpired = isExpired;
        XTilePos = xTilePos;
        YTilePos = yTilePos;
        BombIndex = bombIndex;
        SetToExpire = setToExpire;
        SetToDestroy = setToDestroy;
        SetToKill = setToKill;
    }
}
