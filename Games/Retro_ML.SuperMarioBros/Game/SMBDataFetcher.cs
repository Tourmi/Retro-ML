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

        private ushort currX;

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
        }

        /// <summary>
        /// Needs to be called every time a save state was loaded to reset the global cache.
        /// </summary>
        public void NextState()
        {
            frameCache.Clear();

            tileCache.Clear();

            internalClock.Reset();

            currX = 0;
        }

        public ushort GetPositionX() => (ushort)(ReadSingle(Player.MarioPositionX) + (ReadSingle(GameState.CurrentScreen) * 0x100));
        public byte GetPositionY() => ReadSingle(Player.MarioPositionY);
        public byte[] IsSpritePresent() => Read(Sprite.IsEnemyUpPresent);
        public byte[] GetSpritePosition() => Read(Sprite.EnemyPositions);
        public byte[] GetSprites() => Read(Sprite.EnemyType);
        public ushort IsPowerUpPresent() => ReadSingle(Sprite.IsPowerUpPresent);
        public byte[] GetPowerUpPosition() => Read(Sprite.PowerUpPositions);
        public bool IsOnGround() => ReadSingle(Player.MarioActionState) == 0;
        public bool IsInWater() => ReadSingle(Player.IsSwimming) == 0;
        public bool CanAct() => ReadSingle(Player.MarioState) == 0x8;
        public bool IsDead() => ReadSingle(Player.MarioActionState) == 0x0B || ReadSingle(Player.IsFalling) == 0x01;
        public bool WonLevel() => ReadSingle(Player.MarioState) == 0x04 || ReadSingle(Player.MarioState) == 0x05 || ReadSingle(GameState.WonCondition) == 0x02;
        public bool IsAtMaxSpeed() => ReadSingle(Player.MarioMaxVelocity) == 0x28;
        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public bool IsWaterLevel() => ReadSingle(GameState.LevelType) == 0x1;
        public int GetCoins() => (int)ToUnsignedInteger(Read(GameState.Coins));
        public int GetLives() => (int)ToUnsignedInteger(Read(GameState.Lives));
        public int GetScore() => (int)ToUnsignedInteger(Read(GameState.Score));
        public byte GetPowerUp() => ReadSingle(Player.MarioPowerupState);
        public bool IsFlashing() => ReadSingle(Player.MarioState) == 0xA;
        //Good tiles : 194 coins - 192 ? block with coins - 193 ? block with powerup - 93 block with many coins
        public int[] GoodTile = new int[] { 0xC0, 0xC1, 0xC2, 0x5D };

        /* Walkable sprite :
         * 0x24/0x25 - Static lift
         * 0x26/0x27 - Vertical going lift 
         * 0x28 - Horizontal going lift 
         * 0x29 - Static lift (Will Fall if Player stays on it for too long
         * 0x2A - Horizontal forward moving lift with strange hitbox
         * 0x2B/0x2C - Halves of double lift (like 1.2)
        */
        public int[] WalkableSprite = new int[] { 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C};

        /* Good Sprite
         * 0x2E - PowerUp Object
         * 0x2F - Vine Object
         * 0x30 - Flagpole Flag Object
         * 0x31 - StarFlag Object
         * 0x32 - Jump spring Object
         * 0x34 - Warpzone
         * 0x35 - Retainer Object
         */
        public int[] GoodSprite = new int[] { 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x34, 0x35 };

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

            //Find good sprites
            byte[] isSpriteUp = IsSpritePresent();

            byte[] spritePos = GetSpritePosition();

            byte[] spriteType = GetSprites();

            for (int i = 0; i < 5; i++)
            {
                //To get lift sprite
                if (isSpriteUp[i] != 0 && WalkableSprite.Contains(spriteType[i]))
                {
                    //Enemy X position in tile, to get tile mario is in instead of on the right
                    var goodSpriteXPos = (spritePos[i * 4] + (METATILE_SIZE / 2)) / METATILE_SIZE;

                    //Enemy Y position in tile, to get tile mario is in instead of under : 2 * METATILE_SIZE to adjust position
                    var goodSpriteYPos = (spritePos[(i * 4) + 1] - (2 * METATILE_SIZE)) / METATILE_SIZE;

                    //Mario X position in tile, to get tile mario is in instead of on the right
                    var xPos = (ushort)(ReadSingle(Player.MarioScreenPositionX) + (METATILE_SIZE / 2)) / METATILE_SIZE;

                    //Mario Y position in tile, to get tile mario is in instead of under
                    var yPos = (ushort)(ReadSingle(Player.MarioScreenPositionY) - METATILE_SIZE) / METATILE_SIZE;

                    var goodSpriteXDist = goodSpriteXPos - xPos;

                    var goodSpriteYDist = goodSpriteYPos - yPos;

                    //Is the sprite distance between the bounds that Mario can see?
                    if (goodSpriteXDist <= x_dist && goodSpriteYDist <= y_dist && goodSpriteXDist >= -x_dist && goodSpriteYDist >= -y_dist)
                    {
                        result[goodSpriteYDist + y_dist, goodSpriteXDist + x_dist] = true;
                    }
                }
            }

            return result;
        }

        public bool[,] GetDangerousTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            //To know how many sprites are present on the map / which memory slot to check for their position
            byte[] isSpriteUp = IsSpritePresent();

            byte[] spritePos = GetSpritePosition();

            byte[] spriteType = GetSprites();

            for (int i = 0; i < 5; i++)
            {
                //Sprite is enemy therefore is dangerous
                if (isSpriteUp[i] != 0 && !WalkableSprite.Contains(spriteType[i]) && !GoodSprite.Contains(spriteType[i]))
                {
                    //Enemy X position in tile, to get tile mario is in instead of on the right
                    var enemyXPos = (spritePos[i * 4] + (METATILE_SIZE / 2)) / METATILE_SIZE;

                    //Enemy Y position in tile, to get tile mario is in instead of under : 2 * METATILE_SIZE to adjust position
                    var enemyYPos = (spritePos[(i * 4) + 1] - (2 * METATILE_SIZE)) / METATILE_SIZE;

                    //Mario X position in tile, to get tile mario is in instead of on the right
                    var xPos = (ushort)(ReadSingle(Player.MarioScreenPositionX) + (METATILE_SIZE / 2)) / METATILE_SIZE;

                    //Mario Y position in tile, to get tile mario is in instead of under
                    var yPos = (ushort)(ReadSingle(Player.MarioScreenPositionY) - METATILE_SIZE) / METATILE_SIZE;

                    var enemyXDist = enemyXPos - xPos;

                    var enemyYDist = enemyYPos - yPos;

                    //Is the sprite distance between the bounds that Mario can see?
                    if (enemyXDist <= x_dist && enemyYDist <= y_dist && enemyXDist >= -x_dist && enemyYDist >= -y_dist)
                    {
                        result[enemyYDist + y_dist, enemyXDist + x_dist] = true;
                    }
                }
            }

            return result;
        }

        public bool[,] GetGoodTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var tiles = GetNearbyTiles(x_dist, y_dist);

            //Find good Tiles
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    if (GoodTile.Contains(tiles[i, j]))
                    {
                        result[i, j] = true;
                    }
                }
            }

            //Find powerups
            bool isPowerUp = IsPowerUpPresent() == 0x2E;

            byte[] powerUpPos = GetPowerUpPosition();

            if (isPowerUp)
            {
                //powerUp X position in tile, to get tile mario is in instead of on the right
                var powerUpXPos = (powerUpPos[0] + (METATILE_SIZE / 2)) / METATILE_SIZE;

                //powerUp Y position in tile, to get tile mario is in instead of under : 2 * METATILE_SIZE to adjust position
                var powerUpYPos = (powerUpPos[1] - (2 * METATILE_SIZE)) / METATILE_SIZE;

                //Mario X position in tile, to get tile mario is in instead of on the right
                var xPos = (ushort)(ReadSingle(Player.MarioScreenPositionX) + (METATILE_SIZE / 2)) / METATILE_SIZE;

                //Mario Y position in tile, to get tile mario is in instead of under
                var yPos = (ushort)(ReadSingle(Player.MarioScreenPositionY) - METATILE_SIZE) / METATILE_SIZE;

                var powerUpXDist = powerUpXPos - xPos;

                var powerUpYDist = powerUpYPos - yPos;

                //Is the sprite distance between the bounds that Mario can see?
                if (powerUpXDist <= x_dist && powerUpYDist <= y_dist && powerUpXDist >= -x_dist && powerUpYDist >= -y_dist)
                {
                    result[powerUpYDist + y_dist, powerUpXDist + x_dist] = true;
                }
            }

            //Find good sprites
            byte[] isSpriteUp = IsSpritePresent();

            byte[] spritePos = GetSpritePosition();

            byte[] spriteType = GetSprites();

            for (int i = 0; i < 5; i++)
            {
                //To get goodies sprite
                if (isSpriteUp[i] != 0 && GoodSprite.Contains(spriteType[i]))
                {
                    //Enemy X position in tile, to get tile mario is in instead of on the right
                    var goodSpriteXPos = (spritePos[i * 4] + (METATILE_SIZE / 2)) / METATILE_SIZE;

                    //Enemy Y position in tile, to get tile mario is in instead of under : 2 * METATILE_SIZE to adjust position
                    var goodSpriteYPos = (spritePos[(i * 4) + 1] - (2 * METATILE_SIZE)) / METATILE_SIZE;

                    //Mario X position in tile, to get tile mario is in instead of on the right
                    var xPos = (ushort)(ReadSingle(Player.MarioScreenPositionX) + (METATILE_SIZE / 2)) / METATILE_SIZE;

                    //Mario Y position in tile, to get tile mario is in instead of under
                    var yPos = (ushort)(ReadSingle(Player.MarioScreenPositionY) - METATILE_SIZE) / METATILE_SIZE;

                    var goodSpriteXDist = goodSpriteXPos - xPos;

                    var goodSpriteYDist = goodSpriteYPos - yPos;

                    //Is the sprite distance between the bounds that Mario can see?
                    if (goodSpriteXDist <= x_dist && goodSpriteYDist <= y_dist && goodSpriteXDist >= -x_dist && goodSpriteYDist >= -y_dist)
                    {
                        result[goodSpriteYDist + y_dist, goodSpriteXDist + x_dist] = true;
                    }
                }
            }

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
                    var xInPage = Math.Max((xPos + x) % PAGE_WIDTH, 0);

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
                var newX = GetPositionX();
                //Refresh every 6 tiles
                if (newX - currX >= (16*6))
                {
                tileCache.Clear();
                currX = newX;
                }
                cacheToUse = tileCache;
            }

            return cacheToUse;
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
