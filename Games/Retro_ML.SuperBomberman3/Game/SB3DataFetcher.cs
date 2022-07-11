using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperBomberman3.Configuration;
using static Retro_ML.SuperBomberman3.Game.Addresses;

namespace Retro_ML.SuperBomberman3.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class SB3DataFetcher : IDataFetcher
    {
        private const uint LEVEL_SIZE = LEVEL_HEIGHT * LEVEL_WIDTH;
        private const uint METATILE_SIZE = 0x10;
        public const uint LEVEL_WIDTH = 0xF;
        public const uint LEVEL_HEIGHT = 0xD;
        private const uint MIN_X_POS = 0x10;
        private const uint MIN_Y_POS = 0x10;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> tilesCache;
        private readonly InternalClock internalClock;

        public SB3DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SB3PluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            tilesCache = new();
            internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public byte GetBomberManXPos() => ReadSingle(BombermanAddresses.XPos);
        public byte GetBomberManYPos() => ReadSingle(BombermanAddresses.YPos);

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
            tilesCache.Clear();
            internalClock.Reset();
        }

        /// <summary>
        /// Get all tiles around a position
        /// </summary>
        public bool[,] GetBomberManPositionInLevel()
        {
            bool[,] result = new bool[LEVEL_HEIGHT, LEVEL_WIDTH];

            uint xPosInLevel = (GetBomberManXPos() - MIN_X_POS ) / METATILE_SIZE;
            uint yPosInLevel = (GetBomberManYPos() - MIN_Y_POS ) / METATILE_SIZE;

            result[yPosInLevel, xPosInLevel] = true;

            return result;
        }

        private byte ReadSingle(AddressData addressData) => Read(addressData)[0];

        /// <summary>
        /// Reads a specific amount of bytes from the emulator's memory, using the AddressData
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>

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
        private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData) => addressData.CacheType switch
        {
            AddressData.CacheTypes.Frame => frameCache,
            _ => frameCache,
        };

        private void InitFrameCache()
        {
            List<AddressData> toRead = new()
        {
            BombermanAddresses.XPos,
            BombermanAddresses.YPos,
        };
            _ = Read(toRead.ToArray());
        }

        private static uint ToUnsignedInteger(byte[] bytes)
        {
            uint value = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                value += (uint)bytes[i] << i * 8;
            }
            return value;
        }
    }
}
