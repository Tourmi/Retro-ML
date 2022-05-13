using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioKart.Configuration;
using static Retro_ML.SuperMarioKart.Game.Addresses;

namespace Retro_ML.SuperMarioKart.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class SMKDataFetcher : IDataFetcher
    {
        public const int INTERNAL_CLOCK_LENGTH = 8;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> raceCache;

        private InternalClock internalClock;

        public SMKDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SMKPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            raceCache = new();
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
            raceCache.Clear();

            internalClock.Reset();
        }

        public byte GetMaxCheckpoint() => ReadSingle(Race.CheckpointCount);
        public byte GetCurrentCheckpoint() => ReadSingle(Racer.CurrentCheckpointNumber);
        public sbyte GetCurrentLap() => (sbyte)(ReadSingle(Racer.CurrentLap) - 128);
        public bool IsOffroad() => ReadSingle(Racer.KartStatus) == 0x10;

        public double GetHeadingDifference()
        {
            var flowmap = Read(Racetrack.FlowMap);
            var racerX = (ushort)ToUnsignedInteger(Read(Racer.XPosition));
            var racerY = (ushort)ToUnsignedInteger(Read(Racer.YPosition));
            var flowX = Math.Clamp(racerX / 16, 0, 63);
            var flowY = Math.Clamp(racerY / 16, 0, 63);

            byte currAngle = (byte)(ToUnsignedInteger(Read(Racer.HeadingAngle)) / 256);
            byte currFlowAngle = flowmap[flowY * 64 + flowX];

            return ((sbyte)(currFlowAngle - currAngle)) / (double)sbyte.MaxValue;
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();

        /// <summary>
        /// Reads the Low and High bytes at the addresses specified in AddressData, and puts them together into ushorts, assuming little Endian.
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>
        private ushort[] ReadLowHighBytes(AddressData addressData)
        {
            var cacheToUse = GetCacheToUse(addressData);
            if (!cacheToUse.ContainsKey(addressData.Address))
            {
                cacheToUse[addressData.Address] = emulator.ReadMemory(addressData.Address, addressData.Length);
            }
            if (!cacheToUse.ContainsKey(addressData.HighByteAddress))
            {
                cacheToUse[addressData.HighByteAddress] = emulator.ReadMemory(addressData.HighByteAddress, addressData.Length);
            }

            var result = new ushort[addressData.Length];
            for (int i = 0; i < addressData.Length; i++)
            {
                result[i] = (ushort)(cacheToUse[addressData.Address][i] + (cacheToUse[addressData.HighByteAddress][i] << 8));
            }

            return result;
        }

        /// <summary>
        /// Reads a single byte from the emulator's memory
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>
        private byte ReadSingle(AddressData addressData) => Read(addressData)[0];

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
        private byte[] Read(params (AddressData address, bool isLowHighByte)[] addresses)
        {
            List<(uint addr, uint length)> toFetch = new();

            uint totalBytes = 0;

            foreach ((AddressData address, bool isLowHighByte) in addresses)
            {
                var cacheToUse = GetCacheToUse(address);
                if (!cacheToUse.ContainsKey(address.Address))
                {
                    toFetch.Add((address.Address, address.Length));
                }
                if (isLowHighByte && !cacheToUse.ContainsKey(address.HighByteAddress))
                {
                    toFetch.Add((address.HighByteAddress, address.Length));
                }

                totalBytes += address.Length * (uint)(isLowHighByte ? 2 : 1);
            }

            byte[] data = Array.Empty<byte>();
            if (toFetch.Count > 0)
            {
                data = emulator.ReadMemory(toFetch.ToArray());
            }

            List<byte> bytes = new();
            int dataIndex = 0;
            foreach ((AddressData address, bool isLowHighByte) in addresses)
            {
                int count = (int)address.Length;

                var cacheToUse = GetCacheToUse(address);
                if (!cacheToUse.ContainsKey(address.Address))
                {
                    cacheToUse[address.Address] = data[dataIndex..(dataIndex + count)];
                    dataIndex += count;
                }
                if (isLowHighByte && !cacheToUse.ContainsKey(address.HighByteAddress))
                {
                    cacheToUse[address.HighByteAddress] = data[dataIndex..(dataIndex + count)];
                    dataIndex += count;
                }

                bytes.AddRange(cacheToUse[address.Address]);
                if (isLowHighByte) bytes.AddRange(cacheToUse[address.HighByteAddress]);
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
            var cacheToUse = frameCache;
            if (addressData.CacheDuration == AddressData.CacheDurations.Race)
            {
                cacheToUse = raceCache;
            }

            return cacheToUse;
        }

        private void InitFrameCache()
        {
            (AddressData, bool)[] toRead = new (AddressData, bool)[]
            {
            };

            _ = Read(toRead);
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
