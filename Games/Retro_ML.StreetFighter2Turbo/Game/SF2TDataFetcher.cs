using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Utils;
using Retro_ML.StreetFighter2Turbo.Configuration;
using static Retro_ML.StreetFighter2Turbo.Game.Addresses;

namespace Retro_ML.StreetFighter2Turbo.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class SF2TDataFetcher : IDataFetcher
    {
        private const int MAX_HORIZONTAL_DISTANCE = 0xD200;
        private const int MAX_VERTICAL_DISTANCE = 0x4200;
        private const int MIN_HORIZONTAL_POSITION = 0x3700;
        private const int MAX_HORIZONTAL_POSITION = 0x1CA00;
        private const int MAX_HP = 176;
        private const int MAX_CLOCK = 99;

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
        public bool IsRoundOver() => IsPlayer1InEndRound() || IsPlayer2InEndRound();
        public bool HasPlayerWon() => IsRoundOver() && GetPlayer1Hp() > GetPlayer2Hp();
        public bool HasPlayerLost() => IsRoundOver() && GetPlayer2Hp() > GetPlayer1Hp();
        public bool IsRoundDraw() => IsRoundOver() && GetPlayer1Hp() == GetPlayer2Hp();
        public ulong GetRoundTimer() => ReadNybbleDigitsToUlong(GameAddresses.RoundTimer);
        public double GetRoundTimerNormalized() => GetRoundTimer() / (double)MAX_CLOCK;
        public uint GetPlayer1XPos() => ToUnsignedInteger(Read(Player1Addresses.XPos));
        public uint GetPlayer2XPos() => ToUnsignedInteger(Read(Player2Addresses.XPos));
        public double GetPlayer1XPosNormalized() => GetHorizontalPositionRatio(GetPlayer1XPos());
        public double GetPlayer2XPosNormalized() => GetHorizontalPositionRatio(GetPlayer2XPos());
        public uint GetPlayer1YPos() => ToUnsignedInteger(Read(Player1Addresses.YPos));
        public uint GetPlayer2YPos() => ToUnsignedInteger(Read(Player2Addresses.YPos));
        public byte GetPlayer1Hp() => ReadSingle(Player1Addresses.HP) == 0xFF ? (byte)0 : ReadSingle(Player1Addresses.HP);
        public byte GetPlayer2Hp() => ReadSingle(Player2Addresses.HP) == 0xFF ? (byte)0 : ReadSingle(Player2Addresses.HP);
        public double GetPlayer1HpNormalized() => GetPlayer1Hp() / (double)MAX_HP;
        public double GetPlayer2HpNormalized() => GetPlayer2Hp() / (double)MAX_HP;
        public double GetHorizontalDistanceBetweenPlayers() => (GetPlayer2XPos() - GetPlayer1XPos()) / (double)MAX_HORIZONTAL_DISTANCE;
        public double GetVerticalDistanceBetweenPlayers() => (GetPlayer2YPos() - GetPlayer1YPos()) / (double)MAX_VERTICAL_DISTANCE;
        public bool IsPlayer1InEndRound() => ReadSingle(Player1Addresses.EndRoundStatus) == 0x01;
        public bool IsPlayer2InEndRound() => ReadSingle(Player2Addresses.EndRoundStatus) == 0x01;
        public bool IsPlayer1Crouched() => ReadSingle(Player1Addresses.State) == 0x02;
        public bool IsPlayer2Crouched() => ReadSingle(Player2Addresses.State) == 0x02;
        public bool IsPlayer1Jumping() => ReadSingle(Player1Addresses.State) == 0x04;
        public bool IsPlayer2Jumping() => ReadSingle(Player2Addresses.State) == 0x04;
        public bool IsPlayer1Blocking() => ReadSingle(Player1Addresses.Input) == 0x03;
        public bool IsPlayer2Blocking() => ReadSingle(Player2Addresses.Input) == 0x03;
        public bool IsPlayer1Staggered() => ReadSingle(Player1Addresses.State) == 0x14 || ReadSingle(Player1Addresses.State) == 0x0E;
        public bool IsPlayer2Staggered() => ReadSingle(Player2Addresses.State) == 0x14 || ReadSingle(Player2Addresses.State) == 0x0E;
        public bool IsPlayer1Attacking() => ReadSingle(Player1Addresses.State) == 0x0A;
        public bool IsPlayer2Attacking() => ReadSingle(Player2Addresses.State) == 0x0A;
        public bool IsPlayer1Punching() => IsPlayer1Attacking() ? ReadSingle(Player1Addresses.AttackType) == 0x00 : false;
        public bool IsPlayer2Punching() => IsPlayer2Attacking() ? ReadSingle(Player2Addresses.AttackType) == 0x00 : false;
        public bool IsPlayer1Kicking() => IsPlayer1Attacking() ? ReadSingle(Player1Addresses.AttackType) == 0x02 : false;
        public bool IsPlayer2Kicking() => IsPlayer2Attacking() ? ReadSingle(Player2Addresses.AttackType) == 0x02 : false;
        public bool IsPlayer1Throwing() => IsPlayer1Attacking() ? ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x200 || ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0402 : false;
        public bool IsPlayer2Throwing() => IsPlayer2Attacking() ? ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x200 || ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0402 : false;
        public double GetPlayer1AttackStrength()
        {
            if (IsPlayer1Attacking())
            {
                //Light attack
                if (ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0000 || ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0001) return 1 / 3;
                //Medium attack
                if (ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0200 || ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0202 || ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0203) return 2 / 3;
                //Heavy attack
                if (ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0402 || ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0404 || ToUnsignedInteger(Read(Player1Addresses.AttackStrength)) == 0x0405) return 1;
            }
            return 0;
        }
        public double GetPlayer2AttackStrength()
        {
            if (IsPlayer2Attacking())
            {
                //Light attack
                if (ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0000 || ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0001) return 1 / 3;
                //Medium attack
                if (ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0200 || ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0202 || ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0203) return 2 / 3;
                //Heavy attack
                if (ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0402 || ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0404 || ToUnsignedInteger(Read(Player2Addresses.AttackStrength)) == 0x0405) return 1;
            }
            return 0;
        }
        public double GetEnemyDirection() => GetPlayer2XPos() < GetPlayer1XPos() ? -1.0 : 1.0;

        /// <summary>
        /// Get horizontal absolute position of the character in a ratio between -1 and 1
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private double GetHorizontalPositionRatio(uint pos) => (pos - MIN_HORIZONTAL_POSITION) / (double)(MAX_HORIZONTAL_POSITION - MIN_HORIZONTAL_POSITION) * 2 - 1;

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
            var cacheToUse = frameCache;
            if (!cacheToUse.ContainsKey(addressData.Address))
                cacheToUse[addressData.Address] = emulator.ReadMemory(addressData.Address, addressData.Length);

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
                var cacheToUse = frameCache;
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

                var cacheToUse = frameCache;
                if (!cacheToUse.ContainsKey(address.Address))
                {
                    cacheToUse[address.Address] = data[dataIndex..(dataIndex + count)];
                    dataIndex += count;
                }

                bytes.AddRange(cacheToUse[address.Address]);
            }

            return bytes.ToArray();
        }

        private void InitFrameCache()
        {
            List<AddressData> toRead = new()
            {
                Player1Addresses.XPos,
                Player1Addresses.YPos,
                Player1Addresses.State,
                Player1Addresses.HP,
                Player1Addresses.EndRoundStatus,
                Player1Addresses.Input,
                Player1Addresses.AttackType,
                Player1Addresses.AttackStrength,
                Player2Addresses.XPos,
                Player2Addresses.YPos,
                Player2Addresses.State,
                Player2Addresses.HP,
                Player2Addresses.EndRoundStatus,
                Player2Addresses.Input,
                Player2Addresses.AttackType,
                Player2Addresses.AttackStrength,
                GameAddresses.RoundTimer,
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

        /// <summary>
        /// <br>Reads up to 8 bytes from the address, assuming byte-wise little endian, and interprets all nybbles as decimal digits.</br>
        /// <br>Examples:</br>
        /// <code>
        /// 0x563412 -> 123456
        /// 0x90     -> 90    
        /// 0x0180   -> 8001  
        /// 0x4      -> 4     
        /// 0xA      -> 10    
        /// </code>
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
    }
}
