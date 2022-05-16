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
        public ushort GetPositionX() => (ushort)ToUnsignedInteger(Read(Racer.XPosition));
        public ushort GetPositionY() => (ushort)ToUnsignedInteger(Read(Racer.YPosition));
        public ushort GetHeadingAngle() => (ushort)ToUnsignedInteger(Read(Racer.HeadingAngle));
        public double GetHeadingAngleRadians() => GetHeadingAngle() / (double)ushort.MaxValue * Math.Tau;
        public byte GetKartStatus() => ReadSingle(Racer.KartStatus);
        public byte GetMaxCheckpoint() => ReadSingle(Race.CheckpointCount);
        public byte GetCurrentCheckpoint() => ReadSingle(Racer.CurrentCheckpointNumber);
        public sbyte GetCurrentLap() => (sbyte)(ReadSingle(Racer.CurrentLap) - 128);
        public bool IsOffroad() => ReadSingle(Racer.KartStatus) == 0x10;
        public ushort GetTrackNumber() => (ushort)ToUnsignedInteger(Read(Racetrack.Number));
        public ushort GetCollisionTimer() => (ushort)ToUnsignedInteger(Read(Racer.CollisionTimer));

        public double GetHeadingDifference()
        {
            var flowmap = Read(Racetrack.FlowMap);
            var racerX = (ushort)ToUnsignedInteger(Read(Racer.XPosition));
            var racerY = (ushort)ToUnsignedInteger(Read(Racer.YPosition));
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

        /// <summary>
        /// Returns the tiles surrounding the player in the <paramref name="xDist"/> and <paramref name="yDist"/> directions, which satisfy the <paramref name="isSurfaceFunc"/> function.
        /// </summary>
        public bool[,] GetSurroundingTilesOfType(int xDist, int yDist, Func<byte, bool> isSurfaceFunc)
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

        public double[,] GetObstacleRays(int distance, int rayCount)
        {
            return Raycast.GetRayDistances(GetSurroundingObjects(distance, distance), distance, rayCount, GetHeadingAngleRadians(), Math.Tau * pluginConfig.ViewAngle / 360.0);
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

        private TrackObject[] GetTrackObjects()
        {
            var xPositions = ReadMultiple(TrackObjects.ObjectXPos, TrackObjects.Object, TrackObjects.Objects).ToArray();
            var yPositions = ReadMultiple(TrackObjects.ObjectYPos, TrackObjects.Object, TrackObjects.Objects).ToArray();
            var zPositions = ReadMultiple(TrackObjects.ObjectZPos, TrackObjects.Object, TrackObjects.Objects).ToArray();
            var zVelocities = ReadMultiple(TrackObjects.ObjectZVelocity, TrackObjects.Object, TrackObjects.Objects).ToArray();

            TrackObject[] objects = new TrackObject[xPositions.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i] = new TrackObject()
                {
                    XPos = (short)ToUnsignedInteger(xPositions[i]),
                    YPos = (short)ToUnsignedInteger(yPositions[i]),
                    ZPos = (short)ToUnsignedInteger(zPositions[i]),
                    ZVelocity = (short)ToUnsignedInteger(zVelocities[i])
                };
            }

            return objects;
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
