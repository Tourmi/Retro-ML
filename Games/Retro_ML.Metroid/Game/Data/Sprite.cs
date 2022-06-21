using static Retro_ML.Metroid.Game.Addresses;

namespace Retro_ML.Metroid.Game.Data;

internal class Sprite
{
    public byte YPos { get; }
    public byte XPos { get; }
    public byte YRadius { get; }
    public byte XRadius { get; }
    public byte Health { get; }
    public byte DespawnTimer { get; }

    public byte Status { get; }
    public bool IsInFirstScreen { get; }

    public Sprite(byte[] baseBytes, byte[] extraBytes)
    {
        YPos = baseBytes[Sprites.PosY.Address - Sprites.AllBaseSprites.Address];
        XPos = baseBytes[Sprites.PosX.Address - Sprites.AllBaseSprites.Address];
        Health = baseBytes[Sprites.Hitpoints.Address - Sprites.AllBaseSprites.Address];
        DespawnTimer = baseBytes[Sprites.DespawnTimer.Address - Sprites.AllBaseSprites.Address];

        Status = extraBytes[Sprites.Status.Address - Sprites.AllExtraSprites.Address];
        YRadius = extraBytes[Sprites.VerticalRadius.Address - Sprites.AllExtraSprites.Address];
        XRadius = extraBytes[Sprites.HorizontalRadius.Address - Sprites.AllExtraSprites.Address];
        IsInFirstScreen = extraBytes[Sprites.NameTable.Address - Sprites.AllExtraSprites.Address] == 0;
    }

    /// <summary>
    /// True if the sprite is an enemy and is alive
    /// </summary>
    public bool IsAlive => Status != 0x00 && Health > 0;
    /// <summary>
    /// True if the sprite is a pickup
    /// </summary>
    public bool IsPickup => Status != 0x00 && DespawnTimer > 1;
    /// <summary>
    /// True if the enemy was frozen by the ice beam
    /// </summary>
    public bool IsFrozen => Status == 0x04;
}
