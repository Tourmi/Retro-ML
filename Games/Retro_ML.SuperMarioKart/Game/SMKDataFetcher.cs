using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperMarioKart.Configuration;
using Retro_ML.SuperMarioKart.Game.Data;
using Retro_ML.Utils;
using static Retro_ML.SuperMarioKart.Game.Addresses;

namespace Retro_ML.SuperMarioKart.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class SMKDataFetcher : IDataFetcher
    {
        private const int TILES_ROW_COUNT = 128;
        private const int TILES_COLUMN_COUNT = 128;
        private const int SURROUNDING_TILES_RANGE = 32;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> raceCache;
        private readonly Dictionary<ushort, Dictionary<uint, byte[]>> perTrackCache;
        private readonly SMKPluginConfig pluginConfig;

        private InternalClock internalClock;

        public SMKDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SMKPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            raceCache = new();
            perTrackCache = new();
            this.pluginConfig = pluginConfig;
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
        public ushort GetPositionX() => (ushort)ToUnsignedInteger(Read(Racers.XPosition));
        public ushort GetPositionY() => (ushort)ToUnsignedInteger(Read(Racers.YPosition));
        public ushort GetHeadingAngle() => (ushort)ToUnsignedInteger(Read(Racers.HeadingAngle));
        public double GetHeadingAngleRadians() => GetHeadingAngle() / (double)ushort.MaxValue * Math.Tau;
        public byte GetKartStatus() => ReadSingle(Racers.KartStatus);
        public byte GetMaxCheckpoint() => ReadSingle(Race.CheckpointCount);
        public byte GetCurrentCheckpoint() => ReadSingle(Racers.CurrentCheckpointNumber);
        public sbyte GetCurrentLap() => (sbyte)(ReadSingle(Racers.CurrentLap) - 128);
        public bool IsOffroad() => ReadSingle(Racers.OffRoad) == 0x10;
        public ushort GetTrackNumber() => (ushort)ToUnsignedInteger(Read(Racetrack.Number));
        public ushort GetCollisionTimer() => (ushort)ToUnsignedInteger(Read(Racers.CollisionTimer));
        public byte GetRaceStatus() => ReadSingle(Race.RaceStatus);
        public byte GetCoins() => ReadSingle(Race.Coins);
        public bool IsItemReady() => ReadSingle(Race.ItemState) == 0xC0;
        public byte GetRanking() => (byte)(ReadSingle(Racers.CurrentRank) / 2);
        public bool IsRace() => ReadSingle(Race.Type) <= 2;
        public double GetSpeedOutOfMaxSpeed()
        {
            uint maxSpeed = ToUnsignedInteger(Read(Racers.MaximumSpeed));
            if (maxSpeed == 0) return 0;
            return ToUnsignedInteger(Read(Racers.AbsoluteSpeed)) / (double)maxSpeed;
        }

        public bool[,] GetCurrentItem()
        {
            bool[,] currentItem = new bool[3, 3];

            if (IsItemReady())
            {
                byte itemId = ReadSingle(Race.ItemId);
                currentItem[itemId / 3, itemId % 3] = true;
            }

            return currentItem;
        }

        public double GetHeadingDifference()
        {
            var flowmap = Read(Racetrack.FlowMap);
            var racerX = (ushort)ToUnsignedInteger(Read(Racers.XPosition));
            var racerY = (ushort)ToUnsignedInteger(Read(Racers.YPosition));
            var flowX = Math.Clamp(racerX / 16, 0, 63);
            var flowY = Math.Clamp(racerY / 16, 0, 63);

            byte currAngle = (byte)(GetHeadingAngle() / 256);
            byte currFlowAngle = flowmap[flowY * 64 + flowX];

            return ((sbyte)(currFlowAngle - currAngle)) / (double)sbyte.MaxValue;
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();

        /// <summary>
        /// Returns the distance to specific tiles that satisfy the <paramref name="isSurfaceFunc"/> condition.
        /// </summary>
        public double[,] GetRays(int distance, int rayCount, Func<byte, bool> isSurfaceFunc)
        {
            return Raycast.GetRayDistances(GetSurroundingTilesOfType(distance, distance, isSurfaceFunc), distance, rayCount, GetHeadingAngleRadians(), Math.Tau * pluginConfig.ViewAngle / 360.0);
        }

        public double[,] GetObstacleRays(int distance, int rayCount)
        {
            var objectRays = Raycast.GetRayDistances(GetSurroundingObjects(distance, distance), distance, rayCount, GetHeadingAngleRadians(), Math.Tau * pluginConfig.ViewAngle / 360.0);
            var itemRays = Raycast.GetRayDistances(GetSurroundingItems(distance, distance), distance, rayCount, GetHeadingAngleRadians(), Math.Tau * pluginConfig.ViewAngle / 360.0);
            return Raycast.CombineRays(objectRays, itemRays);
        }

        public double[,] GetRacersRays(int distance, int rayCount)
        {
            var racerRays = Raycast.GetRayDistances(GetSurroundingRacers(distance, distance), distance, rayCount, GetHeadingAngleRadians(), Math.Tau * pluginConfig.ViewAngle / 360.0);

            return racerRays;
        }

        /// <summary>
        /// Returns the tiles surrounding the player in the <paramref name="xDist"/> and <paramref name="yDist"/> directions, which satisfy the <paramref name="isSurfaceFunc"/> function.
        /// </summary>
        private bool[,] GetSurroundingTilesOfType(int xDist, int yDist, Func<byte, bool> isSurfaceFunc)
        {
            var tileTypes = Read(Racetrack.TileSurfaceTypes);
            var surroundingTiles = GetSurroundingTiles(xDist, yDist);
            var tiles = new bool[surroundingTiles.GetLength(0), surroundingTiles.GetLength(1)];
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    byte tile = surroundingTiles[i, j];
                    tiles[i, j] = isSurfaceFunc(tileTypes[tile]);
                }
            }

            return tiles;
        }

        private byte[,] GetSurroundingTiles(int xDist, int yDist)
        {
            int xPos = GetPositionX() / 8;
            int yPos = GetPositionY() / 8;

            byte[] tiles = Read(Racetrack.TileMap);
            byte[,] nearbyTiles = new byte[yDist * 2 + 1, xDist * 2 + 1];
            for (int y = -yDist; y <= yDist; y++)
            {
                for (int x = -xDist; x <= xDist; x++)
                {
                    nearbyTiles[y + yDist, x + xDist] = tiles[
                                Math.Clamp(y + yPos, 0, TILES_ROW_COUNT - 1) * TILES_COLUMN_COUNT +
                                Math.Clamp(x + xPos, 0, TILES_COLUMN_COUNT - 1)
                            ];
                }
            }

            return nearbyTiles;
        }

        private bool[,] GetSurroundingObjects(int xDist, int yDist)
        {
            int xPos = GetPositionX() / 8;
            int yPos = GetPositionY() / 8;

            var objects = GetTrackObjects();
            bool[,] nearbyTiles = new bool[yDist * 2 + 1, xDist * 2 + 1];

            for (int y = -yDist; y <= yDist; y++)
            {
                for (int x = -xDist; x <= xDist; x++)
                {
                    nearbyTiles[y + yDist, x + xDist] = objects.Any(o => o.IsThreatTo(xPos + x, yPos + y));
                }
            }

            return nearbyTiles;
        }

        private bool[,] GetSurroundingItems(int xDist, int yDist)
        {
            int xPos = GetPositionX() / 8;
            int yPos = GetPositionY() / 8;
            var items = GetItems();

            bool[,] nearbyTiles = new bool[yDist * 2 + 1, xDist * 2 + 1];

            for (int y = -yDist; y <= yDist; y++)
            {
                for (int x = -xDist; x <= xDist; x++)
                {
                    nearbyTiles[y + yDist, x + xDist] = items.Any(o => o.IsThreatTo(xPos + x, yPos + y));
                }
            }

            return nearbyTiles;
        }

        private bool[,] GetSurroundingRacers(int xDist, int yDist)
        {
            int xPos = GetPositionX() / 8;
            int yPos = GetPositionY() / 8;
            var racers = GetRacers();

            bool[,] nearbyTiles = new bool[yDist * 2 + 1, xDist * 2 + 1];

            for (int y = -yDist; y <= yDist; y++)
            {
                for (int x = -xDist; x <= xDist; x++)
                {
                    nearbyTiles[y + yDist, x + xDist] = racers.Any(o => o.IsThreatTo(xPos + x, yPos + y));
                }
            }

            return nearbyTiles;
        }

        private Obstacle[] GetTrackObjects()
        {
            var xPositions = ReadMultiple(TrackObjects.ObjectXPos, TrackObjects.SingleObject, TrackObjects.AllObjects).ToArray();
            var yPositions = ReadMultiple(TrackObjects.ObjectYPos, TrackObjects.SingleObject, TrackObjects.AllObjects).ToArray();
            var zPositions = ReadMultiple(TrackObjects.ObjectZPos, TrackObjects.SingleObject, TrackObjects.AllObjects).ToArray();
            var zVelocities = ReadMultiple(TrackObjects.ObjectZVelocity, TrackObjects.SingleObject, TrackObjects.AllObjects).ToArray();

            Obstacle[] objects = new Obstacle[xPositions.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i] = new Obstacle()
                {
                    XPos = (short)ToUnsignedInteger(xPositions[i]),
                    YPos = (short)ToUnsignedInteger(yPositions[i]),
                    ZPos = (short)ToUnsignedInteger(zPositions[i]),
                    ZVelocity = (short)ToUnsignedInteger(zVelocities[i])
                };
            }

            return objects;
        }

        private Obstacle[] GetItems()
        {
            var xPositions = ReadMultiple(Items.ItemXPos, Items.SingleItem, Items.AllItems).ToArray();
            var yPositions = ReadMultiple(Items.ItemYPos, Items.SingleItem, Items.AllItems).ToArray();
            var zPositions = ReadMultiple(Items.ItemZPos, Items.SingleItem, Items.AllItems).ToArray();
            var xVelocities = ReadMultiple(Items.ItemXVelocity, Items.SingleItem, Items.AllItems).ToArray();
            var yVelocities = ReadMultiple(Items.ItemYVelocity, Items.SingleItem, Items.AllItems).ToArray();
            var zVelocities = ReadMultiple(Items.ItemZVelocity, Items.SingleItem, Items.AllItems).ToArray();

            Obstacle[] items = new Obstacle[xPositions.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new Obstacle()
                {
                    XPos = (short)ToUnsignedInteger(xPositions[i]),
                    YPos = (short)ToUnsignedInteger(yPositions[i]),
                    ZPos = (short)ToUnsignedInteger(zPositions[i]),
                    XVelocity = (short)ToUnsignedInteger(xVelocities[i]),
                    YVelocity = (short)ToUnsignedInteger(yVelocities[i]),
                    ZVelocity = (short)ToUnsignedInteger(zVelocities[i])
                };
            }

            return items;
        }

        private Obstacle[] GetRacers()
        {
            var xPositions = ReadMultiple(Racers.XPosition, Racers.SingleRacer, Racers.AllRacers).ToArray();
            var yPositions = ReadMultiple(Racers.YPosition, Racers.SingleRacer, Racers.AllRacers).ToArray();
            var xVelocities = ReadMultiple(Racers.XVelocity, Racers.SingleRacer, Racers.AllRacers).ToArray();
            var yVelocities = ReadMultiple(Racers.YVelocity, Racers.SingleRacer, Racers.AllRacers).ToArray();

            Obstacle[] racers = new Obstacle[xPositions.Length];
            for (int i = 1; i < racers.Length; i++)
            {
                racers[i] = new Obstacle()
                {
                    XPos = (short)ToUnsignedInteger(xPositions[i]),
                    YPos = (short)ToUnsignedInteger(yPositions[i]),
                    XVelocity = (short)ToUnsignedInteger(xVelocities[i]),
                    YVelocity = (short)ToUnsignedInteger(yVelocities[i])
                };
            }

            return racers;
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
        /// Reads into multiple groups of bytes according to the given offset and total byte sizes.
        /// </summary>
        /// <param name="addressData"></param>
        /// <param name="offset"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private IEnumerable<byte[]> ReadMultiple(AddressData addressData, AddressData offset, AddressData total)
        {
            uint count = total.Length / offset.Length;
            var results = new byte[count][];
            var toRead = new List<(AddressData, bool)>();
            for (int i = 0; i < results.Length; i++)
            {
                toRead.Add((new AddressData((uint)(addressData.Address + i * offset.Length), addressData.Length, addressData.CacheDuration, (uint)(addressData.HighByteAddress + i * offset.Length)), addressData.HighByteAddress != 0));
            }
            var result = Read(toRead.ToArray());
            var bytesPerItem = (addressData.HighByteAddress == 0 ? 1 : 2) * addressData.Length;
            for (long i = 0; i < result.Length; i += bytesPerItem)
            {
                var bytes = new byte[bytesPerItem];
                Array.Copy(result, i, bytes, 0, bytesPerItem);
                yield return bytes;
            }
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
            switch (addressData.CacheDuration)
            {
                case AddressData.CacheDurations.Frame:
                    return frameCache;
                case AddressData.CacheDurations.Race:
                    return raceCache;
                case AddressData.CacheDurations.PerTrack:
                    ushort trackNumber = GetTrackNumber();
                    if (!perTrackCache.ContainsKey(trackNumber))
                    {
                        perTrackCache[trackNumber] = new Dictionary<uint, byte[]>();
                    }
                    return perTrackCache[trackNumber];
                default:
                    return frameCache;
            }
        }

        private void InitFrameCache()
        {
            List<(AddressData, bool)> toRead = new()
            {
            };

            toRead.AddRange(GetCalculatedAddresses(TrackObjects.AllObjects.Length, TrackObjects.SingleObject.Length,
                TrackObjects.ObjectXPos,
                TrackObjects.ObjectYPos,
                TrackObjects.ObjectZPos,
                TrackObjects.ObjectZVelocity
                ));

            toRead.AddRange(GetCalculatedAddresses(Items.AllItems.Length, Items.SingleItem.Length,
                Items.ItemXPos,
                Items.ItemYPos,
                Items.ItemZPos,
                Items.ItemXVelocity,
                Items.ItemYVelocity,
                Items.ItemZVelocity
                ));

            toRead.AddRange(GetCalculatedAddresses(Racers.AllRacers.Length, Racers.SingleRacer.Length,
                Racers.XPosition,
                Racers.YPosition,
                Racers.XVelocity,
                Racers.YVelocity
                ));

            _ = Read(toRead.ToArray());
        }

        private static IEnumerable<(AddressData, bool hasHighByte)> GetCalculatedAddresses(uint totalLength, uint offset, params AddressData[] baseAddresses)
        {
            for (uint i = 0; i < totalLength; i += offset)
            {
                for (int j = 0; j < baseAddresses.Length; j++)
                {
                    yield return (new AddressData()
                    {
                        Address = baseAddresses[j].Address + i * offset,
                        CacheDuration = baseAddresses[j].CacheDuration,
                        Length = baseAddresses[j].Length
                    }, baseAddresses[j].HighByteAddress != 0);
                }
            }
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
