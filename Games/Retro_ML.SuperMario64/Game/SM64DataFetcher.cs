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
    private const short MAXIMUM_HORIZONTAL_SPEED = 87;

    private readonly IEmulatorAdapter emulator;
    private readonly Dictionary<uint, byte[]> frameCache;
    private readonly InternalClock internalClock;
    private OctTree scene;

    private byte prevAreaCode;
    private byte currAreaCode;
    private int loadTrisTimer;
    private float outOfBoundsHeight = short.MinValue;

    public SM64DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SM64PluginConfig pluginConfig)
    {
        this.emulator = emulator;
        frameCache = new();
        internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);
        scene = InitNewScene();
    }

    /// <summary>
    /// Needs to be called every frame to reset the memory cache
    /// </summary>
    public void NextFrame()
    {
        frameCache.Clear();
        internalClock.NextFrame();

        prevAreaCode = currAreaCode;
        currAreaCode = GetAreaCode();

        if (loadTrisTimer > 0)
        {
            loadTrisTimer--;
            if (loadTrisTimer == 0)
            {
                _ = InitNewScene();
            }
        }

        if (HasLevelChanged())
        {


            loadTrisTimer = 15;
        }

        int currPrio = 1;
        DebugInfo.AddInfo("Mario Action", MarioActions.GetActionName(GetMarioAction()), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario X Pos", GetMarioX().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Y Pos", GetMarioY().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Z Pos", GetMarioZ().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario X Speed", GetMarioXSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Y Speed", GetMarioYSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Z Speed", GetMarioZSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Horizontal Speed", GetMarioHorizontalSpeed().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Facing Angle", GetMarioFacingAngle().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Health", GetMarioHealth().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Cap Status", Convert.ToString(GetMarioCapFlags(), 2).PadLeft(16, '0').Insert(8, " "), "Mario", currPrio++);
        DebugInfo.AddInfo("Coin Count", GetCoinCount().ToString(), "Collected", currPrio++);
        DebugInfo.AddInfo("Star Count", GetStarCount().ToString(), "Collected", currPrio++);
        DebugInfo.AddInfo("Camera horizontal angle", GetCameraHorizontalAngle().ToString(), "Camera", currPrio++);
        DebugInfo.AddInfo("Area", GetAreaCode().ToString("X"), "Area", currPrio++);

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
        prevAreaCode = GetAreaCode();
        currAreaCode = prevAreaCode;
    }

    public bool[,] GetInternalClockState() => internalClock.GetStates();

    public uint GetMarioAction() => (uint)ReadULong(Mario.Action);
    public float GetMarioX() => ReadFloat(Mario.XPos);
    public float GetMarioY() => ReadFloat(Mario.YPos);
    public float GetMarioZ() => ReadFloat(Mario.ZPos);
    public Vector GetMarioPos() => new(GetMarioX(), GetMarioY(), GetMarioZ());
    public float GetMarioXSpeed() => ReadFloat(Mario.XSpeed);
    public float GetMarioYSpeed() => ReadFloat(Mario.YSpeed);
    public float GetMarioZSpeed() => ReadFloat(Mario.ZSpeed);
    public Vector GetMarioSpeed() => new(GetMarioXSpeed(), GetMarioYSpeed(), GetMarioZSpeed());
    public ushort GetCameraHorizontalAngle() => (ushort)ReadULong(Camera.HorizontalAngle);
    public float GetMarioHorizontalSpeed() => ReadFloat(Mario.HorizontalSpeed);
    public ushort GetMarioFacingAngle() => (ushort)ReadULong(Mario.FacingAngle);
    public ushort GetMarioCapFlags() => (ushort)ReadULong(Mario.HatFlags);
    public byte GetMarioHealth() => ReadByte(Mario.Health);
    public ushort GetCoinCount() => (ushort)ReadULong(Mario.Coins);
    public ushort GetStarCount() => (ushort)ReadULong(Progress.StarCount);
    public uint GetBehaviourBankStart() => (uint)ReadULong(GameObjects.BehaviourBankStartAddress);
    public byte GetAreaCode() => ReadByte(Area.CurrentID);
    public short GetStaticTriangleCount() => (short)ReadULong(Collision.StaticTriangleCount);
    public short GetTriangleCount() => (short)ReadULong(Collision.TotalTriangleCount);

    public bool IsMarioGrounded() => MarioActions.IsGrounded(GetMarioAction());
    public bool IsMarioSwimming() => MarioActions.IsSwimming(GetMarioAction());
    public float GetMarioFacingAngleRadian() => MathF.Tau * (-(GetMarioFacingAngle() / (float)ushort.MaxValue));
    public double GetMarioNormalizedHealth() => GetMarioHealth() / 8.0;
    public Vector GetMissionStarPos()
    {
        var stars = GetObjects().Where(o => o.IsStar()).ToList();
        if (!stars.Any()) return Vector.NaN;
        var marioPos = GetMarioPos();
        var nearestStar = stars.OrderBy(s => (s.Pos - marioPos).SquaredLength).First();
        return nearestStar.Pos;
    }
    public Vector GetMissionStarDirr() => GetMissionStarPos() - GetMarioPos();
    public double[,] GetMissionStarDirection()
    {
        double[,] res = new double[1, 3];

        var dirr = GetMissionStarDirr().Normalized();
        if (float.IsNaN(dirr.SquaredLength)) dirr = Vector.Origin;
        res[0, 0] = dirr.X;
        res[0, 1] = dirr.Y;
        res[0, 2] = dirr.Z;

        return res;
    }

    public double[,] GetMarioSpeeds()
    {
        var res = new double[1, 3];
        var speed = GetMarioSpeed();

        res[0, 0] = speed.X / (double)MAXIMUM_HORIZONTAL_SPEED;
        res[0, 1] = speed.Y / (double)MAXIMUM_VERTICAL_SPEED;
        res[0, 2] = speed.Z / (double)MAXIMUM_HORIZONTAL_SPEED;

        return res;
    }

    public double GetMarioAngle() => (GetMarioFacingAngle() * 2d / ushort.MaxValue) - 1;
    public double GetCameraAngle() => (GetCameraHorizontalAngle() * 2d / ushort.MaxValue) - 1;

    public short GetDynamicTriangleCount() => (short)(GetTriangleCount() - GetStaticTriangleCount());
    public bool HasMarioFallenOff() => GetMarioPos().Y <= outOfBoundsHeight + 2000;

    public Ray GetMarioForwardRay() => new(new(GetMarioX(), GetMarioY() + 110, GetMarioZ()), Vector.Origin.WithZ(1).RotateXZ(GetMarioFacingAngleRadian()));

    public IEnumerable<IRaytracable> GetCollision()
    {
        yield return scene;
    }

    public bool HasLevelChanged() => prevAreaCode == 0xFF && currAreaCode != prevAreaCode;

    /// <summary>
    /// Returns the level's static triangles
    /// </summary>
    public IEnumerable<IRaytracable> GetStaticCollision()
    {
        if (GetAreaCode() == 0xFF) return Enumerable.Empty<IRaytracable>();

        short staticTriCount = GetStaticTriangleCount();
        if (staticTriCount <= 0) return Enumerable.Empty<IRaytracable>();
        uint pointer = (uint)ReadULong(Collision.TrianglesListPointer);
        if (pointer == 0) return Enumerable.Empty<IRaytracable>();

        var bytes = Read(new AddressData(pointer, (uint)(staticTriCount * COLLISION_TRI_SIZE)));

        var tris = new List<CollisionTri>();
        for (int i = 0; i < bytes.Length; i += COLLISION_TRI_SIZE)
        {
            tris.Add(new(bytes[i..(i + COLLISION_TRI_SIZE)]));
        }

        outOfBoundsHeight = tris.Where(t => t.SurfaceType == 0x0A).Select(t => t.Vertex1Y).OrderBy(y => y).FirstOrDefault(short.MinValue);

        return tris.Select(t => (IRaytracable)t.Triangle);
    }

    /// <summary>
    /// Returns the level's dynamic collision triangles
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IRaytracable> GetDynamicCollision()
    {
        if (GetAreaCode() == 0xFF) return Enumerable.Empty<IRaytracable>();
        if (loadTrisTimer > 0) return Enumerable.Empty<IRaytracable>();
        short staticTriCount = GetStaticTriangleCount();
        short dynamicTriCount = GetDynamicTriangleCount();
        if (dynamicTriCount <= 0) return Enumerable.Empty<IRaytracable>();
        uint pointer = (uint)ReadULong(Collision.TrianglesListPointer);

        if (pointer == 0) return Enumerable.Empty<IRaytracable>();

        var bytes = Read(new AddressData(pointer + (uint)(staticTriCount * COLLISION_TRI_SIZE), (ushort)(dynamicTriCount * COLLISION_TRI_SIZE)));
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
        var actives = ReadMultiple(GameObjects.Active, GameObjects.SingleGameObject, GameObjects.AllGameObjects).Select(arr => arr[0]).ToList();
        uint bankStart = GetBehaviourBankStart();

        uint totalBytes = GameObjects.SingleGameObject.Length * (uint)actives.Count(a => a != 0);
        var addressesToInclude = new AddressData[]
        {
            GameObjects.BehaviourAddress,
            GameObjects.XPos,
            GameObjects.YPos,
            GameObjects.ZPos,
            GameObjects.HitboxRadius,
            GameObjects.HitboxHeight,
            GameObjects.HitboxDownOffset
        };
        var bytesPerObject = (int)addressesToInclude.Sum(a => a.Length);

        List<AddressData> addressesToRead = new();
        for (int i = 0; i < actives.Count; i++)
        {
            if (actives[i] == 0) continue;
            foreach (var addr in addressesToInclude)
            {
                addressesToRead.Add(new AddressData(addr.Address + ((uint)i) * GameObjects.SingleGameObject.Length, addr.Length));
            }
        }

        var objectsBytes = Read(addressesToRead.ToArray());
        List<GameObject> objects = new();
        for (int i = 0; i < objectsBytes.Length; i += bytesPerObject)
        {
            objects.Add(new GameObject(objectsBytes[i..(i + bytesPerObject)], bankStart));
        }

        return objects;
    }

    private OctTree InitNewScene()
    {
        frameCache.Clear();

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
            yield return new AddressData((uint)(addressData.Address + i * offset.Length), addressData.Length);
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
    private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData) => frameCache;

    private void InitFrameCache()
    {
        var staticTriCount = GetStaticTriangleCount();
        var dynamicTrisCount = GetDynamicTriangleCount();
        var actives = ReadMultiple(GameObjects.Active, GameObjects.SingleGameObject, GameObjects.AllGameObjects).ToArray();
        byte activeCount = (byte)Array.IndexOf(actives, (byte)0);

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
            Progress.StarCount,
            Camera.HorizontalAngle,
            Mario.FacingAngle,
            new AddressData(Collision.TrianglesListPointer.Address + (uint)(staticTriCount * COLLISION_TRI_SIZE), (ushort)(dynamicTrisCount * COLLISION_TRI_SIZE)),
        };

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

        toRead.AddRange(GetCalculatedAddresses(totalBytes, GameObjects.SingleGameObject.Length, addressesToRead));

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
                    Length = baseAddresses[j].Length
                };
            }
        }
    }
}
