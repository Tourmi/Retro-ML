using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.SuperMario64.Game.Data;
using Retro_ML.Utils;
using static Retro_ML.SuperMario64.Game.Addresses;

namespace Retro_ML.SuperMario64.Game;

/// <summary>
/// Takes care of abstracting away the addresses when communicating with the emulator.
/// </summary>
internal class SM64DataFetcher : IDataFetcher
{
    private const ushort COLLISION_TRI_SIZE = 0x30;

    private readonly IEmulatorAdapter emulator;
    private readonly Dictionary<uint, byte[]> frameCache;
    private readonly Dictionary<uint, byte[]> levelCache;
    private readonly SM64PluginConfig pluginConfig;
    private readonly InternalClock internalClock;

    public SM64DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SM64PluginConfig pluginConfig)
    {
        this.emulator = emulator;
        frameCache = new();
        levelCache = new();
        this.pluginConfig = pluginConfig;
        internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);
    }

    /// <summary>
    /// Needs to be called every frame to reset the memory cache
    /// </summary>
    public void NextFrame()
    {
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
        DebugInfo.AddInfo("Mario Health", GetMarioHealth().ToString(), "Mario", currPrio++);
        DebugInfo.AddInfo("Mario Cap Status", Convert.ToString(GetMarioCapFlags(), 2).PadLeft(16, '0').Insert(8, " "), "Mario", currPrio++);
        DebugInfo.AddInfo("Coin Count", GetCoinCount().ToString(), "Collected", currPrio++);
        DebugInfo.AddInfo("Star Count", GetStarCount().ToString(), "Collected", currPrio++);

        InitFrameCache();
    }

    /// <summary>
    /// Needs to be called every time a save state was loaded to reset the global cache.
    /// </summary>
    public void NextState()
    {
        frameCache.Clear();
        levelCache.Clear();

        internalClock.Reset();
    }

    public bool[,] GetInternalClockState() => internalClock.GetStates();
    public float GetMarioX() => ReadFloat(Mario.XPos);
    public float GetMarioY() => ReadFloat(Mario.YPos);
    public float GetMarioZ() => ReadFloat(Mario.ZPos);
    public float GetMarioXSpeed() => ReadFloat(Mario.XSpeed);
    public float GetMarioYSpeed() => ReadFloat(Mario.YSpeed);
    public float GetMarioZSpeed() => ReadFloat(Mario.ZSpeed);
    public float GetMarioHorizontalSpeed() => ReadFloat(Mario.HorizontalSpeed);
    public ushort GetMarioCapFlags() => (ushort)ReadULong(Mario.HatFlags);
    public byte GetMarioHealth() => ReadByte(Mario.Health);
    public sbyte GetMarioGroundOffset() => (sbyte)ReadByte(Mario.GroundOffset);
    public ushort GetCoinCount() => (ushort)ReadULong(Mario.Coins);
    public ushort GetStarCount() => (ushort)ReadULong(Progress.StarCount);

    public ushort GetStaticTriangleCount() => (ushort)ReadULong(Collision.StaticTriangleCount);
    public ushort GetTriangleCount() => (ushort)ReadULong(Collision.TotalTriangleCount);
    public ushort GetDynamicTriangleCount() => (ushort)(GetTriangleCount() - GetStaticTriangleCount());

    /// <summary>
    /// Returns the level's static triangles
    /// </summary>
    public IEnumerable<CollisionTri> GetStaticTris()
    {
        ushort staticTriCount = GetStaticTriangleCount();
        uint pointer = (uint)ReadULong(Collision.TrianglesListPointer);
        var bytes = Read(new AddressData(pointer, (ushort)(staticTriCount * COLLISION_TRI_SIZE), AddressData.CacheDurations.Level));

        var tris = new List<CollisionTri>();
        for (int i = 0; i < bytes.Length; i += COLLISION_TRI_SIZE)
        {
            tris.Add(new(bytes[i..(i + COLLISION_TRI_SIZE)]));
        }

        return tris;
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
    private ulong ReadULong(AddressData addressData)
    {
        var bytes = Read(addressData);
        ulong value = 0;
        for (int i = 0; i < bytes.Length && i < 8; i++)
        {
            value += (ulong)bytes[i] << (bytes.Length - i - 1) * 8;
        }
        return value;
    }

    private float ReadFloat(AddressData addressData)
    {
        var bytes = Read(addressData);
        //We need to reverse the bytes if the current system is little endian
        if (BitConverter.IsLittleEndian) bytes = bytes.Reverse().ToArray();

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
