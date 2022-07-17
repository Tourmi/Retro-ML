using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML.SuperMario64.Game.Data;
internal class GameObject
{
    private static readonly uint[] EnemyScripts = new uint[]
    {
        0x13000054, //Mr. I main body, subtype 1 == giant Mr. I
        0x130000F8, //Mr. I projectile
        0x1300472C, //goomba
    };
    private static readonly uint[] GoodScripts = new uint[]
    {
        0x130001CC, //Cap switch button

    };
    private static readonly uint[] SolidScripts = new uint[]
    {
        0x130001AC, //Cap switch base
        0x130001CC, //Cap switch button

    };
    private static readonly uint[] OtherScripts = new uint[]
    {
        0x13002EC0, //Mario
        0x13000000, //Star door
        0x13000118, //Whomp fortress big pole
        0x13000144, //pole
    };

    private AABB? cachedAABB;
    private uint bankStart;

    public uint BehaviourScript { get; private set; }
    public uint BankAddress => ToBankAddress(BehaviourScript, bankStart);
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }
    public float Radius { get; private set; }
    public float Height { get; private set; }
    public float DownOffset { get; private set; }

    public AABB AABB
    {
        get
        {
            if (!cachedAABB.HasValue) cachedAABB = new AABB(new(X, Y - DownOffset, Z), new(Radius * 2, Height, Radius * 2));
            return cachedAABB!.Value;
        }
    }

    public GameObject(byte[] bytes, uint bankStart)
    {
        this.bankStart = bankStart;

        BehaviourScript = GetUint(bytes[0..4]);
        X = GetFloat(bytes[4..8]);
        Y = GetFloat(bytes[8..12]);
        Z = GetFloat(bytes[12..16]);
        Radius = GetFloat(bytes[16..20]);
        Height = GetFloat(bytes[20..24]);
        DownOffset = GetFloat(bytes[24..28]);
    }

    public bool IsEnemy() => IsEnemy(BehaviourScript, bankStart);

    public static bool IsEnemy(uint behaviourScript, uint bankStart) => EnemyScripts.Contains(ToBankAddress(behaviourScript, bankStart));
    private static uint ToBankAddress(uint behaviourScript, uint bankStart) => ((uint)Math.Max((behaviourScript & ~0x8000_0000) - (long)bankStart, 0)) + 0x13000000;
    private static uint GetUint(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
    private static float GetFloat(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToSingle(bytes, 0);
    }
}
