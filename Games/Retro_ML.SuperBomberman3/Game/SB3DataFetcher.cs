using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.SuperBomberman3.Configuration;
using static Retro_ML.SuperBomberman3.Game.Addresses;
using Retro_ML.SuperBomberman3.Game.Data;

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
        private const uint MAX_BOMB_ON_MAP = 0x14;
        private const uint MIN_EXPLOSION_EXPANDER = 0x02;
        private const uint MAX_EXPLOSION_EXPANDER = 0x09;
        private const uint MAX_BOMB_TIMER = 0x95;
        private const uint MIN_BOMB_TIMER = 0x01;
        //Our savestates are using 2 mins round time. Can be changed
        private const uint MAX_ROUND_TIME = 0x78;
        //Number of frames counted after detonation
        private const uint NUM_EXPLOSION_FRAMES = 0x20;
        //Number of frames counted from the plant to end of explosion
        private const uint NUM_BOMB_FRAMES = 0xB5;
        //Number of frames counted between the start of the explosion and the damage applied to the players
        private const uint NUM_DAMAGE_PLAYER_FRAMES = 0x03;
        //Number of frames counted between the start of the explosion and the damage applied to the structures
        private const uint NUM_DAMAGE_STRUCT_FRAMES = 0x1F;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly InternalClock internalClock;

        private ulong frameCounter;
        private byte[,] playableTilesCache;
        private uint destructibleTilesRemaining;
        private uint previousFrameDestructibleTilesRemaining;
        private int playersAliveCount;
        private int previousPlayersAliveCount;
        private bool[] playersDead;
        private Bomb[] bombsPlanted;
        private bool[] bombsPlantedIndex;
        private int enemiesEliminated;
        private int wallsDestroyed;

        public SB3DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SB3PluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);

            frameCounter = 0;

            destructibleTilesRemaining = 0;

            previousFrameDestructibleTilesRemaining = 0;

            playersAliveCount = 0;

            previousPlayersAliveCount = 0;

            enemiesEliminated = 0;

            wallsDestroyed = 0;

            playersDead = new bool[4];
            Array.Fill(playersDead, false);

            bombsPlantedIndex = new bool[MAX_BOMB_ON_MAP];
            Array.Fill(bombsPlantedIndex, false);

            bombsPlanted = new Bomb[MAX_BOMB_ON_MAP];
            for (int i = 0; i < MAX_BOMB_ON_MAP; i++)
            {
                bombsPlanted[i] = new Bomb();
            }

            playableTilesCache = new byte[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];
        }

        public bool[,] GetInternalClockState() => internalClock.GetStates();
        public byte GetDestructibleTilesRemaining() => ReadSingle(GameAddresses.DestructibleTilesRemaining);
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
        public int GetRemainingRoundTime() => (ReadSingle(GameAddresses.GameMinutesTimer) * 60) + ReadSingle(GameAddresses.GameSecondsTimer);
        public double GetRemainingRoundTimeNormalized() => GetRemainingRoundTime() / (double)MAX_ROUND_TIME;
        public int GetEnemyEliminated() => enemiesEliminated;
        public int GetWallsDestroyed() => wallsDestroyed;

        /// <summary>
        /// Needs to be called every frame to reset the memory cache
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();
            internalClock.NextFrame();
            InitFrameCache();
            playersAliveCount = GetNumbersOfPlayersAlive();
            destructibleTilesRemaining = GetDestructibleTilesRemaining();
            MapPlayableTiles();
            CheckPlayerDeathStatus();
            TrackBombPlanted();
            TrackBombExpired();
            TrackBombExploded();
            previousPlayersAliveCount = playersAliveCount;
            previousFrameDestructibleTilesRemaining = destructibleTilesRemaining;
            frameCounter++;
        }

        /// <summary>
        /// Needs to be called every time a save state was loaded to reset the global cache.
        /// </summary>
        public void NextState()
        {
            frameCache.Clear();
            internalClock.Reset();

            MapPlayableTiles();

            frameCounter = 0;

            destructibleTilesRemaining = 0;

            previousFrameDestructibleTilesRemaining = 0;

            enemiesEliminated = 0;

            wallsDestroyed = 0;

            Array.Fill(playersDead, false);

            Array.Fill(bombsPlantedIndex, false);

            bombsPlanted = new Bomb[MAX_BOMB_ON_MAP];
            for (int i = 0; i < MAX_BOMB_ON_MAP; i++)
            {
                bombsPlanted[i] = new Bomb();
            };
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
        public Tuple<uint, uint> BombToGridPos(uint coord) => new Tuple<uint, uint>(((coord - 1) / (DESIRED_LEVEL_WIDTH + 3)) - 1, (coord % (DESIRED_LEVEL_WIDTH + 3)) - 1);

        /// <summary>
        /// Convert the player coordinate to 2d grid coordinate. Useful when
        /// </summary>
        public Tuple<uint, uint> PlayerToGridPos() => new Tuple<uint, uint>((GetPlayerYPos()[0] - MIN_Y_POS) / TILES_HEIGHT, (GetPlayerXPos()[0] - MIN_X_POS) / TILES_WIDTH);

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
            byte[] bombsPos = GetBombsPos();
            byte[] bombsTimers = GetBombsTimer();

            //Draw Bombs
            for (int bombsIndex = 0; bombsIndex < bombsPos.Length; bombsIndex++)
            {
                if (bombsTimers[bombsIndex] != 0)
                {
                    var bombPos = BombToGridPos(bombsPos[bombsIndex]);
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
        /// Useful to keep track of the bombs that are being planted on the map as I havent found relevant addresses in game yet...
        /// This method is called every frames and is needed to keep track of the bomb planted
        /// </summary>
        public void TrackBombPlanted()
        {
            byte[] bombsPos = GetBombsPos();
            byte[] bombsTimers = GetBombsTimer();
            //Useful to know if the player has dropped a bomb as I havent found relevant addresses in game yet...
            var playerPos = PlayerToGridPos();

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //If the tile contains a bomb and the bomb is not tracked already
                    if (playableTilesCache[i, j] == 0x50 && !IsBombAlreadyTracked((uint)i, (uint)j))
                    {
                        //Track bomb
                        int bombIndex = GetBombIndex();

                        //If there was a free bomb index to assign (it should always be the case but its pretty muchimpossible to test what happens when there is more than 15+ bombs on the map...)
                        if (bombIndex != -1)
                        {
                            uint bombTimer = bombsTimers[bombIndex];
                            bool isExpired = false;
                            uint setToExpire = (uint)(bombTimer + frameCounter + NUM_EXPLOSION_FRAMES);
                            uint setToKill = (uint)(bombTimer + frameCounter + NUM_DAMAGE_PLAYER_FRAMES);
                            uint setToDestroy = (uint)(bombTimer + frameCounter + NUM_DAMAGE_STRUCT_FRAMES);
                            uint yTilePos = BombToGridPos(bombsPos[bombIndex]).Item1;
                            uint xTilePos = BombToGridPos(bombsPos[bombIndex]).Item2;

                            Bomb bomb = new Bomb(bombIndex, bombTimer, setToExpire, setToKill, setToDestroy, isExpired, yTilePos, xTilePos);
                            bombsPlanted[bombIndex] = bomb;

                            //Is the bomb planted by the player?
                            if (BombToGridPos(bombsPos[bombIndex]).Equals(playerPos))
                            {
                                bomb.IsPlantedByPlayer = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Useful to keep track of the bombs that are expired in the table. Method called on each frames.
        /// </summary>
        public void TrackBombExpired()
        {
            foreach (Bomb bomb in bombsPlanted)
            {
                //If the bomb is expired, we want to flag it and free its index
                if (bomb.SetToExpire == frameCounter)
                {
                    bomb.IsExpired = true;
                    FreeBombIndex(bomb.BombIndex);
                }
            }
        }

        /// <summary>
        /// Track Bomb exploded. We want to know if a bomb detonated by the player blew up an enemy of a (some) destructible walls
        /// Right now, the function doesnt check which bomb exactly killed an ennemy, so if player and enemy 1 set a bomb at the exact same frame
        /// and the bomb blow up enemy 2, the points are given to the player. Same thing goes for the destructible blocks.
        /// </summary>
        public void TrackBombExploded()
        {
            bool wallCheck = false;
            bool killCheck = false;

            foreach (Bomb bomb in bombsPlanted)
            {
                //If the bomb is not expired
                if (bomb.IsExpired == false)
                {
                    //If the bomb destroyed a destructible wall
                    if (!wallCheck && bomb.IsPlantedByPlayer && bomb.SetToDestroy == frameCounter && destructibleTilesRemaining < previousFrameDestructibleTilesRemaining)
                    {
                        wallsDestroyed += (previousFrameDestructibleTilesRemaining - destructibleTilesRemaining);
                        //With certain powerups, the player can plant many bombs at once, we already give it credits for all instances this way.
                        wallCheck = true;
                    }

                    //If the player killed an enemy
                    if (!killCheck && bomb.IsPlantedByPlayer && bomb.SetToKill == frameCounter && playersAliveCount < previousPlayersAliveCount)
                    {
                        enemiesEliminated += previousPlayersAliveCount - playersAliveCount;
                        //With certain powerups, the player can plant many bombs at once, we already give it credits for all instances this way.
                        killCheck = true;
                    }
                }
            }
        }

        /// <summary>
        /// Check if the bomb is already tracked and havent exploded yet.
        /// </summary>
        public bool IsBombAlreadyTracked(uint y, uint x)
        {
            bool result = false;

            foreach (Bomb bomb in bombsPlanted)
            {
                //Check if a bomb is already planted at the position. There cant be 2 bombs on the same tile.
                if (bomb.XTilePos == x && bomb.YTilePos == y && bomb.IsExpired == false)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the next index to check in method GetBombTimer() and GetBombPosition() as they act as a FIFO Queue.
        /// </summary>
        public int GetBombIndex()
        {
            int result = -1;

            for (int index = 0; index < MAX_BOMB_ON_MAP; index++)
            {

                if (bombsPlantedIndex[index] == false)
                {
                    bombsPlantedIndex[index] = true;
                    result = index;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Free the bomb index in the FIFO Queue
        /// </summary>
        public void FreeBombIndex(int index)
        {
            bombsPlantedIndex[index] = false;
        }

        /// <summary>
        /// Get the powerup that is the closest to the player on the grid using manhattan distance.
        /// Values are normalized. If there is no powerup present, the function will return a pair with values = (1.0, 1.0)
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
            GameAddresses.DestructibleTilesRemaining,
            GameAddresses.GameSecondsTimer,
            GameAddresses.GameMinutesTimer,
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
