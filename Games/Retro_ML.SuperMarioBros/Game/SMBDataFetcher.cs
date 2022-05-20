using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioBros.Configuration;
using static Retro_ML.SuperMarioBros.Game.Addresses;

namespace Retro_ML.SuperMarioBros.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class SMBDataFetcher : IDataFetcher
    {
        private const int PAGE_SIZE = PAGE_HEIGHT * PAGE_WIDTH;
        private const int METATILE_SIZE = 0x10;
        private const int PAGE_WIDTH = 0x10;
        private const int PAGE_HEIGHT = 0xD;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> tileCache;

        private readonly byte[] currentTiles;

        private InternalClock internalClock;

        public SMBDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SMBPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            tileCache = new();
            currentTiles = new byte[0x1A0];
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

            tileCache.Clear();

            internalClock.Reset();
        }

        public ushort GetPositionX() => (ushort)(ReadSingle(Player.MarioPositionX) + (ReadSingle(GameState.CurrentScreen)*0x100));
        public byte GetPositionY() => ReadSingle(Player.MarioPositionY);
        public bool IsOnGround() => ToUnsignedInteger(Read(Player.MarioActionState)) == 0;
        public bool IsInWater() => ToUnsignedInteger(Read(Player.IsSwimming)) == 0;
        //Tochange
        public bool CanAct() => ToUnsignedInteger(Read(Player.MarioState)) != 8;
        public bool IsDead() => ToUnsignedInteger(Read(Player.MarioState)) == 11;
        public bool WonLevel() => ToUnsignedInteger(Read(Player.MarioActionState)) == 3;
        public bool IsAtMaxSpeed() => ToUnsignedInteger(Read(Player.MarioMaxVelocity)) == 0x28;
        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public bool IsWaterLevel() => ToUnsignedInteger(Read(GameState.LevelType)) == 01;
        public int GetCoins() => (int)ToUnsignedInteger(Read(GameState.Coins));
        public int GetLives() => (int)ToUnsignedInteger(Read(GameState.Lives));
        public int GetScore() => (int)ToUnsignedInteger(Read(GameState.Score));
        public byte GetPowerUp() => ReadSingle(Player.MarioPowerupState);
        public bool IsFlashing() => ToUnsignedInteger(Read(Player.MarioState)) == 10;

        public bool[,] GetWalkableTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var tiles = GetNearbyTiles(x_dist, y_dist);

            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    result[i, j] = tiles[i, j] != 0;
                }
            }

            return result;
        }

        public bool[,] GetDangerousTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];
            
            return result;
        }

        public bool[,] GetGoodTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            return result;
        }
        /// <summary>
        /// Get all tiles around a position
        /// </summary>
        private byte[,] GetNearbyTiles(int x_dist, int y_dist)
        {
            //Tile array representing current loaded tiles for 2 pages
            var tileArray = Read(GameState.Tiles);

            //Mario X position in tile, to get tile mario is in instead of on the right
            var xPos = (GetPositionX() + (METATILE_SIZE / 2)) / METATILE_SIZE;

            //Mario Y position in tile, to get tile mario is in instead of under
            var yPos = (GetPositionY() - METATILE_SIZE) / METATILE_SIZE;

            byte[,] result = new byte[y_dist * 2 + 1, x_dist * 2 + 1];

            for (int y = -y_dist; y <= y_dist; y++)
            {
                //Get valid Y pos for tile
                var yInPage = Math.Min(Math.Max(0, y + yPos), PAGE_HEIGHT - 1) * PAGE_WIDTH;

                for (int x = -x_dist; x <= x_dist; x++)
                {
                    var page = ((xPos + x) / PAGE_WIDTH) % 2;

                    //Get tile X pos
                    var xInPage = (xPos + x) % PAGE_WIDTH;

                    var index = (page * PAGE_SIZE) + yInPage + xInPage;

                    result[y + y_dist, x + x_dist] = tileArray[index];
                }
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

        private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData)
        {
            var cacheToUse = frameCache;

            if (addressData.CacheType == AddressData.CacheTypes.TilesCache)
            {
                tileCache.Clear();

                cacheToUse = tileCache;
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
