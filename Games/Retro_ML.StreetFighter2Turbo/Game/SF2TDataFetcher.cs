using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.StreetFighter2Turbo.Configuration;
using static Retro_ML.StreetFighter2Turbo.Game.Addresses;

namespace Retro_ML.StreetFighter2Turbo.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class SF2TDataFetcher : IDataFetcher
    {
        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly InternalClock internalClock;

        public SF2TDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SF2TPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
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
            internalClock.Reset();
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public byte GetPlayer1XPos() => ReadSingle(GameAddresses.Player1XPos);
        public byte GetPlayer2XPos() => ReadSingle(GameAddresses.Player2XPos);
        public byte GetPlayer1Screen() => ReadSingle(GameAddresses.Player1CurrentScreen);
        public byte GetPlayer2Screen() => ReadSingle(GameAddresses.Player2CurrentScreen);
        public uint GetPlayer1AbsoluteXPosition() => (uint)(GetPlayer1Screen() * 0x100) + GetPlayer1XPos();
        public uint GetPlayer2AbsoluteXPosition() => (uint)(GetPlayer2Screen() * 0x100) + GetPlayer2XPos();
        public uint GetDistanceBetweenPlayers() => (uint)Math.Abs(GetPlayer2AbsoluteXPosition() - GetPlayer1AbsoluteXPosition());
        public bool isPlayer1Dead() => ReadSingle(GameAddresses.Player1HP) == 0x07;
        public bool isPlayer2Dead() => ReadSingle(GameAddresses.Player2HP) == 0x07;
        public bool isPlayer1Crouched() => ToUnsignedInteger(Read(GameAddresses.Player1State)) == 0x6D;
        public bool isPlayer2Crouched() => ToUnsignedInteger(Read(GameAddresses.Player2State)) == 0x6D;
        public bool isPlayer1Airborn() => ToUnsignedInteger(Read(GameAddresses.Player1State)) == 0x65;
        public bool isPlayer2Airborn() => ToUnsignedInteger(Read(GameAddresses.Player2State)) == 0x65;


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
                cacheToUse[addressData.Address] = emulator.ReadMemory(addressData.Address, addressData.Length);

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

        private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData)
        {
            return frameCache;
        }

        private void InitFrameCache()
        {
            (AddressData, bool)[] toRead = new (AddressData, bool)[]
            {
                (GameAddresses.Player1XPos, false),
                (GameAddresses.Player2XPos, false),
                (GameAddresses.Player1CurrentScreen, false),
                (GameAddresses.Player2CurrentScreen, false),
                (GameAddresses.Player1State, false),
                (GameAddresses.Player2State, false),
                (GameAddresses.Player1HP, false),
                (GameAddresses.Player2HP, false),
                (GameAddresses.RoundTimer, false),
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
