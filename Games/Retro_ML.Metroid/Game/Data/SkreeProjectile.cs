namespace Retro_ML.Metroid.Game.Data;

internal class SkreeProjectile
{
    public byte DespawnTimer { get; }
    public byte XPos { get; }
    public byte YPos { get; }
    public bool IsInFirstScreen { get; }

    public SkreeProjectile(byte[] bytes)
    {
        DespawnTimer = bytes[0];
        YPos = bytes[1];
        XPos = bytes[2];
        IsInFirstScreen = bytes[3] == 0;
    }

    public bool IsActive()
    {
        return DespawnTimer != 0;
    }

    public static IEnumerable<SkreeProjectile> FromBytes(byte[] bytes)
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new SkreeProjectile(bytes[(i * 4)..(i * 4 + 4)]);
        }
    }
}
