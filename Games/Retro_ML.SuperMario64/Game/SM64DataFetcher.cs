using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.SuperMario64.Game.Data;
using Retro_ML.Utils;
using Retro_ML.Utils.Game.Geometry3D;
using static Retro_ML.SuperMario64.Game.Addresses;

namespace Retro_ML.SuperMario64.Game;

/// <summary>
/// Takes care of abstracting away the addresses when communicating with the emulator.
/// </summary>
internal class SM64DataFetcher : IDataFetcher
{
    private const ushort COLLISION_TRI_SIZE = 0x30;
    private const short MAXIMUM_VERTICAL_SPEED = 75;

    private readonly IEmulatorAdapter emulator;
    private readonly Dictionary<uint, byte[]> frameCache;
    private readonly Dictionary<uint, byte[]> levelCache;
    private readonly SM64PluginConfig pluginConfig;
    private readonly InternalClock internalClock;
    private readonly List<IRaytracable> solidCollisionCache;
    private OctTree scene;

    private uint prevTriListPtr;

    public SM64DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SM64PluginConfig pluginConfig)
    {
        this.emulator = emulator;
        frameCache = new();
        levelCache = new();
        solidCollisionCache = new();
        this.pluginConfig = pluginConfig;
        internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);
        scene = InitNewScene();
    }

    /// <summary>
    /// Needs to be called every frame to reset the memory cache
    /// </summary>
    public void NextFrame()
    {
        uint trisPointer = (uint)ReadULong(Collision.TrianglesListPointer);
        prevTriListPtr = trisPointer;

        frameCache.Clear();
        internalClock.NextFrame();
        int currPrio = 1;
        DebugInfo.AddInfo("Mario X Pos", GetMarioX().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Y Pos", GetMarioY().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Z Pos", GetMarioZ().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario X Speed", GetMarioXSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Y Speed", GetMarioYSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Z Speed", GetMarioZSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Horizontal Speed", GetMarioHorizontalSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Ground Offset", GetMarioGroundOffset().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Facing Angle", GetMarioFacingAngle().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Health", GetMarioHealth().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Cap Status", Convert.ToString(GetMarioCapFlags(), 2).PadLeft(16, '0').Insert(8, " "), "Mario", currPrio++);
        DebugInfo.AddInfo("Coin Count", GetCoinCount().ToString(), "Collected", currPrio++);
        DebugInfo.AddInfo("Star Count", GetStarCount().ToString(), "Collected", currPrio++);

        if (HasLevelChanged())
        {
            _ = InitNewScene();
        }
        scene.Update();
        scene.AddObjects(GetDynamicCollision().ToArray());

        InitFrameCache();
    }

    /// <summary>
    /// Needs to be called every time a save state was loaded to reset the global cache.
    /// </summary>
    public void NextState()
    {
        frameCache.Clear();
        _ = InitNewScene();

        internalClock.Reset();
        prevTriListPtr = (uint)ReadULong(Collision.TrianglesListPointer);
    }

    public bool[,] GetInternalClockState() => internalClock.GetStates();
    public float GetMarioX() => ReadFloat(Mario.XPos);
    public float GetMarioY() => ReadFloat(Mario.YPos);
    public float GetMarioZ() => ReadFloat(Mario.ZPos);
    public float GetMarioXSpeed() => ReadFloat(Mario.XSpeed);
    public float GetMarioYSpeed() => ReadFloat(Mario.YSpeed);
    public float GetMarioZSpeed() => ReadFloat(Mario.ZSpeed);
    public float GetMarioHorizontalSpeed() => ReadFloat(Mario.HorizontalSpeed);
    public ushort GetMarioFacingAngle() => (ushort)ReadULong(Mario.FacingAngle);
    public float GetMarioFacingAngleRadian() => MathF.Tau * (-(GetMarioFacingAngle() / (float)ushort.MaxValue));
    public ushort GetMarioCapFlags() => (ushort)ReadULong(Mario.HatFlags);
    public byte GetMarioHealth() => ReadByte(Mario.Health);
    public sbyte GetMarioGroundOffset() => (sbyte)ReadByte(Mario.GroundOffset);
    public ushort GetCoinCount() => (ushort)ReadULong(Mario.Coins);
    public ushort GetStarCount() => (ushort)ReadULong(Progress.StarCount);
    public uint GetBehaviourBankStart() => (uint)ReadULong(GameObjects.BehaviourBankStartAddress);

    public ushort GetStaticTriangleCount() => (ushort)ReadULong(Collision.StaticTriangleCount);
    public ushort GetTriangleCount() => (ushort)ReadULong(Collision.TotalTriangleCount);
    public ushort GetDynamicTriangleCount() => (ushort)(GetTriangleCount() - GetStaticTriangleCount());

    public Ray GetMarioForwardRay() => new(new(GetMarioX(), GetMarioY() + 110, GetMarioZ()), Vector.Origin.WithZ(1).RotateXZ(GetMarioFacingAngleRadian()));

    public IEnumerable<IRaytracable> GetCollision()
    {
        yield return scene;
    }

    public bool HasLevelChanged()
    {
        uint trisPointer = (uint)ReadULong(Collision.TrianglesListPointer);
        return trisPointer != 0 && trisPointer != prevTriListPtr;
    }

    /// <summary>
    /// Returns the level's static triangles
    /// </summary>
    public IEnumerable<IRaytracable> GetStaticCollision()
    {
        if (!solidCollisionCache.Any())
        {
            ushort staticTriCount = GetStaticTriangleCount();
            uint pointer = (uint)ReadULong(Collision.TrianglesListPointer);
            if (pointer == 0) return Enumerable.Empty<IRaytracable>();

            var bytes = Read(new AddressData(pointer, (ushort)(staticTriCount * COLLISION_TRI_SIZE), AddressData.CacheDurations.Level));

            var tris = new List<CollisionTri>();
            for (int i = 0; i < bytes.Length; i += COLLISION_TRI_SIZE)
            {
                tris.Add(new(bytes[i..(i + COLLISION_TRI_SIZE)]));
            }

            solidCollisionCache.AddRange(tris.Select(s => (IRaytracable)s.Triangle));
        }
        return solidCollisionCache;
    }

    /// <summary>
    /// Returns the level's dynamic collision triangles
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IRaytracable> GetDynamicCollision()
    {
        ushort staticTriCount = GetStaticTriangleCount();
        ushort dynamicTriCount = GetDynamicTriangleCount();
        uint pointer = (uint)ReadULong(Collision.TrianglesListPointer);

        if (pointer == 0) return Enumerable.Empty<IRaytracable>();

        var bytes = Read(new AddressData(pointer + (uint)(staticTriCount * COLLISION_TRI_SIZE), (ushort)(dynamicTriCount * COLLISION_TRI_SIZE), AddressData.CacheDurations.Frame));
        var tris = new List<CollisionTri>();
        for (int i = 0; i < bytes.Length; i += COLLISION_TRI_SIZE)
        {
            tris.Add(new(bytes[i..(i + COLLISION_TRI_SIZE)], isStatic: false));
        }

        return tris.Select(t => (IRaytracable)t.Triangle);
    }

    public IEnumerable<IRaytracable> GetEnemyHitboxes() => GetObjects().Where(o => o.IsEnemy()).Select(o => (IRaytracable)o.AABB);

    public IEnumerable<IRaytracable> GetGoodieHitboxes() => GetObjects().Where(o => o.IsGoodie()).Select(o => (IRaytracable)o.AABB);

    public IEnumerable<GameObject> GetObjects()
    {
        var actives = ReadMultiple(GameObjects.Active, GameObjects.SingleGameObject, GameObjects.AllGameObjects).ToArray();
        byte activeCount = (byte)Array.IndexOf(actives, (byte)0);
        if (activeCount > 240) activeCount = 240;
        uint bankStart = GetBehaviourBankStart();

        uint totalBytes = GameObjects.SingleGameObject.Length * activeCount;
        var addressesToRead = new AddressData[]
        {
            GameObjects.BehaviourAddress,
            GameObjects.XPos,
            GameObjects.YPos,
            GameObjects.ZPos,
            GameObjects.HitboxRadius,
            GameObjects.HitboxHeight,
            GameObjects.HitboxDownOffset
        };
        var bytesPerObject = (int)addressesToRead.Sum(a => a.Length);

        var addresses = GetCalculatedAddresses(totalBytes, GameObjects.SingleGameObject.Length, addressesToRead);

        var objectsBytes = Read(addresses.ToArray());
        List<GameObject> objects = new();
        for (int i = 0; i < objectsBytes.Length; i += bytesPerObject)
        {
            objects.Add(new GameObject(objectsBytes[i..(i + bytesPerObject)], bankStart));
        }

        return objects;
    }

    private OctTree InitNewScene()
    {
        levelCache.Clear();
        solidCollisionCache.Clear();

        scene = new(new Vector(7005.25f, 7000.65f, -7005.85f), 32_000, 100f, 8, 2);
        scene.AddObjects(GetStaticCollision().ToArray());
        return scene;
    }

    /// <summary>
    /// Reads a single byte from the emulator's memory
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private byte ReadByte(AddressData addressData) => Read(addressData)[0];

    /// <summary>
    /// Reads up to 8 bytes from the address, assuming big endian.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private ulong ReadULong(AddressData addressData) => ToULong(Read(addressData));

    private static ulong ToULong(byte[] bytes)
    {
        ulong value = 0;
        for (int i = 0; i < bytes.Length && i < 8; i++)
        {
            value += (ulong)bytes[i] << (bytes.Length - i - 1) * 8;
        }
        return value;
    }

    /// <summary>
    /// Reads 4 bytes from the address into a float number.
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private float ReadFloat(AddressData addressData)
    {
        var bytes = Read(addressData).ToArray();
        //We need to reverse the bytes if the current system is little endian
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

        return BitConverter.ToSingle(bytes);
    }

    /// <summary>
    /// Reads a specific amount of bytes from the emulator's memory, using the AddressData
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private byte[] Read(AddressData addressData)
    {
        var cacheToUse = GetCacheToUse(addressData);
        if (!cacheToUse.ContainsKey(addressData.Address))
            cacheToUse[addressData.Address] = emulator.ReadMemory(addressData.Address, addressData.Length);

        return cacheToUse[addressData.Address];
    }

    /// <summary>
    /// Reads into multiple groups of bytes according to the given offset and total byte sizes.
    /// </summary>
    /// <param name="addressData"></param>
    /// <param name="offset"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    private IEnumerable<byte[]> ReadMultiple(AddressData addressData, AddressData offset, AddressData total)
    {
        var toRead = GetAddressesToRead(addressData, offset, total);
        var result = Read(toRead.ToArray());
        var bytesPerItem = addressData.Length;
        for (long i = 0; i < result.Length; i += bytesPerItem)
        {
            var bytes = new byte[bytesPerItem];
            Array.Copy(result, i, bytes, 0, bytesPerItem);
            yield return bytes;
        }
    }

    /// <summary>
    /// Returns the addresses to read when reading from a RAM Table
    /// </summary>
    /// <param name="addressData"></param>
    /// <param name="offset"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    private static IEnumerable<AddressData> GetAddressesToRead(AddressData addressData, AddressData offset, AddressData total)
    {
        uint count = total.Length / offset.Length;
        for (int i = 0; i < count; i++)
        {
            yield return new AddressData((uint)(addressData.Address + i * offset.Length), addressData.Length, addressData.CacheDuration);
        }
    }

    /// <summary>
    /// Reads multiple ranges of addresses
    /// </summary>
    /// <param name="addresses"></param>
    /// <returns></returns>
    private byte[] Read(params AddressData[] addresses)
    {
        List<(uint addr, uint length)> toFetch = new();

        uint totalBytes = 0;

        foreach (var address in addresses)
        {
            var cacheToUse = GetCacheToUse(address);
            if (!cacheToUse.ContainsKey(address.Address))
                toFetch.Add((address.Address, address.Length));

            totalBytes += address.Length;
        }

        byte[] data = Array.Empty<byte>();
        if (toFetch.Count > 0)
            data = emulator.ReadMemory(toFetch.ToArray());

        List<byte> bytes = new();
        int dataIndex = 0;
        foreach (AddressData address in addresses)
        {
            int count = (int)address.Length;

            var cacheToUse = GetCacheToUse(address);
            if (!cacheToUse.ContainsKey(address.Address))
            {
                cacheToUse[address.Address] = data[dataIndex..(dataIndex + count)];
                dataIndex += count;
            }

            bytes.AddRange(cacheToUse[address.Address]);
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// Which cache to use depending on the AddressData
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData) => addressData.CacheDuration switch
    {
        AddressData.CacheDurations.Frame => frameCache,
        AddressData.CacheDurations.Level => levelCache,
        _ => frameCache,
    };

    private void InitFrameCache()
    {
        List<AddressData> toRead = new()
        {
            Mario.XPos,
            Mario.YPos,
            Mario.ZPos,
            Mario.XSpeed,
            Mario.YSpeed,
            Mario.ZSpeed,
            Mario.HorizontalSpeed,
            Mario.Coins,
            Mario.HatFlags,
            Progress.StarCount
        };

        _ = Read(toRead.ToArray());
    }

    private static IEnumerable<AddressData> GetCalculatedAddresses(uint totalLength, uint offset, params AddressData[] baseAddresses)
    {
        for (uint i = 0; i < totalLength; i += offset)
        {
            for (int j = 0; j < baseAddresses.Length; j++)
            {
                yield return new AddressData()
                {
                    Address = baseAddresses[j].Address + i,
                    CacheDuration = baseAddresses[j].CacheDuration,
                    Length = baseAddresses[j].Length
                };
            }
        }
    }
}
