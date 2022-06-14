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
        private readonly Dictionary<uint, byte[]> levelCache;

        private ushort currScreen;

        private InternalClock internalClock;

        public SMBDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SMBPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            tileCache = new();
            levelCache = new();
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

            levelCache.Clear();

            internalClock.Reset();

            currScreen = 0;
        }

        public byte[] GetTileArray() => Read(GameAddresses.Tiles);
        public ushort GetCurrentScreen() => ReadSingle(GameAddresses.CurrentScreen);
        public ushort GetMarioPositionX() => (ushort)(ReadSingle(PlayerAddresses.MarioPositionX) + (GetCurrentScreen() * 0x100));
        public byte GetMarioPositionY() => ReadSingle(PlayerAddresses.MarioPositionY);
        public ushort GetMarioScreenPositionX() => ReadSingle(PlayerAddresses.MarioScreenPositionX);
        public ushort GetMarioScreenPositionY() => ReadSingle(PlayerAddresses.MarioScreenPositionY);
        public byte[] IsSpritePresent() => Read(SpriteAddresses.IsSpritePresent);
        public byte[] GetSpriteHitbox() => Read(SpriteAddresses.SpriteHitbox);
        public byte[] GetSpritePositionX() => Read(SpriteAddresses.SpritePositionX);
        public byte[] GetSpritePositionY() => Read(SpriteAddresses.SpritePositionY);
        public byte[] GetSpriteScreenPosition() => Read(SpriteAddresses.SpriteScreenPosition);
        public byte[] GetFirebarAngle() => Read(SpriteAddresses.FirebarSpinAngle);
        public byte[] GetSprites() => Read(SpriteAddresses.SpriteType);
        public byte[] IsHammerPresent() => Read(SpriteAddresses.IsHammerPresent);
        public byte[] GetHammerHitbox() => Read(SpriteAddresses.HammerHitbox);
        public ushort IsPowerUpPresent() => ReadSingle(SpriteAddresses.IsPowerUpPresent);
        public byte[] GetPowerUpHitbox() => Read(SpriteAddresses.PowerUpHitbox);
        public bool IsOnGround() => ReadSingle(PlayerAddresses.MarioFloatState) == 0x00;
        public bool IsInWater() => ReadSingle(PlayerAddresses.IsSwimming) == 0x00;
        public bool CanAct() => ReadSingle(PlayerAddresses.MarioActionState) == 0x08;
        public bool IsDead() => ReadSingle(PlayerAddresses.MarioActionState) == 0x0B || ReadSingle(PlayerAddresses.IsFallingToDeath) == 0x01;
        public bool WonLevel() => ReadSingle(PlayerAddresses.MarioActionState) == 0x04 || ReadSingle(PlayerAddresses.MarioActionState) == 0x05 || ReadSingle(GameAddresses.WonCondition) == 0x02 || ReadSingle(PlayerAddresses.MarioFloatState) == 0x03;
        public bool IsAtMaxSpeed() => IsInWater() ? ReadSingle(PlayerAddresses.MarioMaxVelocity) == 0x18 : ReadSingle(PlayerAddresses.MarioMaxVelocity) == 0x28;
        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public bool IsWaterLevel() => ReadSingle(GameAddresses.LevelType) == 0x01;
        public int GetCoins() => (int)ToUnsignedInteger(Read(GameAddresses.Coins));
        public int GetLives() => (int)ToUnsignedInteger(Read(GameAddresses.Lives));
        public byte GetPowerUp() => ReadSingle(PlayerAddresses.MarioPowerupState);
        public bool IsFlashing() => ReadSingle(PlayerAddresses.MarioActionState) == 0x0A;

        /// <summary>
        /// Draw the sprites tiles based on the sprite number and position, depending on its hitbox and offset to Mario.
        /// </summary>
        /// <param name="x_dist"></param>
        /// <param name="y_dist"></param>
        /// <param name="tilesArray"></param>
        /// <param name="spriteNum"></param>
        /// <param name="spriteHitbox"></param>
        private void DrawSpriteTiles(int x_dist, int y_dist, bool[,] tilesArray, int spriteNum, byte[] spriteHitbox)
        {
            var spriteXPosMin = (spriteHitbox[spriteNum * 4] + (METATILE_SIZE / 2)) / METATILE_SIZE;
            var spriteYPosMin = (spriteHitbox[(spriteNum * 4) + 1] - (2 * METATILE_SIZE)) / METATILE_SIZE;
            var spriteXPosMax = (spriteHitbox[(spriteNum * 4) + 2] + (METATILE_SIZE / 2)) / METATILE_SIZE;
            var spriteYPosMax = (spriteHitbox[(spriteNum * 4) + 3] - (2 * METATILE_SIZE)) / METATILE_SIZE;

            //Mario X position in tile, to get tile mario is in instead of on the right
            var xPos = (GetMarioScreenPositionX() + (METATILE_SIZE / 2)) / METATILE_SIZE;

            //Mario Y position in tile, to get tile mario is in instead of under
            var yPos = (GetMarioScreenPositionY() - METATILE_SIZE) / METATILE_SIZE;

            var spriteXDistMin = spriteXPosMin - xPos;
            var spriteYDistMin = spriteYPosMin - yPos;
            var spriteXDistMax = spriteXPosMax - xPos;
            var spriteYDistMax = spriteYPosMax - yPos;

            //Draw the sprite if the sprite distance is between the bounds that Mario can see
            for (int ySpriteDist = spriteYDistMin; ySpriteDist <= spriteYDistMax; ySpriteDist++)
                for (int xSpriteDist = spriteXDistMin; xSpriteDist <= spriteXDistMax; xSpriteDist++)
                    //Is the sprite distance between the bounds that Mario can see?
                    if (xSpriteDist <= x_dist && ySpriteDist <= y_dist && xSpriteDist >= -x_dist && ySpriteDist >= -y_dist)
                        tilesArray[ySpriteDist + y_dist, xSpriteDist + x_dist] = true;
        }
        /// <summary>
        /// Draw the firebar based on the sprite number and position, depending on its offset to mario.
        /// </summary>
        /// <param name="x_dist"></param>
        /// <param name="y_dist"></param>
        /// <param name="tilesArray"></param>
        /// <param name="firebarNum"></param>
        /// <param name="isLong"></param>
        private void DrawFirebarTiles(int x_dist, int y_dist, bool[,] tilesArray, int firebarNum, bool isLong = false)
        {
            byte[] firebarPosX = GetSpritePositionX();
            byte[] firebarPosY = GetSpritePositionY();
            byte[] firebarAngle = GetFirebarAngle();
            byte[] firebarScreen = GetSpriteScreenPosition();
            //Normal firebar is made of 6 fireballs = 3 tiles long - Long firebar is made of 12 fireballs = 6 tiles long
            var firebarLength = (isLong) ? 6 : 3;

            //To Convert the angle (0-32) to radians in order to find MaxY/X coordinates of the spinning firebar
            var convertedFirebarAngle = firebarAngle[firebarNum] * (2 * Math.PI / 32);

            double firebarXPosMin = (firebarPosX[firebarNum] + (firebarScreen[firebarNum] * 0x100) + (METATILE_SIZE / 2)) / METATILE_SIZE;
            double firebarYPosMin = (firebarPosY[firebarNum] - (2 * METATILE_SIZE)) / METATILE_SIZE;

            //Get X coord of firebar depending on the angle.
            double firebarXPosMax = firebarXPosMin + (firebarLength * Math.Sin(convertedFirebarAngle));

            //Get Y coord of firebar depending on the angle.
            double firebarYPosMax = firebarYPosMin - (firebarLength * Math.Cos(convertedFirebarAngle));

            //Make sure to draw on the left side if negative value
            if (firebarXPosMin > firebarXPosMax)
                (firebarXPosMin, firebarXPosMax) = (firebarXPosMax, firebarXPosMin);

            //Make sure to draw on the upper side if negative value
            if (firebarYPosMin > firebarYPosMax)
                (firebarYPosMin, firebarYPosMax) = (firebarYPosMax, firebarYPosMin);

            //Mario X position in tile, to get tile mario is in instead of on the right
            var xPos = (GetMarioPositionX() + (METATILE_SIZE / 2)) / METATILE_SIZE;

            //Mario Y position in tile, to get tile mario is in instead of under
            var yPos = (GetMarioScreenPositionY() - METATILE_SIZE) / METATILE_SIZE;

            var firebarXDistMin = firebarXPosMin - xPos;
            var firebarYDistMin = firebarYPosMin - yPos;
            var firebarXDistMax = firebarXPosMax - xPos;
            var firebarYDistMax = firebarYPosMax - yPos;

            //Draw the sprite if the sprite distance is between the bounds that Mario can see
            for (int yFirebarDist = (int)firebarYDistMin; yFirebarDist <= firebarYDistMax; yFirebarDist++)
                for (int xFirebarDist = (int)firebarXDistMin; xFirebarDist <= firebarXDistMax; xFirebarDist++)
                    //Is the sprite distance between the bounds that Mario can see?
                    if (xFirebarDist <= x_dist && yFirebarDist <= y_dist && xFirebarDist >= -x_dist && yFirebarDist >= -y_dist)
                        tilesArray[yFirebarDist + y_dist, xFirebarDist + x_dist] = true;
        }
        /// <summary>
        /// Get walkable solid tiles around a position 
        /// </summary>
        public bool[,] GetWalkableTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var tiles = GetNearbyTiles(x_dist, y_dist);

            for (int i = 0; i < tiles.GetLength(0); i++)
                for (int j = 0; j < tiles.GetLength(1); j++)
                    result[i, j] = tiles[i, j] != 0;

            //Find good sprites and draw them
            byte[] isSpriteUp = IsSpritePresent();
            byte[] spriteType = GetSprites();

            for (int i = 0; i < 5; i++)
                //To get lift sprite
                if (isSpriteUp[i] != 0 && Data.Sprite.WalkableSprite.Contains(spriteType[i]))
                    DrawSpriteTiles(x_dist, y_dist, result, i, GetSpriteHitbox());

            return result;
        }
        /// <summary>
        /// Get dangerous tiles around a position
        /// can be ennemies, dangerous sprites like birebars and hammers
        /// </summary>
        public bool[,] GetDangerousTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            //To know how many sprites are present on the map / which type they are / which memory slot to check for their position
            byte[] isSpriteUp = IsSpritePresent();
            byte[] spriteType = GetSprites();
            byte[] isHammerUp = IsHammerPresent();

            //For ennemies and firebar
            for (int i = 0; i < 5; i++)
            {
                if (isSpriteUp[i] == 0)
                    continue;
                //Sprite is firebar
                if (Data.Sprite.FireBarSprite.Contains(spriteType[i])) DrawFirebarTiles(x_dist, y_dist, result, i);
                //Sprite is long firebar
                if (Data.Sprite.LongFireBarSprite.Contains(spriteType[i]))
                    DrawFirebarTiles(x_dist, y_dist, result, i, true);
                //Sprite is enemy
                if (!Data.Sprite.WalkableSprite.Contains(spriteType[i]) && !Data.Sprite.GoodSprite.Contains(spriteType[i]) && !Data.Sprite.FireBarSprite.Contains(spriteType[i]))
                    DrawSpriteTiles(x_dist, y_dist, result, i, GetSpriteHitbox());
            }

            //For hammers
            for (int i = 0; i < 9; i++)
                if (isHammerUp[i] != 0)
                    DrawSpriteTiles(x_dist, y_dist, result, i, GetHammerHitbox());

            return result;
        }
        /// <summary>
        /// Get good tiles around a position
        /// can be coins, powerups and beneficial sprites like lifts
        /// </summary>
        public bool[,] GetGoodTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var tiles = GetNearbyTiles(x_dist, y_dist);

            //Find good Tiles
            for (int i = 0; i < tiles.GetLength(0); i++)
                for (int j = 0; j < tiles.GetLength(1); j++)
                    if (Data.Tiles.GoodTile.Contains(tiles[i, j]))
                        result[i, j] = true;

            //Draw powerups
            var isPowerUp = IsPowerUpPresent();

            if (isPowerUp == Data.Sprite.PowerupSprite)
                DrawSpriteTiles(x_dist, y_dist, result, 0, GetPowerUpHitbox());

            //Draw good sprites like flag, trampoline, etc
            byte[] isSpriteUp = IsSpritePresent();
            byte[] spriteType = GetSprites();

            for (int i = 0; i < 5; i++)
                if (isSpriteUp[i] != 0 && Data.Sprite.GoodSprite.Contains(spriteType[i]))
                    DrawSpriteTiles(x_dist, y_dist, result, i, GetSpriteHitbox());

            return result;
        }
        /// <summary>
        /// Get all tiles around a position
        /// </summary>
        private byte[,] GetNearbyTiles(int x_dist, int y_dist)
        {
            byte[,] result = new byte[y_dist * 2 + 1, x_dist * 2 + 1];

            //Tile array representing current loaded tiles for 2 pages
            var tileArray = GetTileArray();

            //Mario X and Y position in tile, to get right tile mario is in
            var xPos = (GetMarioPositionX() + (METATILE_SIZE / 2)) / METATILE_SIZE;
            var yPos = (GetMarioPositionY() - METATILE_SIZE) / METATILE_SIZE;

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
            var cacheToUse = frameCache;

            if (addressData.CacheType == AddressData.CacheTypes.TilesCache)
            {
                //Current game screen
                var newScreen = GetCurrentScreen();
                //Refresh every screen changes
                if (newScreen - currScreen > 0)
                {
                    tileCache.Clear();
                    currScreen = newScreen;
                }
                cacheToUse = tileCache;
            }

            if (addressData.CacheType == AddressData.CacheTypes.Level)
            {
                cacheToUse = levelCache;
            }

            return cacheToUse;
        }

        private void InitFrameCache()
        {
            (AddressData, bool)[] toRead = new (AddressData, bool)[]
            {
                (GameAddresses.CurrentScreen, false),
                (GameAddresses.Coins, false),
                (GameAddresses.Lives, false),
                (GameAddresses.WonCondition, false),
                (PlayerAddresses.MarioPositionX, false),
                (PlayerAddresses.MarioPositionY, false),
                (PlayerAddresses.MarioScreenPositionX, false),
                (PlayerAddresses.MarioActionState, false),
                (PlayerAddresses.IsSwimming, false),
                (PlayerAddresses.IsFallingToDeath, false),
                (PlayerAddresses.MarioMaxVelocity, false),
                (PlayerAddresses.MarioFloatState, false),
                (PlayerAddresses.MarioPowerupState, false),
                (SpriteAddresses.IsSpritePresent, false),
                (SpriteAddresses.SpriteHitbox, false),
                (SpriteAddresses.SpritePositionX, false),
                (SpriteAddresses.SpritePositionY, false),
                (SpriteAddresses.SpriteScreenPosition, false),
                (SpriteAddresses.FirebarSpinAngle, false),
                (SpriteAddresses.SpriteType, false),
                (SpriteAddresses.IsHammerPresent, false),
                (SpriteAddresses.HammerHitbox, false),
                (SpriteAddresses.IsPowerUpPresent, false),
                (SpriteAddresses.PowerUpHitbox, false),
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
