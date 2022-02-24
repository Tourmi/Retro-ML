using SMW_ML.Emulator;
using SMW_ML.Game.SuperMarioWorld.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static SMW_ML.Game.SuperMarioWorld.Addresses;

namespace SMW_ML.Game.SuperMarioWorld
{
    public class DataFetcher
    {
        public const int INTERNAL_CLOCK_LENGTH = 8;
        private const int TILE_SIZE = 0x10;
        private const int SCREEN_WIDTH = 0x10;
        private const int SCREEN_HEIGHT_HORIZONTAL = 0x1B;
        private const int SCREEN_HEIGHT_VERTICAL = 0x10;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> levelCache;

        private readonly Dictionary<ushort, ushort[]> map16Caches;
        private ushort[,]? nearbyTilesCache;
        private ushort[,]? nearbyLayer2TilesCache;
        private ushort currLevelNumber;

        private int internal_clock_timer = INTERNAL_CLOCK_LENGTH;
        private bool internalClockOn = false;

        public DataFetcher(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
            frameCache = new();
            levelCache = new();
            map16Caches = new Dictionary<ushort, ushort[]>();
        }

        /// <summary>
        /// Needs to be called every frame to reset the memory cache
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();
            if (internal_clock_timer <= 0)
            {
                internal_clock_timer = INTERNAL_CLOCK_LENGTH;
                internalClockOn = !internalClockOn;
            }
            internal_clock_timer--;

            nearbyTilesCache = null;
            nearbyLayer2TilesCache = null;
        }

        /// <summary>
        /// Needs to be called every level to reset the level cache.
        /// </summary>
        public void NextLevel()
        {
            NextFrame();
            levelCache.Clear();
            internal_clock_timer = INTERNAL_CLOCK_LENGTH;
            internalClockOn = false;
        }

        public uint GetPositionX() => ToUnsignedInteger(Read(Player.PositionX));
        public uint GetPositionY() => ToUnsignedInteger(Read(Player.PositionY));
        public bool IsOnGround() => ReadSingle(Player.IsOnGround) != 0 || ReadSingle(Player.IsOnSolidSprite) != 0;
        public bool CanAct() => ReadSingle(Player.PlayerAnimationState) == PlayerAnimationStates.NONE ||
                                ReadSingle(Player.PlayerAnimationState) == PlayerAnimationStates.FLASHING;
        public bool IsDead() => ReadSingle(Player.PlayerAnimationState) == PlayerAnimationStates.DYING;
        public bool WonLevel() => ReadSingle(Level.EndLevelTimer) != 0;
        public bool IsInWater() => ReadSingle(Player.IsInWater) != 0;
        public bool CanJumpOutOfWater() => ReadSingle(Player.CanJumpOutOfWater) != 0;
        public bool IsSinking() => ReadSingle(Player.AirFlag) == 0x24;
        public bool IsRaising() => ReadSingle(Player.AirFlag) == 0x0B;
        public bool IsCarryingSomething() => ReadSingle(Player.IsCarryingSomething) != 0;
        public bool CanClimb() => (ReadSingle(Player.CanClimb) & 0b00001011 | ReadSingle(Player.CanClimbOnAir)) != 0;
        public bool IsAtMaxSpeed() => ReadSingle(Player.DashTimer) == 0x70;
        public bool WasInternalClockTriggered() => internalClockOn;
        public bool WasDialogBoxOpened() => ReadSingle(Level.TextBoxTriggered) != 0;
        public bool IsWaterLevel() => ReadSingle(Level.IsWater) != 0;
        public int GetCoins() => ReadSingle(Counters.Coins);
        public int GetLives() => ReadSingle(Counters.Lives);
        public int GetYoshiCoins() => ReadSingle(Counters.YoshiCoinCollected);
        public int GetScore()
        {
            var scoreBytes = Read(Counters.Score);
            int score = 0;
            for (int i = 0; i < scoreBytes.Length; i++)
            {
                score += scoreBytes[i] << ((scoreBytes.Length - i - 1) * 8);
            }

            return score;
        }

        public bool[,] GetWalkableTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var spriteStatuses = Read(Addresses.Sprite.Statuses);
            var aliveIndexes = spriteStatuses.Select((s, i) => (s, i)).Where(si => SpriteStatuses.IsAlive(si.s)).Select(si => si.i).ToArray();
            var sprites = GetSprites(aliveIndexes);

