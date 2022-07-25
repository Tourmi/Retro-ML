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
        public const uint DESIRED_LEVEL_WIDTH = 0xD;
        public const uint DESIRED_LEVEL_HEIGHT = 0xB;
        public const uint TILES_WIDTH = 0x10;
        public const uint TILES_HEIGHT = 0x10;
        private const uint MIN_X_POS = 0x10;
        private const uint MIN_Y_POS = 0x10;
        private const uint MAX_X_POS = 0xD0;
        private const uint MAX_Y_POS = 0xB0;
        private const uint MAX_ACCELERATOR = 0x100;
        private const uint MAX_BOMB = 0x9;
        private const uint MIN_EXPLOSION_EXPANDER = 0x02;
        private const uint MAX_EXPLOSION_EXPANDER = 0x09;
        private const uint MAX_BOMB_TIMER = 0x95;
        private const uint MIN_BOMB_TIMER = 0x01;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly InternalClock internalClock;

        private byte[,] playableTilesCache;
        private bool[] playersDead;

        public SB3DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SB3PluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            playersDead = new bool[4];
            Array.Fill(playersDead, false);
            playableTilesCache = new byte[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];
            internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public bool IsSuddenDeathActivated() => ReadSingle(GameAddresses.GameMinutesTimer) == 0x00 ?  true : false;
        public byte[] GetPlayerXPos() => Read(PlayersAddresses.XPos);
        public byte[] GetPlayerYPos() => Read(PlayersAddresses.YPos);
        public byte[] GetTiles() => Read(GameAddresses.DynamicTiles);
        public byte[] GetBombsPos() => Read(GameAddresses.BombsPosition);
        public byte[] GetBombsTimer() => Read(GameAddresses.BombsTimer);
        public double GetPlayerXPositionNormalized() => GetXPositionNormalized(GetPlayerXPos()[0]);
        public double GetPlayerYPositionNormalized() => GetYPositionNormalized(GetPlayerYPos()[0]);
        public double GetClosestPowerupXPosNormalized() => GetClosestPowerUp().Item1;
        public double GetClosestPowerupYPosNormalized() => GetClosestPowerUp().Item2;
        public byte GetBombsPlanted() => ReadSingle(PlayersAddresses.BombsPlanted);
        public double GetBombsPlantedNormalized() => GetBombsPlanted() / (double)MAX_BOMB;
        public bool IsPlayerIdle() => ReadSingle(PlayersAddresses.IdleTimer) == 0xFF;
        public byte[] GetPlayerDeathTimer() => Read(PlayersAddresses.DeathTimer);
        public byte GetPlayerExtraBombPowerUpLevel() => ReadSingle(PowerupsAddresses.ExtraBomb);
        public byte GetPlayerExplosionExpanderPowerUpLevel() => ReadSingle(PowerupsAddresses.ExplosionExpander);
        public byte GetPlayerAcceleratorPowerUpLevel() => ReadSingle(PowerupsAddresses.Accelerator);
        public double GetPlayerExtraBombPowerUpLevelNormalized() => GetPlayerExtraBombPowerUpLevel() / (double)MAX_BOMB;
        public double GetPlayerExplosionExpanderPowerUpLevelNormalized() => GetPlayerExplosionExpanderPowerUpLevel() / (double)(MAX_EXPLOSION_EXPANDER - MIN_EXPLOSION_EXPANDER);
        public double GetPlayerAcceleratorPowerUpLevelNormalized() => GetPlayerAcceleratorPowerUpLevel() / (double)MAX_ACCELERATOR;
        public bool GetPlayerLouiePowerUpState() => ReadSingle(PowerupsAddresses.Louie) == 0;
        public bool IsLouieColourYellow() => GetPlayerLouiePowerUpState() ? ToUnsignedInteger(Read(PowerupsAddresses.LouieColours)) == 0x35F02DD : false;
        public bool IsLouieColourBrown() => GetPlayerLouiePowerUpState() ? ToUnsignedInteger(Read(PowerupsAddresses.LouieColours)) == 0x35B2210D : false;
        public bool IsLouieColourPink() => GetPlayerLouiePowerUpState() ? ToUnsignedInteger(Read(PowerupsAddresses.LouieColours)) == 0x7DFF695A : false;
        public bool IsLouieColourGreen() => GetPlayerLouiePowerUpState() ? ToUnsignedInteger(Read(PowerupsAddresses.LouieColours)) == 0x1BE00AC : false;
        public bool IsLouieColourBlue() => GetPlayerLouiePowerUpState() ? ToUnsignedInteger(Read(PowerupsAddresses.LouieColours)) == 0x7E805940 : false;
        public bool GetPlayerKickPowerUpState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x02) != 0;
        public bool GetPlayerGlovePowerUpState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x04) != 0;
        public bool GetPlayerSlimeBombPowerUpState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x20) != 0;
        public bool GetPlayerPowerBombPowerUpState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x40) != 0;
        public int GetNumbersOfPlayersAlive() => playersDead.Where(c => !c).Count();
        public bool IsPlayerDead() => playersDead[0] == true;

        /// <summary>
        /// Needs to be called every frame to reset the memory cache
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();
            MapPlayableTiles();
            CheckPlayerDeathStatus();
            internalClock.NextFrame();
            InitFrameCache();
        }

        /// <summary>
        /// Needs to be called every time a save state was loaded to reset the global cache.
        /// </summary>
        public void NextState()
        {
            frameCache.Clear();
            Array.Fill(playersDead, false);
            MapPlayableTiles();
            internalClock.Reset();
        }

        /// <summary>
        /// The tile map that is read in memory is of size 176 (11 x 16 tiles). However, the playable area in the game is only 11 x 13.
        /// This function convert the 1d tileMap into the 2d playableTileMap so it is used as a class variable for other functions.
        /// </summary>
        public void MapPlayableTiles()
        {
            byte[] tiles = GetTiles();
            byte[,] playableTiles = new byte[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    playableTiles[i, j] = tiles[(i * TILES_WIDTH) + (j + 1)];
                }
            }

            playableTilesCache = playableTiles;
        }

        /// <summary>
        /// Check if a player is dead.
        /// </summary>
        public void CheckPlayerDeathStatus()
        {
            byte[] timers = GetPlayerDeathTimer();
            
            for (int playerIndex = 0; playerIndex < 4; playerIndex++)
            {
                //If player is not already flagged as dead
                if (playersDead[playerIndex] != true)
                {
                    //If the timer associated with the player == 60 (0x3c), it means the player just died
                    if (timers[playerIndex] == 0x3C)
                    {
                        playersDead[playerIndex] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Get X position normalized on the grid 
        /// </summary>
        public double GetXPositionNormalized(int pos) => (pos - MIN_X_POS) / (double)(MAX_X_POS - MIN_X_POS);

        /// <summary>
        /// Get Y position normalized on the grid
        /// </summary>
        public double GetYPositionNormalized(int pos) => (pos - MIN_Y_POS) / (double)(MAX_Y_POS - MIN_Y_POS);

        /// <summary>
        /// Get bomb timer normalized
        /// </summary>
        public double GetBombTimerNormalized(byte timer) => 1.0 - (timer / (double)(MAX_BOMB_TIMER - MIN_BOMB_TIMER));

        /// <summary>
        /// Convert Bomb coordinate varying from 17 to 189 to 2d grid coordinate varying from 0 to 13 (horizontally) and from 0 to 11 (vertically)
        /// </summary>
        public Tuple<uint, uint> ToGridPos(uint coord) => new Tuple<uint, uint>(((coord - 1) / (DESIRED_LEVEL_WIDTH + 3)) - 1, (coord % (DESIRED_LEVEL_WIDTH + 3)) - 1);

        /// <summary>
        /// Return an array of double representing normalized horizontal distance between the player and every enemies 
        /// Our savestates contains 3 enemies. But the game lets you play vs 1 to 4 enemies. 
        /// </summary>
        public double[,] GetEnemiesXDistanceToThePlayer(uint enemyCount)
        {
            double[,] result = new double[enemyCount, 1];

            double playerXPosInLevel = GetPlayerXPositionNormalized();
            byte[] enemyXPos = GetPlayerXPos();

            for (int enemy = 0; enemy < enemyCount; enemy++)
            {
                double enemyXPosInLevel = GetXPositionNormalized(enemyXPos[enemy + 1]);
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
            byte[] enemyYPos = GetPlayerYPos();

            for (int enemy = 0; enemy < enemyCount; enemy++)
            {
                double enemyYPosInLevel = GetYPositionNormalized(enemyYPos[enemy + 1]);
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

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //Tile is undestructible
                    if (playableTilesCache[i, j] == 0x80 || playableTilesCache[i, j] == 0x84) result[i, j] = 1.0;
                    //Tile is destructible
                    else if (playableTilesCache[i, j] == 0x30) result[i, j] = 0.5;
                    //Tile is free
                    else if (playableTilesCache[i, j] == 0x00) result[i, j] = 0.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Draw dangers ; bombs, explosions, etc. Anything that can damage players.
        /// </summary>
        public double[,] DrawDangers()
        {
            double[,] result = new double[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];
            //byte[] tiles = GetTiles();
            byte[] bombsPos = GetBombsPos();
            byte[] bombsTimers = GetBombsTimer();

            //Draw Bombs
            for (int bombsIndex = 0; bombsIndex < bombsPos.Length; bombsIndex++)
            {
                if (bombsTimers[bombsIndex] != 0)
                {
                    var bombPos = ToGridPos(bombsPos[bombsIndex]);
                    var bombTimer = GetBombTimerNormalized(bombsTimers[bombsIndex]);
                    result[bombPos.Item1, bombPos.Item2] = bombTimer;
                }
            }

            //Draw Explosions and other dangers
            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    if (playableTilesCache[i, j] == 0x24 || playableTilesCache[i, j] == 0x05 || playableTilesCache[i, j] == 0x07 || playableTilesCache[i, j] == 0x04) result[i, j] = 1.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the powerup that is the closest to the player on the grid using manhattan distance.
        /// Values are normalized. If there is no powerup present, the function will return a pair of (1.0, 1.0)
        /// </summary>
        public Tuple<double, double> GetClosestPowerUp()
        {
            var playerXPos = GetPlayerXPos()[0];
            var playerYPos = GetPlayerYPos()[0];
            double closestX = 1.0;
            double closestY = 1.0;
            int closestDist = int.MaxValue;

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //If tile contains a powerup
                    if (playableTilesCache[i, j] == 0x10)
                    {
                        int powerupXPos = (int)(j * TILES_WIDTH) + (int)MIN_X_POS;
                        int powerupYPos = (int)(i * TILES_HEIGHT) + (int)MIN_Y_POS;

                        //If the powerup is the closest found to date, we want to tag it.
                        var dist = Utils.MathUtils.ManhattanDistance(playerXPos, playerYPos, powerupXPos, powerupYPos);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestX = (powerupXPos - playerXPos) / (double)(MAX_X_POS - MIN_X_POS);
                            closestY = (powerupYPos - playerYPos) / (double)(MAX_Y_POS - MIN_Y_POS);
                        }
                    }
                }
            }

            return new Tuple<double, double>(closestX, closestY);
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
        private byte[] Read(params AddressData[] addresses)
        {
            List<(uint addr, uint length)> toFetch = new();

            uint totalBytes = 0;

            foreach (var address in addresses)
            {
                var cacheToUse = frameCache;
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

                var cacheToUse = frameCache;
                if (!cacheToUse.ContainsKey(address.Address))
                {
                    cacheToUse[address.Address] = data[dataIndex..(dataIndex + count)];
                    dataIndex += count;
                }

                bytes.AddRange(cacheToUse[address.Address]);
            }

            return bytes.ToArray();
        }

        private void InitFrameCache()
        {
            List<AddressData> toRead = new()
        {
            GameAddresses.DynamicTiles,
            GameAddresses.StaticTiles,
            GameAddresses.BombsPosition,
            GameAddresses.BombsTimer,
            PlayersAddresses.XPos,
            PlayersAddresses.YPos,
            PlayersAddresses.BombsPlanted,
            PlayersAddresses.DeathTimer,
            PowerupsAddresses.ExplosionExpander,
            PowerupsAddresses.Accelerator,
            PowerupsAddresses.ExtraBomb,
            PowerupsAddresses.Louie,
            PowerupsAddresses.LouieColours,
            PowerupsAddresses.BombermanUpgrade,
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
