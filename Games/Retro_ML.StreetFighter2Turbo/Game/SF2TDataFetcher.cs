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
        public byte GetPlayer1Screen() => ReadSingle(Player1Addresses.CurrentScreen);
        public byte GetPlayer2Screen() => ReadSingle(Player2Addresses.CurrentScreen);
        public uint GetPlayer1XPos() => (uint)(ToUnsignedInteger(Read(Player1Addresses.XPos)) + (GetPlayer1Screen() * 0x10000));
        public uint GetPlayer1YPos() => ToUnsignedInteger(Read(Player1Addresses.YPos));
        public uint GetPlayer2XPos() => (uint)(ToUnsignedInteger(Read(Player2Addresses.XPos)) + (GetPlayer2Screen() * 0x10000));
        public uint GetPlayer2YPos() => ToUnsignedInteger(Read(Player2Addresses.YPos));
        public uint GetPlayer1Hp() => ToUnsignedInteger(Read(Player1Addresses.HP));
        public uint GetPlayer2Hp() => ToUnsignedInteger(Read(Player2Addresses.HP));
        public byte GetPlayer1RoundCount() => ReadSingle(Player1Addresses.RoundsWin);
        public byte GetPlayer2RoundCount() => ReadSingle(Player2Addresses.RoundsWin);
        public uint GetHorizontalDistanceBetweenPlayers() => (uint)Math.Abs(GetPlayer2XPos() - GetPlayer1XPos());
        public bool isPlayer1Dead() => ReadSingle(Player1Addresses.HP) == 0xFF;
        public bool isPlayer2Dead() => ReadSingle(Player2Addresses.HP) == 0xFF;
        public bool isPlayer1InEndRound() => ReadSingle(Player1Addresses.EndRoundStatus) == 0x01;
        public bool isPlayer2InEndRound() => ReadSingle(Player2Addresses.EndRoundStatus) == 0x01;
        public bool isPlayer1Crouched() => ReadSingle(Player1Addresses.State) == 0x02;
        public bool isPlayer2Crouched() => ReadSingle(Player2Addresses.State) == 0x02;
        public bool isPlayer1Jumping() => ReadSingle(Player1Addresses.State) == 0x04;
        public bool isPlayer2Jumping() => ReadSingle(Player2Addresses.State) == 0x04;
        public bool isPlayer1Attacking() => ReadSingle(Player1Addresses.State) == 0x0E;
        public bool isPlayer2Attacking() => ReadSingle(Player2Addresses.State) == 0x0E;
        public bool isPlayer1Blocking() => ReadSingle(Player1Addresses.Input) == 0x03;
        public bool isPlayer2Blocking() => ReadSingle(Player2Addresses.Input) == 0x03;

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
                (Player1Addresses.XPos, false),
                (Player1Addresses.YPos, false),
                (Player1Addresses.CurrentScreen, false),
                (Player1Addresses.State, false),
                (Player1Addresses.HP, false),
                (Player1Addresses.EndRoundStatus, false),
                (Player1Addresses.RoundsWin, false),
                (Player1Addresses.Input, false),
                (Player2Addresses.XPos, false),
                (Player2Addresses.YPos, false),
                (Player2Addresses.CurrentScreen, false),
                (Player2Addresses.State, false),
                (Player2Addresses.HP, false),
                (Player2Addresses.EndRoundStatus, false),
                (Player2Addresses.RoundsWin, false),
                (Player2Addresses.Input, false),
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
