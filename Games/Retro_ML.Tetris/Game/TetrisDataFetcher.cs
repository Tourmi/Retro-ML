using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Tetris.Configuration;
using static Retro_ML.Tetris.Game.Addresses;

namespace Retro_ML.Tetris.Game
{
    /// <summary>
    /// Takes care of abstracting away the addresses when communicating with the emulator.
    /// </summary>
    internal class TetrisDataFetcher : IDataFetcher
    {

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;

        private const int PLAY_WIDTH = 10;
        private const int PLAY_HEIGHT = 17;
        private const int PLAY_OFFSET_X = 2;
        private const int PLAY_OFFSET_Y = 1;
       // private const int NUMBER_OF_ROWS_TO_SEE = 4;

        private const byte SOLID_TILES_START = 128;
        private const byte SOLID_TILES_END = 143;

        private TetrisPluginConfig pluginConfig;

        public TetrisDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, TetrisPluginConfig pluginConfig)
        {
            this.emulator = emulator;
            this.pluginConfig = pluginConfig;
            frameCache = new();
        }

        /// <summary>
        /// Needs to be called every frame to reset the memory cache
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();

            InitFrameCache();
        }

        /// <summary>
        /// Needs to be called every time a save state was loaded to reset the global cache.
        /// </summary>
        public void NextState()
        {
            frameCache.Clear();

        }

        public uint GetSingles() => ToUnsignedInteger(Read(Score.Single));
        public uint GetDoubles() => ToUnsignedInteger(Read(Score.Double));
        public uint GetTriples() => ToUnsignedInteger(Read(Score.Triple));
        public uint GetTetrises() => ToUnsignedInteger(Read(Score.Tetris));
        public bool IsGameOver() => ReadSingle(GameStatus) == 0x01;

        public double GetPositionX() => (ReadSingle(CurrentBlock.PosX) - 31) / 8 / 10.0;
        public double GetPositionY() => (ReadSingle(CurrentBlock.PosY) - 24) / 8 / 17.0;

        /// <summary>
        /// Returns the play area solid tiles 
        /// </summary>
        /// <returns></returns>
        public bool[,] GetSolidTiles()
        {
            var bytes = Read(Addresses.BackgroundTiles);
            bool[,] solidTiles = new bool[32, 32];
            for (int i = 0; i < bytes.Length; i++)
            {
                solidTiles[i / 32, i % 32] = bytes[i] >= SOLID_TILES_START && bytes[i] <= SOLID_TILES_END;
            }

            int firstLineWithTile = -1;
            bool[,] tiles = new bool[PLAY_HEIGHT, PLAY_WIDTH];
            for (int i = PLAY_OFFSET_Y; i < PLAY_HEIGHT + PLAY_OFFSET_Y; i++)
            {
                for (int j = PLAY_OFFSET_X; j < PLAY_WIDTH + PLAY_OFFSET_X; j++)
                {
                    if (firstLineWithTile == -1)
                        if (solidTiles[i, j])
                            firstLineWithTile = i - PLAY_OFFSET_Y;

                    tiles[i - PLAY_OFFSET_Y, j - PLAY_OFFSET_X] = solidTiles[i, j];
                }
            }

            bool[,] newTiles = new bool[pluginConfig.VisibleRows, PLAY_WIDTH];
            if (firstLineWithTile != -1)
            {
                firstLineWithTile = Math.Min(firstLineWithTile, PLAY_HEIGHT - pluginConfig.VisibleRows);
                for (int i = firstLineWithTile; i - firstLineWithTile < pluginConfig.VisibleRows; i++)
                {
                    for (int j = 0; j < PLAY_WIDTH; j++)
                    {
                        newTiles[i - firstLineWithTile, j] = tiles[i, j];
                    }
                }
            }

            return newTiles;
        }

        /// <summary>
        /// Returns the tiles being placed and their rotation
        /// </summary>
        /// <returns></returns>
        public bool[,] GetCurrentBlock() => GetBlockForType(ReadSingle(CurrentBlock.Type));

        public bool[,] GetNextBlock() => GetBlockForType(ReadSingle(NextBlock.Type));

        private bool[,] GetBlockForType(byte blockType)
        {
            bool[,] block = new bool[7, 4];

            if (blockType >= 7 * 4)
            {
                return block;
            }

            //If blockType is a square, set every rotation to true
            if (blockType / 4 == 0x3)
            {
                block[3, 0] = true;
                block[3, 1] = true;
                block[3, 2] = true;
                block[3, 3] = true;
            }
            else
            {
                block[blockType / 4, blockType % 4] = true;
            }

            return block;
        }

        public void GetCurrentBlockPosition()
        {
            
        }

        /// <summary>
        /// Reads the Low and High bytes at the addresses specified in AddressData, and puts them together into ushorts, assuming little Endian.
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>
        private ushort[] ReadLowHighBytes(AddressData addressData)
        {
            var cacheToUse = frameCache;
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
            var cacheToUse = frameCache;
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
                var cacheToUse = frameCache;
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

                var cacheToUse = frameCache;
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

        private void InitFrameCache()
        {
            (AddressData, bool)[] toRead = new (AddressData, bool)[]
            {
                new (BackgroundTiles, false), new (Score.Single, false), new (Score.Double, false), new (Score.Triple, false), new(Score.Tetris, false)
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
