using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Utils;
using static Retro_ML.Metroid.Game.Addresses;

namespace Retro_ML.Metroid.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class MetroidDataFetcher : IDataFetcher
    {
        public const int MAXIMUM_VERTICAL_SPEED = 0x500;
        public const int MAXIMUM_HORIZONTAL_SPEED = 0x180;
        public const int INVINCIBILITY_TIMER_LENGTH = 50;
        public const int TANK_HEALTH = 1000;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> roomCache;
        private readonly MetroidPluginConfig pluginConfig;

        private InternalClock internalClock;

        public MetroidDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, MetroidPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            roomCache = new();
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

            InitFrameCache();
        }

        /// <summary>
        /// Needs to be called every time a save state was loaded to reset the global cache.
        /// </summary>
        public void NextState()
        {
            frameCache.Clear();
            roomCache.Clear();

            internalClock.Reset();
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();

        public ushort GetSamusHealth() => (ushort)ReadNybbleDigitsToUlong(Samus.Health);
        public ushort GetMaximumHealth() => (ushort)((ReadSingle(Progress.EnergyTanks) + 1) * TANK_HEALTH - 1);
        public double GetSamusHealthRatio() => GetSamusHealth() / (double)GetMaximumHealth();
        public ushort GetDeathCount() => (ushort)ReadULong(Progress.Deaths);
        public byte GetSamusXPosition() => ReadSingle(Samus.XPosition);
        public byte GetSamusYPosition() => ReadSingle(Samus.YPosition);
        public bool IsSamusLookingRight() => ReadSingle(Samus.LookingDirection) == 0;
        public bool IsSamusInLava() => ReadSingle(Samus.InLava) == 1;
        public bool IsMetroidOnSamusHead() => ReadSingle(Samus.HasMetroidOnHead) == 1;
        public bool IsSamusOnElevator() => ReadSingle(Samus.IsOnElevator) == 1;
        public bool IsSamusUsingMissiles() => ReadSingle(Samus.UsingMissiles) == 1;
        public double SamusInvincibilityTimer() => ReadSingle(Samus.InvincibleTimer) / (double)INVINCIBILITY_TIMER_LENGTH;
        public double GetCurrentMissiles() => ReadSingle(Progress.Missiles) / Math.Max(1.0, ReadSingle(Progress.MissileCapacity));

        public byte GetMapX() => ReadSingle(Gamestate.MapX);
        public byte GetMapY() => ReadSingle(Gamestate.MapY);

        public double GetSamusVerticalSpeed() => (((sbyte)ReadSingle(Samus.VerticalSpeed)) * 0x100 + ReadSingle(Samus.VerticalFractionalSpeed)) / (double)MAXIMUM_VERTICAL_SPEED;
        public double GetSamusHorizontalSpeed() => (((sbyte)ReadSingle(Samus.HorizontalSpeed)) * 0x100 + ReadSingle(Samus.HorizontalFractionalSpeed)) / (double)MAXIMUM_HORIZONTAL_SPEED;

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
            for (int i = 0; i < bytes.Length && i < 8; i++)
            {
                value += (ulong)bytes[i] << i * 8;
            }
            return value;
        }

        /// <summary>
        /// <br>Reads up to 8 bytes from the address, assuming byte-wise little endian, and interprets all nybbles as decimal digits.</br>
        /// <br>Examples:</br>
        /// <code><br>0x3412 -> 1234  </br>
        /// <br>0x90   -> 90    </br>
        /// <br>0x0180 -> 8001  </br>
        /// <br>0x4    -> 4     </br>
        /// <br>0xA    -> 10    </br></code>
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>
        private ulong ReadNybbleDigitsToUlong(AddressData addressData)
        {
            var bytes = Read(addressData);
            ulong value = 0;
            for (int i = 0; i < bytes.Length && i < 8; i++)
            {
                var currByte = bytes[i];

                var smallDigit = currByte & 0b0000_1111;
                var bigDigit = (currByte & 0b1111_0000) >> 4;
                value += ((ulong)(smallDigit + bigDigit * 10)) * 100.PosPow(i);
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
            var cacheToUse = GetCacheToUse(addressData);
            if (!cacheToUse.ContainsKey(addressData.Address))
            {
                cacheToUse[addressData.Address] = emulator.ReadMemory(addressData.Address, addressData.Length);
            }

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
            uint count = total.Length / offset.Length;
            var results = new byte[count][];
            var toRead = new List<AddressData>();
            for (int i = 0; i < results.Length; i++)
            {
                toRead.Add(new AddressData((uint)(addressData.Address + i * offset.Length), addressData.Length, addressData.CacheDuration));
            }
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
                {
                    toFetch.Add((address.Address, address.Length));
                }

                totalBytes += address.Length;
            }

            byte[] data = Array.Empty<byte>();
            if (toFetch.Count > 0)
            {
                data = emulator.ReadMemory(toFetch.ToArray());
            }

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
            switch (addressData.CacheDuration)
            {
                case AddressData.CacheDurations.Frame:
                    return frameCache;
                case AddressData.CacheDurations.Room:
                    return roomCache;
                default:
                    return frameCache;
            }
        }

        private void InitFrameCache()
        {
            List<AddressData> toRead = new()
            {
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
}
