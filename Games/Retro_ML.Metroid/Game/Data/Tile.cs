namespace Retro_ML.Metroid.Game.Data;
internal static class Tile
{
    private const byte AIR = 0xFF;
    private const byte DOOR = 0x4E;
    private const byte REGULAR_DOOR_FRAME = 0xA0;
    private const byte MISSILE_DOOR_FRAME = 0xA1;
    private const byte BUSH_1 = 0xCD;
    private const byte BUSH_2 = 0xCE;
    private const byte BUSH_3 = 0xCF;
    private const byte BUSH_4 = 0xD0;
    private const byte BUSH_5 = 0xD1;
    private const byte BUSH_6 = 0xD2;
    private const byte BUSH_7 = 0xD3;
    private const byte BUSH_8 = 0xD4;

    private static readonly byte[] passthroughTiles = new byte[] { AIR, DOOR, REGULAR_DOOR_FRAME, MISSILE_DOOR_FRAME, BUSH_1, BUSH_2, BUSH_3, BUSH_4, BUSH_5, BUSH_6, BUSH_7, BUSH_8 };
    private static readonly byte[] doorTiles = new byte[] { DOOR };
    private static readonly byte[] doorFrameTiles = new byte[] { REGULAR_DOOR_FRAME, MISSILE_DOOR_FRAME };

    public static bool IsPassthrough(byte tile) => passthroughTiles.Contains(tile);
    public static bool IsSolid(byte tile) => !IsPassthrough(tile);
    public static bool IsDoor(byte tile) => doorTiles.Contains(tile);
    public static bool IsDoorFrame(byte tile) => doorFrameTiles.Contains(tile);
    public static bool IsRegularDoor(byte tile) => tile == REGULAR_DOOR_FRAME;
    public static bool IsMissileDoor(byte tile) => tile == MISSILE_DOOR_FRAME;
}
