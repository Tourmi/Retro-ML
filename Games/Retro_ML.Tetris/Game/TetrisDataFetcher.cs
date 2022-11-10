using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Tetris.Configuration;
using static Retro_ML.Tetris.Game.Addresses;

namespace Retro_ML.Tetris.Game;

/// <summary>
/// Takes care of abstracting away the addresses when communicating with the emulator.
/// </summary>
internal class TetrisDataFetcher : IDataFetcher
{

    private readonly IEmulatorAdapter emulator;
    private readonly Dictionary<uint, byte[]> frameCache;
    private readonly Dictionary<uint, byte[]> tilesCache;

    private double oldY;

    public const int PLAY_WIDTH = 10;
    private const int PLAY_HEIGHT = 17;
    public const int PLAY_OFFSET_X = 2;
    private const int PLAY_OFFSET_Y = 1;

    public const int INITIAL_X_INDEX = 4;

    private const byte SOLID_TILES_START = 128;
    private const byte SOLID_TILES_END = 143;

    private TetrisPluginConfig pluginConfig;

    public TetrisDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, TetrisPluginConfig pluginConfig)
    {
        this.emulator = emulator;
        this.pluginConfig = pluginConfig;
        frameCache = new();
        tilesCache = new();
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
        tilesCache.Clear();

        oldY = 0.0;
    }

    public uint GetSingles() => ToUnsignedInteger(Read(Score.Single));
    public uint GetDoubles() => ToUnsignedInteger(Read(Score.Double));
    public uint GetTriples() => ToUnsignedInteger(Read(Score.Triple));
    public uint GetTetrises() => ToUnsignedInteger(Read(Score.Tetris));
    public bool IsGameOver() => ReadSingle(GameStatus) is 0x01 or 0x04 or 0x0D;

    public int GetPositionXIndex() => (ReadSingle(CurrentBlock.PosX) - 31) / 8;
    public double GetPositionX() => GetPositionXIndex() / (double)PLAY_WIDTH;
    public int GetPositionYIndex() => (ReadSingle(CurrentBlock.PosY) - 24) / 8;
    public double GetPositionY() => GetPositionYIndex() / (double)PLAY_HEIGHT;

    /// <summary>
    /// Returns the play area solid tiles 
    /// </summary>
    /// <returns></returns>
    public bool[,] GetSolidTiles()
    {
        var solidTiles = GetAllTiles();

        var playingTiles = GetPlayingTiles(solidTiles, out int firstLineWithTile);

        bool[,] tilesToGiveAI = new bool[pluginConfig.VisibleRows, PLAY_WIDTH];
        if (firstLineWithTile != -1)
        {
            firstLineWithTile = Math.Min(firstLineWithTile, PLAY_HEIGHT - pluginConfig.VisibleRows);
            for (int i = firstLineWithTile; i - firstLineWithTile < pluginConfig.VisibleRows; i++)
            {
                for (int j = 0; j < PLAY_WIDTH; j++)
                {
                    tilesToGiveAI[i - firstLineWithTile, j] = playingTiles[i, j];
                }
            }
        }

        return tilesToGiveAI;
    }

    public double[,] GetNormalizedHeight()
    {
        double[,] normalizedHeights = new double[1, PLAY_WIDTH];
        var heights = GetColumnHeights();

        for (int i = 0; i < heights.Length; i++)
        {
            normalizedHeights[0, i] = heights[i] / (double)PLAY_HEIGHT;
        }

        return normalizedHeights;
    }

    public int[] GetColumnHeights()
    {
        var solidTiles = GetAllTiles();
        var playingTiles = GetPlayingTiles(solidTiles, out _);

        int[] heights = new int[PLAY_WIDTH];
        for (int i = PLAY_HEIGHT - 1; i >= 0; i--)
        {
            for (int j = 0; j < PLAY_WIDTH; j++)
            {
                if (playingTiles[i, j])
                {
                    heights[j] = PLAY_HEIGHT - i;
                }
            }
        }
        return heights;
    }

    private bool[,] GetAllTiles()
    {
        var bytes = Read(Addresses.BackgroundTiles);
        bool[,] solidTiles = new bool[32, 32];
        for (int i = 0; i < bytes.Length; i++)
        {
            solidTiles[i / 32, i % 32] = bytes[i] >= SOLID_TILES_START && bytes[i] <= SOLID_TILES_END;
        }

        return solidTiles;
    }

    private static bool[,] GetPlayingTiles(bool[,] solidTiles, out int firstLineWithTile)
    {
        firstLineWithTile = -1;
        bool[,] playingTiles = new bool[PLAY_HEIGHT, PLAY_WIDTH];
        for (int i = PLAY_OFFSET_Y; i < PLAY_HEIGHT + PLAY_OFFSET_Y; i++)
        {
            for (int j = PLAY_OFFSET_X; j < PLAY_WIDTH + PLAY_OFFSET_X; j++)
            {
                if (firstLineWithTile == -1)
                    if (solidTiles[i, j])
                        firstLineWithTile = i - PLAY_OFFSET_Y;

                playingTiles[i - PLAY_OFFSET_Y, j - PLAY_OFFSET_X] = solidTiles[i, j];
            }
        }

        return playingTiles;
    }

    /// <summary>
    /// Returns the tiles being placed and their rotation
    /// </summary>
    /// <returns></returns>
    public bool[,] GetCurrentBlockType() => GetBlockForType(ReadSingle(CurrentBlock.Type));

    public bool[,] GetNextBlockType() => GetBlockForType(ReadSingle(NextBlock.Type));

    public bool[,] GetCurrentBlockRotation() => GetRotation(ReadSingle(CurrentBlock.Type));

    public bool[,] GetNextBlockRotation() => GetRotation(ReadSingle(NextBlock.Type));

    public static bool[,] GetRotation(byte blockType)
    {
        bool[,] rotation = new bool[1, 4];

        if (blockType / 4 == 0x3)
        {
            rotation[0, 0] = true;
            rotation[0, 1] = true;
            rotation[0, 2] = true;
            rotation[0, 3] = true;
        }
        else
        {
            rotation[0, blockType % 4] = true;
        }

        return rotation;
    }

    private static bool[,] GetBlockForType(byte blockType)
    {
        bool[,] type = new bool[1, 7];

        type[0, blockType / 4] = true;

        return type;
    }

    public int GetNumberOfHoles()
    {
        return CountHoles(GetPlayingTiles(GetAllTiles(), out _));
    }

    public int CountHoles(bool[,] tiles)
    {
        bool[,] visited = new bool[PLAY_HEIGHT, PLAY_WIDTH];

        int count = 0;
        for (int i = 0; i < PLAY_HEIGHT; i++)
        {
            for (int j = 0; j < PLAY_WIDTH; j++)
            {
                if (!tiles[i, j] && !visited[i, j])
                {
                    DFS(tiles, i, j, visited);
                    count++;
                }
            }
        }
        return count - 1;
    }

    /// <summary>
    /// Try to visit all empty tiles in current section
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="visited"></param>
    private void DFS(bool[,] tiles, int row, int col, bool[,] visited)
    {
        visited[row, col] = true;

        int[] rowNbr = new int[] { 0, -1, 0, 1 };
        int[] colNbr = new int[] { -1, 0, 1, 0 };

        for (int k = 0; k < 4; k++)
        {
            if (IsSafe(tiles, row + rowNbr[k], col + colNbr[k], visited))
            {
                DFS(tiles, row + rowNbr[k], col + colNbr[k], visited);
            }
        }
    }

    /// <summary>
    /// Check if the index are within the array bounds and if it was already visited
    /// </summary>
    /// <param name="M"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="visited"></param>
    /// <returns></returns>
    private static bool IsSafe(bool[,] M, int row,
                       int col, bool[,] visited)
    {
        return (row >= 0) && (row < PLAY_HEIGHT) && (col >= 0) && (col < PLAY_WIDTH) && (!M[row, col] && !visited[row, col]);
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

    /// <summary>
    /// Which cache to use depending on the AddressData
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData)
    {
        var cacheToUse = frameCache;
        if (addressData.CacheDuration == AddressData.CacheDurations.Tiles)
        {
            cacheToUse = tilesCache;
            if (GetPositionY() < oldY)
            {
                cacheToUse.Clear();
            }
            oldY = GetPositionY();
        }

        return cacheToUse;
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
