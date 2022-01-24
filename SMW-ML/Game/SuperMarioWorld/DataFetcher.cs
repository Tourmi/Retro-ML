using SMW_ML.Emulator;
using SMW_ML.Game.SuperMarioWorld.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SMW_ML.Game.SuperMarioWorld.Addresses;

namespace SMW_ML.Game.SuperMarioWorld
{
    public class DataFetcher
    {
        private const int INTERNAL_CLOCK_LENGTH = 3;
        private const int TILE_SIZE = 0x10;
        private const int SCREEN_WIDTH = 0x10;
        private const int SCREEN_HEIGHT_HORIZONTAL = 0x1B;
        private const int SCREEN_HEIGHT_VERTICAL = 0x10;

        private readonly HashSet<ushort> walkableTiles;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> levelCache;

        private readonly Dictionary<ushort, ushort[]> map16Caches;
        private ushort[,]? nearbyTilesCache;

        private int internal_clock_timer = INTERNAL_CLOCK_LENGTH;

        public DataFetcher(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
            frameCache = new();
            levelCache = new();
            map16Caches = new Dictionary<ushort, ushort[]>();

            walkableTiles = new HashSet<ushort>();
            walkableTiles.UnionWith(Enumerable.Range(0x100, 0x12F - 0x100).Select(i => (ushort)i));
            walkableTiles.UnionWith(Enumerable.Range(0x130, 0x16E - 0x130).Select(i => (ushort)i));
        }

        /// <summary>
        /// Needs to be called every frame to reset the memory cache
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();
            internal_clock_timer--;
            if (internal_clock_timer < 0)
            {
                internal_clock_timer = INTERNAL_CLOCK_LENGTH;
            }

            nearbyTilesCache = null;
        }

        /// <summary>
        /// Needs to be called every level to reset the level cache.
        /// </summary>
        public void NextLevel()
        {
            levelCache.Clear();
            internal_clock_timer = INTERNAL_CLOCK_LENGTH;
        }

        public uint GetPositionX() => ToUnsignedInteger(Read(Player.PositionX));
        public uint GetPositionY() => ToUnsignedInteger(Read(Player.PositionY));
        public bool IsOnGround() => ReadSingle(Player.IsOnGround) != 0 || ReadSingle(Player.IsOnSolidSprite) != 0;
        public bool CanAct() => ReadSingle(Player.PlayerAnimationState) == PlayerAnimationStates.NONE;
        public bool IsDead() => ReadSingle(Player.PlayerAnimationState) == PlayerAnimationStates.DYING;
        public bool WonLevel() => ReadSingle(Level.EndLevelTimer) != 0;
        public bool IsInWater() => ReadSingle(Player.IsInWater) != 0;
        public bool CanJumpOutOfWater() => ReadSingle(Player.CanJumpOutOfWater) != 0;
        public bool IsSinking() => ReadSingle(Player.AirFlag) == 0x24;
        public bool IsRaising() => ReadSingle(Player.AirFlag) == 0x0B;
        public bool IsCarryingSomething() => ReadSingle(Player.IsCarryingSomething) != 0;
        public bool CanClimb() => (ReadSingle(Player.CanClimb) & 0b00001011 | ReadSingle(Player.CanClimbOnAir)) != 0;
        public bool IsAtMaxSpeed() => ReadSingle(Player.DashTimer) == 0x70;
        public bool WasInternalClockTriggered() => internal_clock_timer == 0;
        public bool WasDialogBoxOpened() => ReadSingle(Level.TextBoxTriggered) != 0;
        public bool[,] GetWalkableTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[x_dist * 2 + 1, y_dist * 2 + 1];
            ushort levelNumber = ReadSingle(Level.Number);
            if (levelNumber > 0x24) levelNumber += 0xDC;

            if (!map16Caches.ContainsKey(levelNumber))
            {
                map16Caches[levelNumber] = ReadLowHighBytes(Level.Map16);
            }

            if (nearbyTilesCache == null)
            {
                nearbyTilesCache = GetNearbyTiles(map16Caches[levelNumber], x_dist, y_dist);
            }

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = walkableTiles.Contains(nearbyTilesCache[i, j]);
                }
            }

            return result;
        }
        public bool[,] GetDangerousTilesAroundPosition(int x_dist, int y_dist)
        {
            bool[,] result = new bool[x_dist * 2 + 1, y_dist * 2 + 1];

            var spriteStatuses = Read(Addresses.Sprite.Statuses);
            var aliveIndexes = spriteStatuses.Select((s, i) => (s, i)).Where(si => SpriteStatuses.CanBeDangerous(si.s)).Select(si => si.i).ToArray();
            var sprites = GetSprites(aliveIndexes);

            foreach (var sprite in sprites)
            {
                if (!SpriteNumbers.IsDangerous(sprite.Number)) continue;
                var xSpriteDist = (sprite.XPos / TILE_SIZE - (int)GetPositionX() / TILE_SIZE);
                var ySpriteDist = (sprite.YPos / TILE_SIZE - (int)GetPositionY() / TILE_SIZE);

                //Is the sprite distance between the bounds that Mario can see?
                if (xSpriteDist <= x_dist && ySpriteDist <= y_dist && xSpriteDist >= -x_dist && ySpriteDist >= -y_dist)
                {
                    result[ySpriteDist + y_dist, xSpriteDist + x_dist] = true;
                }
            }

            return result;
        }

        private ushort[,] GetNearbyTiles(ushort[] map16Cache, int x_dist, int y_dist)
        {
            ushort[,] result = new ushort[x_dist * 2 + 1, y_dist * 2 + 1];
            //We add half a tile to get the middle of the player's tile
            var offsetX = GetPositionX() / TILE_SIZE;
            var offsetY = (GetPositionY() + TILE_SIZE) / TILE_SIZE;

            byte screensCount = ReadSingle(Level.ScreenCount);

            bool isVertical = (ReadSingle(Level.ScreenMode) & 0b00000001) != 0;
            if (isVertical)
            {
                for (int y = -y_dist; y <= y_dist; y++)
                {
                    //Snap the Y coordinate to a valid tile
                    var tileYToRead = Math.Min(Math.Max(0x00, y + offsetY), SCREEN_HEIGHT_VERTICAL * screensCount - 1);
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
                for (int y = -y_dist; y <= y_dist; y++)
                {
                    //Snap the Y coordinate to a valid tile
                    var tileYToRead = Math.Min(Math.Max(0x00, y + offsetY), SCREEN_HEIGHT_HORIZONTAL - 1);
                    for (int x = -x_dist; x <= x_dist; x++)
                    {
                        //Snap the X coordinate to a valid tile
                        var tileXToRead = Math.Min(Math.Max(0x00, x + offsetX), SCREEN_WIDTH * screensCount - 1);

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

            for (int i = 0; i < indexes.Length; i++)
            {
                sprites[i] = new Data.Sprite
                {
                    Number = numbers[indexes[i]],
                    XPos = xPositions[indexes[i]],
                    YPos = yPositions[indexes[i]]
                };
            }

            return sprites;
        }

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
                cacheToUse = levelCache;
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
