using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.PokemonGen1.Configuration;
using Retro_ML.PokemonGen1.Game.Data;
using Retro_ML.Utils;
using static Retro_ML.PokemonGen1.Game.Addresses;
using static Retro_ML.PokemonGen1.Game.Data.PokemonTypes;

namespace Retro_ML.PokemonGen1.Game;

/// <summary>
/// Takes care of abstracting away the addresses when communicating with the emulator.
/// </summary>
internal class PokemonDataFetcher : IDataFetcher
{
    private const int MAXIMUM_SLEEP_COUNTER = 7;
    private const int MAXIMUM_ATTACK_STAT = 366;
    private const int MAXIMUM_DEFENSE_STAT = 458;
    private const int MAXIMUM_SPEED_STAT = 378;
    private const int MAXIMUM_SPECIAL_STAT = 406;

    public bool IsPokemonYellow { get; private set; } = true;

    private readonly IEmulatorAdapter emulator;
    private readonly Dictionary<uint, byte[]> fakeCache;
    private readonly Dictionary<uint, byte[]> turnCache;
    private readonly Dictionary<uint, byte[]> battleCache;
    private readonly PokemonPluginConfig pluginConfig;
    private readonly InternalClock internalClock;

    public PokemonDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, PokemonPluginConfig pluginConfig)
    {
        this.emulator = emulator;
        fakeCache = new();
        turnCache = new();
        battleCache = new();
        this.pluginConfig = pluginConfig;
        internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);
    }

    /// <summary>
    /// Needs to be called every frame to reset the memory cache
    /// </summary>
    public void NextFrame()
    {
        turnCache.Clear();
        internalClock.NextFrame();

        InitFrameCache();
    }

    /// <summary>
    /// Needs to be called every time a save state was loaded to reset the global cache.
    /// </summary>
    public void NextState()
    {
        battleCache.Clear();
        turnCache.Clear();

        IsPokemonYellow = false;
        IsPokemonYellow = (ReadSingle(PlayerPokemons.EndOfList) != 0x0);

        internalClock.Reset();
    }

    public bool[,] GetInternalClockState() => internalClock.GetStates();

    public byte GetOpposingPokemonStatusEffect() => ReadSingle(OpposingPokemon.StatusEffect);
    public double GetOpposingPokemonSleep() => (ReadSingle(OpposingPokemon.StatusEffect) & 0b0000_0111) / (double)MAXIMUM_SLEEP_COUNTER;
    public bool GetOpposingPokemonParalysis() => (ReadSingle(OpposingPokemon.StatusEffect) & 0b0100_0000) != 0;
    public bool GetOpposingPokemonFrozen() => (ReadSingle(OpposingPokemon.StatusEffect) & 0b0010_0000) != 0;
    public bool GetOpposingPokemonBurned() => (ReadSingle(OpposingPokemon.StatusEffect) & 0b0001_0000) != 0;
    public bool GetOpposingPokemonPoisoned() => (ReadSingle(OpposingPokemon.StatusEffect) & 0b0000_1000) != 0;
    public byte GetStatusEffect() => ReadSingle(CurrentPokemon.StatusEffect);
    public double GetSleep() => (ReadSingle(CurrentPokemon.StatusEffect) & 0b0000_0111) / (double)MAXIMUM_SLEEP_COUNTER;
    public bool GetParalysis() => (ReadSingle(CurrentPokemon.StatusEffect) & 0b0100_0000) != 0;
    public bool GetFrozen() => (ReadSingle(CurrentPokemon.StatusEffect) & 0b0010_0000) != 0;
    public bool GetBurned() => (ReadSingle(CurrentPokemon.StatusEffect) & 0b0001_0000) != 0;
    public bool GetPokemonPoisoned() => (ReadSingle(CurrentPokemon.StatusEffect) & 0b0000_1000) != 0;
    public bool IsSuperEffective() => GetMultiplier() >= 2;
    public bool IsNotVeryEffective() => GetMultiplier() < 1;
    public bool IsSTAB() => ReadSingle(CurrentPokemon.SelectedMoveType) == ReadSingle(CurrentPokemon.Type1) || ReadSingle(CurrentPokemon.SelectedMoveType) == ReadSingle(CurrentPokemon.Type2);
    public double OpposingCurrentHP() => ReadULong(OpposingPokemon.CurrentHP) / (double)ReadULong(OpposingPokemon.MaxHP);
    public double GetAttack() => ReadULong(CurrentPokemon.Attack) / (double)MAXIMUM_ATTACK_STAT;
    public double GetDefense() => ReadULong(CurrentPokemon.Defense) / (double)MAXIMUM_DEFENSE_STAT;
    public double GetSpeed() => ReadULong(CurrentPokemon.Speed) / (double)MAXIMUM_SPEED_STAT;
    public double GetSpecial() => ReadULong(CurrentPokemon.Special) / (double)MAXIMUM_SPECIAL_STAT;
    public double GetOpposingAttack() => ReadULong(OpposingPokemon.Attack) / (double)MAXIMUM_ATTACK_STAT;
    public double GetOpposingDefense() => ReadULong(OpposingPokemon.Defense) / (double)MAXIMUM_DEFENSE_STAT;
    public double GetOpposingSpeed() => ReadULong(OpposingPokemon.Speed) / (double)MAXIMUM_SPEED_STAT;
    public double GetOpposingSpecial() => ReadULong(OpposingPokemon.Special) / (double)MAXIMUM_SPECIAL_STAT;
    public bool WonFight() => ReadULong(OpposingPokemon.CurrentHP) == 0;
    public bool LostFight() => ReadULong(CurrentPokemon.CurrentHP) == 0;
    public bool InFight() => ReadSingle(GameState) != 0;
    public double SelectedMovePower() => ReadSingle(CurrentPokemon.SelectedMovePower) / 170.0;
    public bool Move1Exists() => Read(CurrentPokemon.MoveIDs)[0] != 0;
    public bool Move2Exists() => Read(CurrentPokemon.MoveIDs)[1] != 0;
    public bool Move3Exists() => Read(CurrentPokemon.MoveIDs)[2] != 0;
    public bool Move4Exists() => Read(CurrentPokemon.MoveIDs)[3] != 0;
    public bool IsFightOptionSelected() => ReadSingle(FightCursor) == 193;
    public byte GetMovePP(int index) => (byte)(Read(CurrentPokemon.MovesCurrentPP)[index] & 0b0011_1111);
    public bool IsMoveDisabled(int index) => Read(CurrentPokemon.MoveIDs)[index] == ReadSingle(CurrentPokemon.DisabledMove);
    public bool IsPlayerTrapped() => (ReadSingle(OpposingPokemon.BattleStatus) & 0b0010_0000) != 0;
    public int GetMoveCursorIndex() => ReadSingle(MoveCursorIndex);

    public double GetMultiplier()
    {
        byte moveType = ReadSingle(CurrentPokemon.SelectedMoveType);
        byte ennemyType1 = ReadSingle(OpposingPokemon.Type1);
        byte ennemyType2 = ReadSingle(OpposingPokemon.Type2);

        return PokemonTypes.GetMultiplier((Types)moveType, (Types)ennemyType1, (Types)ennemyType2);
    }

    /// <summary>
    /// Reads a single byte from the emulator's memory
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private byte ReadSingle(AddressData addressData) => Read(addressData)[0];

    /// <summary>
    /// Reads up to 8 bytes from the address, assuming little endian.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private ulong ReadULong(AddressData addressData)
    {
        var bytes = Read(addressData);
        ulong value = 0;
        if (addressData.IsBigEndian)
        {
            for (int i = 0; i < bytes.Length && i < 8; i++)
            {
                value += (ulong)bytes[i] << (bytes.Length - i - 1) * 8;
            }
        }
        else
        {
            for (int i = 0; i < bytes.Length && i < 8; i++)
            {
                value += (ulong)bytes[i] << i * 8;
            }
        }

        return value;
    }

    /// <summary>
    /// Reads a specific amount of bytes from the emulator's memory, using the AddressData
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private byte[] Read(AddressData addressData)
    {
        addressData = GetCorrectedAddress(addressData);
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
            yield return new AddressData((uint)(addressData.Address + i * offset.Length), addressData.Length, addressData.CacheDuration, addressData.IsBigEndian);
        }
    }

    /// <summary>
    /// Reads multiple ranges of addresses
    /// </summary>
    /// <param name="addresses"></param>
    /// <returns></returns>
    private byte[] Read(params AddressData[] addresses)
    {
        addresses = GetCorrectedAddresses(addresses);

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
    private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData)
    {
        if (addressData.CacheDuration == AddressData.CacheDurations.NoCache)
        {
            fakeCache.Clear();
        }
        return addressData.CacheDuration switch
        {
            AddressData.CacheDurations.NoCache => fakeCache,
            AddressData.CacheDurations.Turn => turnCache,
            AddressData.CacheDurations.Battle => battleCache,
            _ => fakeCache,
        };
    }

    private void InitFrameCache()
    {
        List<AddressData> toRead = new()
        {

        };

        _ = Read(toRead.ToArray());
    }

    private AddressData GetCorrectedAddress(AddressData address) => GetCorrectedAddresses(address)[0];

    private AddressData[] GetCorrectedAddresses(params AddressData[] addresses)
    {
        if (IsPokemonYellow)
        {
            AddressData[] newAddresses = new AddressData[addresses.Length];
            for (int i = 0; i < addresses.Length; i++)
            {
                newAddresses[i] = new AddressData(addresses[i].Address - 1, addresses[i].Length, addresses[i].CacheDuration, addresses[i].IsBigEndian);
            }
            addresses = newAddresses;
        }
        return addresses;
    }

}
