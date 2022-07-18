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
        private const uint METATILE_SIZE = 0x10;
        public const uint DESIRED_LEVEL_WIDTH = 0xD;
        public const uint DESIRED_LEVEL_HEIGHT = 0xB;
        public const uint LEVEL_TILES_WIDTH = 0x10;
        public const uint LEVEL_TILES_HEIGHT = 0x10;
        private const uint MIN_X_POS = 0x10;
        private const uint MIN_Y_POS = 0x10;
        private const uint MAX_X_POS = 0xD0;
        private const uint MAX_Y_POS = 0xB0;
        private const uint MAX_ACCELERATOR = 0x100;
        private const uint MAX_BOMB = 0xA;
        private const uint MAX_EXPLOSION_EXPANDER = 0xA;

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
        public byte[] GetBomberMansXPos() => Read(PlayersAddresses.XPos);
        public byte[] GetBomberMansYPos() => Read(PlayersAddresses.YPos);
        public byte[] GetTiles() => Read(GameAddresses.Tiles);
        public double GetPlayerXPositionNormalized() => GetXPositionNormalized(0);
        public double GetPlayerYPositionNormalized() => GetYPositionNormalized(0);
        public byte GetPlayerAFKTimer() => ReadSingle(PlayersAddresses.AFKTimer);
        public byte GetPlayerExtraBombPowerUpLevel() => ReadSingle(PowerupsAddresses.ExtraBomb);
        public byte GetPlayerExplosionExpanderPowerUpLevel() => ReadSingle(PowerupsAddresses.ExplosionExpander);
        public byte GetPlayerAcceleratorPowerUpLevel() => ReadSingle(PowerupsAddresses.Accelerator);
        public double GetPlayerExtraBombPowerUpLevelNormalized() => GetPlayerExtraBombPowerUpLevel() / (double) MAX_BOMB;
        public double GetPlayerExplosionExpanderPowerUpLevelNormalized() => GetPlayerExplosionExpanderPowerUpLevel() / (double) MAX_EXPLOSION_EXPANDER;
        public double GetPlayerAcceleratorPowerUpLevelNormalized() => GetPlayerAcceleratorPowerUpLevel() / (double)MAX_ACCELERATOR;
        public bool GetPlayerLouiePowerUpState() => ReadSingle(PowerupsAddresses.Louie) == 0;
        public bool GetPlayerPowerBombPowerUpState() => ReadSingle(PowerupsAddresses.PowerBomb) == 1;
        public bool GetPlayerPowerGlovesPowerUpState() => ReadSingle(PowerupsAddresses.PowerGloves) == 1;

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
        /// Get X position normalized position in the level
        /// </summary>
        public double GetXPositionNormalized(int index) => (GetBomberMansXPos()[index] - MIN_X_POS) / (double)(MAX_X_POS - MIN_X_POS);

        /// <summary>
        /// Get Y position normalized position in the level
        /// </summary>
        public double GetYPositionNormalized(int index) => (GetBomberMansYPos()[index] - MIN_Y_POS) / (double)(MAX_Y_POS - MIN_Y_POS);

        /// <summary>
        /// Return an array of double representing normalized horizontal distance between the player and every enemies 
        /// Our savestates contains 3 enemies. But the game lets you play vs 1 to 4 enemies. 
        /// </summary>
        public double[,] GetEnemiesXDistanceToThePlayer(uint enemyCount)
        {
            double[,] result = new double[enemyCount, 1];

            double playerXPosInLevel = GetPlayerXPositionNormalized();

            for (int enemy = 0 ; enemy < enemyCount; enemy++)
            {
                double enemyXPosInLevel = GetXPositionNormalized(enemy + 1);
                result[enemy, 0] = enemyXPosInLevel - playerXPosInLevel;
            }

            return result;
        }

        /// <summary>
        /// Return an array of double representing normalized vertical distance between the player and every enemies 
        /// Our savestates contains 3 enemies. But the game lets you play vs 1 to 4 enemies. 
        /// </summary>
        public double[,] GetEnemiesYDistanceToThePlayer(uint enemyCount)
        {
            double[,] result = new double[enemyCount, 1];

            double playerYPosInLevel = GetPlayerYPositionNormalized();

            for (int enemy = 0; enemy < enemyCount; enemy++)
            {
                double enemyYPosInLevel = GetYPositionNormalized(enemy + 1);
                result[enemy, 0] = enemyYPosInLevel - playerYPosInLevel;
            }

            return result;
        }

        /// <summary>
        /// Draw game tiles. There is 3 types of tiles to draw : undestructible, destructible and empty
        /// </summary>
        public double[,] DrawTiles()
        {
            double[,] result = new double[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];
            byte[] tiles = GetTiles();

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //Tile is undestructible
                    if (tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x80 || tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x84) result[i, j] = 1.0;
                    //Tile is destructible
                    else if (tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x30) result[i, j] = 0.5;
                    //Tile is free
                    else if (tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x00) result[i, j] = 0.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Draw dangers ; bombs and explosions.
        /// </summary>
        public double[,] DrawDangers()
        {
            double[,] result = new double[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];
            byte[] tiles = GetTiles();

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //Is a bomb or part of an explosion
                    if (tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x24 || tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x05 || tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x07 || tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x50) result[i, j] = 1.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Draw goodies that are mostly powerups.
        /// </summary>
        public double[,] DrawGoodies()
        {
            double[,] result = new double[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];
            byte[] tiles = GetTiles();

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //Is a bomb or part of an explosion
                    if (tiles[(i * LEVEL_TILES_WIDTH) + (j + 1)] == 0x10) result[i, j] = 1.0;
                }
            }

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
            GameAddresses.Tiles,
            PlayersAddresses.XPos,
            PlayersAddresses.YPos,
            PlayersAddresses.AFKTimer,
            PowerupsAddresses.ExplosionExpander,
            PowerupsAddresses.Accelerator,
            PowerupsAddresses.ExtraBomb,
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
