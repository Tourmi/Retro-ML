namespace Retro_ML.SuperBomberman3.Game.Data;

/// <summary>
/// So far with our analysis, its impossible to find a way in the game memory to associate a bomb planted to a certain player.
/// The game handles bombs with a first-in first-out queue (FIFO). We use this class to keep track of useful informations related to the bombs planted.
/// Using the bomb class, we can associate a bomb detonation to a certain player's death or a wall destruction.
/// </summary>
internal class Bomb
{
    /// <summary>
    /// Represent the bomb index in the game FIFO bomb queue.
    /// </summary>
    public int QueueBombIndex { get; set; } = int.MaxValue;
    /// <summary>
    /// Flag used to keep track if a bomb is expired in game.
    /// We consider a bomb as expired when the game frees its index in the FIFO bomb queue.
    /// It expires visually when the exposion disapears in game. Roughly 20 frames after detonation.
    /// </summary>
    public bool IsExpired { get; set; } = true;
    /// <summary>
    /// Flag that represent if a bomb is planted by the main player. Set to false if planted by enemies.
    /// </summary>
    public bool IsPlantedByMainPlayer { get; set; } = false;
    /// <summary>
    /// Represent the bomb X pos in Tile value.
    /// </summary>
    public uint XTilePos { get; set; } = uint.MaxValue;
    /// <summary>
    /// Represent the bomb Y pos in Tile value.
    /// </summary>
    public uint YTilePos { get; set; } = uint.MaxValue;
    /// <summary>
    /// Represent the frame when the bomb is set to expire.
    /// Act as a sort of timer.
    /// </summary>
    public uint SetToExpire { get; set; } = uint.MaxValue;
    /// <summary>
    /// Represent the frame when the bomb is set to damage players.
    /// Act as a sort of timer.
    /// </summary>
    public uint SetToDamagePlayers { get; set; } = uint.MaxValue;
    /// <summary>
    /// Represent the frame when the bomb is set to destroy walls.
    /// Act as a sort of timer.
    /// </summary>
    public uint SetToDestroyWalls { get; set; } = uint.MaxValue;
    /// <summary>
    /// Default constructor used to instanciate bombs with default values.
    /// </summary>
    public Bomb()
    {

    }
    /// <summary>
    /// Main constructor.
    /// </summary>
    public Bomb(int queueBombIndex, bool isExpired, uint yTilePos, uint xTilePos, uint setToExpire, uint setToDamagePlayers, uint setToDestroyWalls)
    {
        QueueBombIndex = queueBombIndex;
        IsExpired = isExpired;
        XTilePos = xTilePos;
        YTilePos = yTilePos;
        SetToExpire = setToExpire;
        SetToDamagePlayers = setToDamagePlayers;
        SetToDestroyWalls = setToDestroyWalls;
    }
}