            foreach (var sprite in sprites)
            {
                if (!SpriteNumbers.IsSolid(sprite.Number)) continue;
                SetSpriteTiles(x_dist, y_dist, result, sprite);
            }

            byte levelTileset = ReadSingle(Level.Header.TilesetSetting);
            var tileset = Tileset.GetWalkableTiles(levelTileset);
            var walkableTiles = GetTilesAroundPosition(x_dist, y_dist, tileset);

            for (int i = 0; i < walkableTiles.GetLength(0); i++)
            {
                for (int j = 0; j < walkableTiles.GetLength(1); j++)
                {
                    result[i, j] |= walkableTiles[i, j];
                }
            }

            return result;
        }

        public bool[,] GetDangerousTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var spriteStatuses = Read(Addresses.Sprite.Statuses);
            var aliveIndexes = spriteStatuses.Select((s, i) => (s, i)).Where(si => SpriteStatuses.CanBeDangerous(si.s)).Select(si => si.i).ToArray();
            var sprites = GetSprites(aliveIndexes);

            foreach (var sprite in sprites)
            {
                if (!SpriteNumbers.IsDangerous(sprite.Number)) continue;
                SetSpriteTiles(x_dist, y_dist, result, sprite);
            }

            var extendedSprites = GetExtendedSprites();
            foreach (var extendedSprite in extendedSprites)
            {
                if (!extendedSprite.IsDangerous()) continue;
                var xSpriteDist = (extendedSprite.XPos / TILE_SIZE - (int)GetPositionX() / TILE_SIZE);
                var ySpriteDist = (extendedSprite.YPos / TILE_SIZE - (int)GetPositionY() / TILE_SIZE);

                //Is the sprite distance between the bounds that Mario can see?
                if (xSpriteDist <= x_dist && ySpriteDist <= y_dist && xSpriteDist >= -x_dist && ySpriteDist >= -y_dist)
                {
                    result[ySpriteDist + y_dist, xSpriteDist + x_dist] = true;
                }
            }

            byte levelTileset = ReadSingle(Level.Header.TilesetSetting);
            var tileset = Tileset.GetDangerousTiles(levelTileset);

            var dangerousTiles = GetTilesAroundPosition(x_dist, y_dist, tileset);
            for (int i = 0; i < dangerousTiles.GetLength(0); i++)
            {
                for (int j = 0; j < dangerousTiles.GetLength(1); j++)
                {
                    result[i, j] |= dangerousTiles[i, j];
                }
            }

            return result;
        }

        public bool[,] GetGoodTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];

            var spriteStatuses = Read(Addresses.Sprite.Statuses);
            var aliveIndexes = spriteStatuses.Select((s, i) => (s, i)).Where(si => SpriteStatuses.IsAlive(si.s)).Select(si => si.i).ToArray();
            var sprites = GetSprites(aliveIndexes);

            foreach (var sprite in sprites)
            {
                if (!SpriteNumbers.IsGood(sprite.Number)) continue;
                SetSpriteTiles(x_dist, y_dist, result, sprite);
            }

            byte levelTileset = ReadSingle(Level.Header.TilesetSetting);
            var tileset = Tileset.GetGoodTiles(levelTileset);

            var dangerousTiles = GetTilesAroundPosition(x_dist, y_dist, tileset);
            for (int i = 0; i < dangerousTiles.GetLength(0); i++)
            {
                for (int j = 0; j < dangerousTiles.GetLength(1); j++)
                {
                    result[i, j] |= dangerousTiles[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the tiles in <paramref name="tilesArray"/> to true based on the <paramref name="sprite"/>'s position and size.
        /// </summary>
        /// <param name="x_dist"></param>
        /// <param name="y_dist"></param>
        /// <param name="tilesArray"></param>
        /// <param name="sprite"></param>
        private void SetSpriteTiles(int x_dist, int y_dist, bool[,] tilesArray, Data.Sprite sprite)
        {
            var clipping = sprite.GetSpriteClipping();
            var rotationOffset = sprite.GetSpriteRotationOffset();
            int minX = sprite.XPos + clipping.X + rotationOffset.X;
            int minY = sprite.YPos + clipping.Y + rotationOffset.Y;
            int maxX = minX + clipping.Width - 1;
            int maxY = minY + clipping.Height - 1;

            int marioXPosition = (int)GetPositionX() / TILE_SIZE;
            int marioYPosition = (int)(GetPositionY() + TILE_SIZE) / TILE_SIZE;

            var xMinDist = (minX / TILE_SIZE) - marioXPosition;
            var yMinDist = (minY / TILE_SIZE) - marioYPosition;
            var xMaxDist = (maxX / TILE_SIZE) - marioXPosition;
            var yMaxDist = (maxY / TILE_SIZE) - marioYPosition;

            for (int ySpriteDist = yMinDist; ySpriteDist <= yMaxDist; ySpriteDist++)
            {
                for (int xSpriteDist = xMinDist; xSpriteDist <= xMaxDist; xSpriteDist++)
                {
                    //Is the sprite distance between the bounds that Mario can see?
                    if (xSpriteDist <= x_dist && ySpriteDist <= y_dist && xSpriteDist >= -x_dist && ySpriteDist >= -y_dist)
                    {
                        tilesArray[ySpriteDist + y_dist, xSpriteDist + x_dist] = true;
                    }
                }
            }
        }

        private bool[,] GetTilesAroundPosition(int x_dist, int y_dist, IEnumerable<ushort> tileset)
        {
            bool[,] result = new bool[y_dist * 2 + 1, x_dist * 2 + 1];
            ushort levelNumber = GetLevelNumber();

            if (!map16Caches.ContainsKey(levelNumber))
            {
                map16Caches[levelNumber] = ReadLowHighBytes(Level.Map16);
            }

            if (nearbyTilesCache == null)
            {
                nearbyTilesCache = GetNearbyTiles(map16Caches[levelNumber], x_dist, y_dist, (int)GetPositionX() / TILE_SIZE, (int)(GetPositionY() + TILE_SIZE) / TILE_SIZE, 0);
            }

            //If layer 2 is interactive
            if (nearbyLayer2TilesCache == null && (ReadSingle(Level.ScreenMode) & 0b10000000) != 0)
            {
                bool isLayer2Vertical = (ReadSingle(Level.ScreenMode) & 0b00000010) != 0;
                int layer2ScreenStart = isLayer2Vertical ? 0x0E : 0x10;
                int offsetX = (int)GetPositionX()
                    + ((int)ToUnsignedInteger(Read(Level.Layer2X)) - (int)ToUnsignedInteger(Read(Level.Layer1X)));
                int offsetY = (int)(GetPositionY() + TILE_SIZE)
                    + ((int)ToUnsignedInteger(Read(Level.Layer2Y)) - (int)ToUnsignedInteger(Read(Level.Layer1Y)));

                nearbyLayer2TilesCache = GetNearbyTiles(map16Caches[levelNumber], x_dist, y_dist, offsetX / TILE_SIZE, offsetY / TILE_SIZE, layer2ScreenStart);
            }

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = tileset.Contains(nearbyTilesCache[i, j]);
                    if (nearbyLayer2TilesCache != null)
                    {
                        result[i, j] |= tileset.Contains(nearbyLayer2TilesCache[i, j]);
                    }
                }
            }

            return result;
        }

        private ushort[,] GetNearbyTiles(ushort[] map16Cache, int x_dist, int y_dist, int offsetX, int offsetY, int screenStart)
        {
            ushort[,] result = new ushort[y_dist * 2 + 1, x_dist * 2 + 1];

            byte screensCount = ReadSingle(Level.ScreenCount);

            bool isVertical = (ReadSingle(Level.ScreenMode) & 0b00000001) != 0;
            if (isVertical)
            {
                offsetY += screenStart * SCREEN_HEIGHT_VERTICAL;

                for (int y = -y_dist; y <= y_dist; y++)
                {
                    //Snap the Y coordinate to a valid tile
                    var tileYToRead = Math.Min(Math.Max(screenStart * SCREEN_HEIGHT_VERTICAL, y + offsetY), (screenStart * SCREEN_HEIGHT_VERTICAL) + SCREEN_HEIGHT_VERTICAL * screensCount - 1);
                    for (int x = -x_dist; x <= x_dist; x++)
                    {
                        //Snap the X coordinate to a valid tile
                        var tileXToRead = Math.Min(Math.Max(0x00, x + offsetX), SCREEN_WIDTH * 2 - 1);

                        var index = SCREEN_HEIGHT_VERTICAL * SCREEN_WIDTH * 2 * (tileYToRead / SCREEN_WIDTH) + SCREEN_HEIGHT_VERTICAL * SCREEN_WIDTH * (tileXToRead / SCREEN_WIDTH) + (tileYToRead % SCREEN_HEIGHT_VERTICAL) * SCREEN_WIDTH + tileXToRead % SCREEN_WIDTH;
                        result[y + y_dist, x + x_dist] = map16Cache[index];
                    }
                }
            }
            else
            {
                offsetX += screenStart * SCREEN_WIDTH;
                for (int y = -y_dist; y <= y_dist; y++)
                {
                    //Snap the Y coordinate to a valid tile
                    var tileYToRead = Math.Min(Math.Max(0x00, y + offsetY), SCREEN_HEIGHT_HORIZONTAL - 1);
                    for (int x = -x_dist; x <= x_dist; x++)
                    {
                        //Snap the X coordinate to a valid tile
                        var tileXToRead = Math.Min(Math.Max(screenStart * SCREEN_WIDTH, x + offsetX), (screenStart * SCREEN_WIDTH) + SCREEN_WIDTH * screensCount - 1);

                        var index = SCREEN_HEIGHT_HORIZONTAL * SCREEN_WIDTH * (tileXToRead / SCREEN_WIDTH) + tileYToRead * SCREEN_WIDTH + (tileXToRead % SCREEN_WIDTH);
                        result[y + y_dist, x + x_dist] = map16Cache[index];
                    }
                }
            }

            return result;
        }

        private Data.Sprite[] GetSprites(int[] indexes)
        {
            var sprites = new Data.Sprite[indexes.Length];

            byte[] numbers = Read(Addresses.Sprite.SpriteNumbers);
            ushort[] xPositions = ReadLowHighBytes(Addresses.Sprite.XPositions);
            ushort[] yPositions = ReadLowHighBytes(Addresses.Sprite.YPositions);
            byte[] props2 = Read(Addresses.Sprite.SpritesProperties2);
            byte[] misc151C = Read(Addresses.Sprite.Misc151C);
            byte[] misc1528 = Read(Addresses.Sprite.Misc1528);
            byte[] misc1602 = Read(Addresses.Sprite.Misc1602);
            byte[] misc187B = Read(Addresses.Sprite.Misc187B);

            for (int i = 0; i < indexes.Length; i++)
            {
                sprites[i] = new Data.Sprite
                {
                    Number = numbers[indexes[i]],
                    XPos = xPositions[indexes[i]],
                    YPos = yPositions[indexes[i]],
                    Properties2 = props2[indexes[i]],
                    Misc151C = misc151C[indexes[i]],
                    Misc1528 = misc1528[indexes[i]],
                    Misc1602 = misc1602[indexes[i]],
                    Misc187B = misc187B[indexes[i]]
                };
            }

            return sprites;
        }

        private Data.ExtendedSprite[] GetExtendedSprites()
        {
            var extendedSprites = new Data.ExtendedSprite[Addresses.ExtendedSprite.Numbers.Length];
            byte[] numbers = Read(Addresses.ExtendedSprite.Numbers);
            ushort[] xPositions = ReadLowHighBytes(Addresses.ExtendedSprite.XPositions);
            ushort[] yPositions = ReadLowHighBytes(Addresses.ExtendedSprite.YPositions);

            for (int i = 0; i < extendedSprites.Length; i++)
            {
                extendedSprites[i] = new Data.ExtendedSprite()
                {
                    Number = numbers[i],
                    XPos = xPositions[i],
                    YPos = yPositions[i]
                };
            }

            return extendedSprites;
        }

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
        /// Which cache to use depending on the AddressData
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>
        private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData)
        {
            var cacheToUse = frameCache;
            if (addressData.CacheDuration == AddressData.CacheDurations.Level)
            {
                // If the level number changed, we need to reset the level cache
                if (currLevelNumber != GetLevelNumber()) levelCache.Clear();
                cacheToUse = levelCache;
                currLevelNumber = GetLevelNumber();
            }

            return cacheToUse;
        }

        /// <summary>
        /// Returns the current level's number
        /// </summary>
        /// <returns></returns>
        private ushort GetLevelNumber()
        {
            ushort result = ReadSingle(Level.Number);
            if (result > 0x24) result += 0xDC;

            return result;
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
