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
        /*We use 4 players for our savestates to replicate the usual bomberman experience, but the game itself support from 2 to 5 players.
         *It can and should be changed if the savestates uses a different value*/
        public const uint NUM_PLAYERS = 0x04;
        //Number of ennemies in the game.
        public const uint NUM_ENEMIES = NUM_PLAYERS - 1;
        /*We want to draw only the "navigable" part of the grid for our training which is 13 tiles wide. 
         *Some levels have teleporters on the far edges and need 15 tiles wide. We do not take that for account in our implementation.*/
        public const uint DESIRED_LEVEL_WIDTH = 0xD;
        /*We want to draw only the "navigable" part of the grid for our training which is 11 tiles long. 
        *Some levels have teleporters on the far edges and need 13 tiles long. We do not take that for account in our implementation.*/
        public const uint DESIRED_LEVEL_HEIGHT = 0xB;
        //Tiles width/height in pixels. 
        public const uint TILES_WIDTH = 0x10;
        public const uint TILES_HEIGHT = 0x10;
        //Minimum player X position on the map in pixels.
        private const uint MIN_X_POS = 0x10;
        //Minimum player Y position on the map in pixels.
        private const uint MIN_Y_POS = 0x10;
        //Maximum player X position on the map in pixels.
        private const uint MAX_X_POS = 0xD0;
        //Minimum player Y position on the map in pixels.
        private const uint MAX_Y_POS = 0xB0;
        //Maximum accelerator level of a player.
        private const uint MAX_ACCELERATOR = 0x100;
        //Maximum amount of bomb a player can drop at once.
        private const uint MAX_BOMB = 0x9;
        //Maximum amount that we deem can be on the map at once. In theory it should be 60, but its hard to test in game... 20 should be safe enough for 4 players.
        private const uint MAX_BOMB_ON_MAP = 0x14;
        //Minimum explosion expander level that a player starts with. It means that the explosion will be of 1 + 2 (expander level) width and height.
        private const uint MIN_EXPLOSION_EXPANDER = 0x02;
        //Maximum explosion expander level that a player can reach. It means that the explosion will be of 1 + 9 (expander level) width and height.
        private const uint MAX_EXPLOSION_EXPANDER = 0x09;
        //Represent the start of the bomb explosion countdown after a bomb is dropped.
        private const uint MAX_BOMB_TIMER = 0x95;
        //Represent the minimum value of the explosion countdown.
        private const uint MIN_BOMB_TIMER = 0x01;
        //For some bombermans models, the death timer starts at 0x3B.
        private const uint DEATH_TIMER_START = 0x3B;
        //For some others bombermans models, the death timer starts at 0x3C.
        private const uint DEATH_TIMER_START_2 = 0x3C;
        //Our savestates are using 2 mins round time (default). It can and should be changed if the savestates uses a different value.
        private const uint MAX_ROUND_TIME = 0x78;
        //Number of frames counted after detonation. Represent the number of frames where the explosion is active.
        private const uint NUM_EXPLOSION_FRAMES = 0x20;
        //Number of frames counted between the start of the explosion and the damage applied to the players.
        private const uint NUM_START_EXPLOSION_PLAYER_DAMAGE_FRAMES = 0x05;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly InternalClock internalClock;

        private byte[,] playableTilesCache;
        private int enemiesAliveCount;
        private int previousEnemiesAliveCount;
        private int destructibleTilesRemaining;
        private int previousFrameDestructibleTilesRemaining;
        private bool[] playersAliveStatus;
        private Bomb[] bombsPlanted;
        private bool[] bombsPlantedIndex;
        private int enemiesEliminated;
        private int wallsDestroyed;
        private int frameCounter;

        public SB3DataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, SB3PluginConfig pluginConfig)
        {
            this.emulator = emulator;
            frameCache = new();
            internalClock = new InternalClock(pluginConfig.InternalClockTickLength, pluginConfig.InternalClockLength);

            frameCounter = 0;
            destructibleTilesRemaining = 0;
            previousFrameDestructibleTilesRemaining = 0;
            enemiesAliveCount = 0;
            previousEnemiesAliveCount = 0;
            enemiesEliminated = 0;
            wallsDestroyed = 0;

            playersAliveStatus = new bool[NUM_PLAYERS];
            Array.Fill(playersAliveStatus, true);

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
        public int GetRemainingRoundTime() => (ReadSingle(GameAddresses.GameMinutesTimer) * 60) + ReadSingle(GameAddresses.GameSecondsTimer);
        public double GetRemainingRoundTimeNormalized() => GetRemainingRoundTime() / (double)MAX_ROUND_TIME;
        public byte[] GetTiles() => Read(GameAddresses.DynamicTiles);
        public byte GetDestructibleTilesRemaining() => ReadSingle(GameAddresses.DestructibleTilesRemaining);
        public byte[] GetPlayersXPos() => Read(PlayersAddresses.PlayersXPos);
        public byte[] GetPlayersYPos() => Read(PlayersAddresses.PlayersYPos);
        public double GetPlayerXPositionNormalized(int pos) => (pos - MIN_X_POS) / (double)(MAX_X_POS - MIN_X_POS);
        public double GetPlayerYPositionNormalized(int pos) => (pos - MIN_Y_POS) / (double)(MAX_Y_POS - MIN_Y_POS);
        public double GetMainPlayerXPositionNormalized() => GetPlayersXPos()[0] >= 0x10 ? GetPlayerXPositionNormalized(GetPlayersXPos()[0]) : 0.0;
        public double GetMainPlayerYPositionNormalized() => GetPlayersYPos()[0] >= 0x10 ? GetPlayerYPositionNormalized(GetPlayersYPos()[0]) : 0.0;
        public byte[] GetBombsPos() => Read(GameAddresses.BombsPositions);
        public byte[] GetBombsTimer() => Read(GameAddresses.BombsTimers);
        public double GetBombsTimerNormalized(byte timer) => 1.0 - (timer / (double) MAX_BOMB_TIMER);
        public byte GetMainPlayerBombsPlanted() => Read(PlayersAddresses.PlayersBombsPlantedCount)[0];
        public double GetMainPlayerBombsPlantedNormalized() => GetMainPlayerBombsPlanted() / (double)MAX_BOMB;
        public double GetClosestPowerupToMainPlayerXPosNormalized() => GetClosestPowerUp(playableTilesCache).Item1;
        public double GetClosestPowerupToMainPlayerYPosNormalized() => GetClosestPowerUp(playableTilesCache).Item2;
        public byte GetMainPlayerExtraBombPowerUpLevel() => Read(PowerupsAddresses.ExtraBomb)[0];
        public byte GetMainPlayerExplosionExpanderPowerUpLevel() => Read(PowerupsAddresses.ExplosionExpander)[0];
        public byte GetMainPlayerAcceleratorPowerUpLevel() => Read(PowerupsAddresses.Accelerator)[0];
        public double GetMainPlayerExtraBombPowerUpLevelNormalized() => GetMainPlayerExtraBombPowerUpLevel() / (double)MAX_BOMB;
        public double GetMainPlayerExplosionExpanderPowerUpLevelNormalized() => GetMainPlayerExplosionExpanderPowerUpLevel() / (double)(MAX_EXPLOSION_EXPANDER - MIN_EXPLOSION_EXPANDER);
        public double GetMainPlayerAcceleratorPowerUpLevelNormalized() => GetMainPlayerAcceleratorPowerUpLevel() / (double)MAX_ACCELERATOR;
        public bool IsMainPlayerOnLouie() => ReadSingle(PowerupsAddresses.IsOnLouie) == 0;
        public bool IsLouieColourYellow() => IsMainPlayerOnLouie() ? ToUnsignedInteger(Read(PowerupsAddresses.MountedLouieColours)) == 0x35F02DD : false;
        public bool IsLouieColourBrown() => IsMainPlayerOnLouie() ? ToUnsignedInteger(Read(PowerupsAddresses.MountedLouieColours)) == 0x35B2210D : false;
        public bool IsLouieColourPink() => IsMainPlayerOnLouie() ? ToUnsignedInteger(Read(PowerupsAddresses.MountedLouieColours)) == 0x7DFF695A : false;
        public bool IsLouieColourGreen() => IsMainPlayerOnLouie() ? ToUnsignedInteger(Read(PowerupsAddresses.MountedLouieColours)) == 0x1BE00AC : false;
        public bool IsLouieColourBlue() => IsMainPlayerOnLouie() ? ToUnsignedInteger(Read(PowerupsAddresses.MountedLouieColours)) == 0x7E805940 : false;
        public double GetLouieColour() => IsLouieColourYellow() ? 0.2 : IsLouieColourBrown() ? 0.4 : IsLouieColourPink() ? 0.6 : IsLouieColourGreen() ? 0.8 : IsLouieColourBlue() ? 1.0 : 0;
        public bool GetMainPlayerKickUpgradeState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x02) != 0;
        public bool GetMainPlayerGloveUpgradeState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x04) != 0;
        public bool GetMainPlayerSlimeBombUpgradeState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x20) != 0;
        public bool GetMainPlayerPowerBombUpgradeState() => (ReadSingle(PowerupsAddresses.BombermanUpgrade) & 0x40) != 0;
        public byte[] GetPlayersDeathTimer() => Read(PlayersAddresses.PlayersDeathTimer);
        public int GetNumberOfPlayersAlive() => playersAliveStatus.Where(c => c).Count();
        public bool IsMainPlayerDead() => playersAliveStatus[0] == false;
        public bool IsPlayerDead(int index) => playersAliveStatus[index] == false;
        //By conventional means != draw
        public bool IsRoundOver() => GetNumberOfPlayersAlive() == 1;
        public bool IsRoundWon() => IsRoundOver() && !IsMainPlayerDead();
        public bool IsRoundLost() => IsRoundOver() && IsMainPlayerDead();
        public bool IsRoundDraw() => GetNumberOfPlayersAlive() == 0;
        public int GetNumberOfEnemiesEliminated() => enemiesEliminated;
        public int GetNumberOfWallsDestroyed() => wallsDestroyed;

        /// <summary>
        /// Convert Bomb coordinate varying from 17 to 189 to 2d grid coordinate varying from 0 to 13 (horizontally) and from 0 to 11 (vertically).
        /// </summary>
        public Tuple<uint, uint> BombToGridPos(uint coord) => new Tuple<uint, uint>(((coord - 1) / (DESIRED_LEVEL_WIDTH + 3)) - 1, (coord % (DESIRED_LEVEL_WIDTH + 3)) - 1);

        /// <summary>
        /// Convert the player coordinate to 2d grid coordinate.
        /// </summary>
        public Tuple<uint, uint> MainPlayerToGridPos() => new Tuple<uint, uint>((GetPlayersYPos()[0] - MIN_Y_POS) / TILES_HEIGHT, (GetPlayersXPos()[0] - MIN_X_POS) / TILES_WIDTH);

        /// <summary>
        /// The tile map that is read in memory is of size 176 (11 x 16 tiles). However, the playable area in the game is only 11 x 13.
        /// This function is called for every frames and convert the flat tileMap into the 2d playableTileMap class variable for other functions.
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
        /// This function is called for every frames and check if some players died.
        /// </summary>
        public void CheckPlayerDeathStatus()
        {
            byte[] timers = GetPlayersDeathTimer();

            for (int playerIndex = 0; playerIndex < NUM_PLAYERS; playerIndex++)
            {
                //If player is not already flagged as dead.
                if (playersAliveStatus[playerIndex] != false)
                {
                    //If the timer associated with the player == 59 (0x3B) or 60 (0x3C) for some players, it means the player just died.
                    if (timers[playerIndex] == DEATH_TIMER_START || timers[playerIndex] == DEATH_TIMER_START_2)
                    {
                        playersAliveStatus[playerIndex] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Return an array of double representing normalized horizontal distance between the player and every enemies. 
        /// </summary>
        public double[,] GetEnemiesXDistanceToThePlayer()
        {
            double[,] result = new double[NUM_ENEMIES, 1];

            double playerXPosInLevel = GetMainPlayerXPositionNormalized();
            byte[] enemyXPos = GetPlayersXPos();

            for (int enemy = 0; enemy < NUM_ENEMIES; enemy++)
            {
                double enemyXPosInLevel = GetPlayerXPositionNormalized(enemyXPos[enemy + 1]);

                //If the enemy is alive
                if (!IsPlayerDead(enemy + 1))
                {
                    result[enemy, 0] = enemyXPosInLevel - playerXPosInLevel;
                }
                //If the enemy is dead
                else if (IsPlayerDead(enemy + 1))
                {
                    result[enemy, 0] = 0.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Return an array of double representing normalized vertical distance between the player and every enemies. 
        /// </summary>
        public double[,] GetEnemiesYDistanceToThePlayer()
        {
            double[,] result = new double[NUM_ENEMIES, 1];

            double playerYPosInLevel = GetMainPlayerYPositionNormalized();
            byte[] enemyYPos = GetPlayersYPos();

            for (int enemy = 0; enemy < NUM_ENEMIES; enemy++)
            {
                double enemyYPosInLevel = GetPlayerYPositionNormalized(enemyYPos[enemy + 1]);

                //If the enemy is alive
                if (!IsPlayerDead(enemy + 1))
                {
                    result[enemy, 0] = enemyYPosInLevel - playerYPosInLevel;
                }
                //If the enemy is dead
                else if (IsPlayerDead(enemy + 1))
                {
                    result[enemy, 0] = 0.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Draw game tiles. There is 3 types of tiles to draw : undestructible, destructible and empty.
        /// </summary>
        public double[,] DrawTiles()
        {
            double[,] result = new double[DESIRED_LEVEL_HEIGHT, DESIRED_LEVEL_WIDTH];

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //Tile is undestructible.
                    if (playableTilesCache[i, j] == 0x80 || playableTilesCache[i, j] == 0x84) result[i, j] = 1.0;
                    //Tile is destructible.
                    else if (playableTilesCache[i, j] == 0x30) result[i, j] = 0.5;
                    //Tile is free.
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

            //Draw Bombs.
            for (int bombsIndex = 0; bombsIndex < bombsPos.Length; bombsIndex++)
            {
                if (bombsTimers[bombsIndex] != 0)
                {
                    var bombPos = BombToGridPos(bombsPos[bombsIndex]);
                    var bombTimer = GetBombsTimerNormalized(bombsTimers[bombsIndex]);
                    result[bombPos.Item1, bombPos.Item2] = bombTimer;
                }
            }

            //Draw Explosions and other dangers.
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
        /// Values are normalized. If there is no powerup present, the function will return a pair with values = (1.0, 1.0)
        /// Values are already normalized.
        /// </summary>
        public Tuple<double, double> GetClosestPowerUp(byte[,] tilesCache)
        {
            var playerXPos = GetPlayersXPos()[0];
            var playerYPos = GetPlayersYPos()[0];
            double closestX = 1.0;
            double closestY = 1.0;
            int closestDist = int.MaxValue;

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //If tile contains a powerup
                    if (tilesCache[i, j] == 0x10)
                    {
                        int powerupXPos = (int)(j * TILES_WIDTH) + (int)MIN_X_POS;
                        int powerupYPos = (int)(i * TILES_HEIGHT) + (int)MIN_Y_POS;

                        //If the powerup is the closest found to date, we want to tag it.
                        var dist = Utils.MathUtils.ManhattanDistance(playerXPos, playerYPos, powerupXPos, powerupYPos);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            //Normalize values
                            closestX = (powerupXPos - playerXPos) / (double)(MAX_X_POS - MIN_X_POS);
                            closestY = (powerupYPos - playerYPos) / (double)(MAX_Y_POS - MIN_Y_POS);
                        }
                    }
                }
            }

            return new Tuple<double, double>(closestX, closestY);
        }

        /// <summary>
        /// Useful to keep track of the bombs that are being planted on the map but needs to be called for every frames.
        /// </summary>
        public void TrackBombPlanted()
        {
            byte[] bombsPos = GetBombsPos();
            var mainPlayerPos = MainPlayerToGridPos();

            for (int i = 0; i < DESIRED_LEVEL_HEIGHT; i++)
            {
                for (int j = 0; j < DESIRED_LEVEL_WIDTH; j++)
                {
                    //If the tile contains a bomb and the bomb is not tracked already.
                    if (playableTilesCache[i, j] == 0x50 && !IsBombAlreadyTracked((uint)i, (uint)j))
                    {
                        //Track the bomb.
                        int bombIndex = GetBombIndex();

                        //If there was a free bomb index to assign (it should always be the case but its pretty much impossible to test what happens when there is more than 15+ bombs on the map...).
                        if (bombIndex != -1)
                        {
                            bool isExpired = false;
                            uint setToExpire = (uint)(MAX_BOMB_TIMER + frameCounter + NUM_EXPLOSION_FRAMES);
                            uint setToKill = (uint)(MAX_BOMB_TIMER + frameCounter + NUM_START_EXPLOSION_PLAYER_DAMAGE_FRAMES);
                            uint setToDestroy = (uint)(MAX_BOMB_TIMER + frameCounter);
                            uint yTilePos = BombToGridPos(bombsPos[bombIndex]).Item1;
                            uint xTilePos = BombToGridPos(bombsPos[bombIndex]).Item2;

                            Bomb bomb = new(bombIndex, isExpired, yTilePos, xTilePos, setToExpire, setToKill, setToDestroy);
                            bombsPlanted[bombIndex] = bomb;

                            //If the bomb position is equal to the main player position, it means that he planted the bomb.
                            if (BombToGridPos(bombsPos[bombIndex]).Equals(mainPlayerPos))
                            {
                                bomb.IsPlantedByMainPlayer = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Useful to keep track of the bombs that are expired. Method called on each frames.
        /// </summary>
        public void TrackBombExpired()
        {
            foreach (Bomb bomb in bombsPlanted)
            {
                //If the bomb is expired, we want to flag it and free its index.
                if (bomb.SetToExpire <= frameCounter)
                {
                    bomb.IsExpired = true;
                    FreeBombIndex(bomb.QueueBombIndex);
                }
            }
        }

        /// <summary>
        /// Track Bomb exploded. We want to know if a bomb detonated by the main player killed an enemy of a (some) destructible walls.
        /// Right now, the function doesnt check which bomb exactly killed an ennemy, so if the main player and enemy set a bomb at the exact same frame,
        /// and it killed an enemy, it will reward the kill to the main player. 
        /// This function needs to be called for every frames.
        /// </summary>
        public void TrackBombExploded()
        {
            bool wallCheck = false;
            bool killCheck = false;

            foreach (Bomb bomb in bombsPlanted)
            {
                //If the bomb is not expired.
                if (bomb.IsExpired == false)
                {
                    //If the bomb destroyed a destructible wall.
                    if (!wallCheck && bomb.IsPlantedByMainPlayer && bomb.SetToDestroyWalls == frameCounter && destructibleTilesRemaining < previousFrameDestructibleTilesRemaining)
                    {
                        wallsDestroyed += previousFrameDestructibleTilesRemaining - destructibleTilesRemaining;
                        //With certain powerups, the player can plant many bombs at once, we already give it credits for all instances this way.
                        wallCheck = true;
                    }

                    //If the player killed an enemy. Some bomberman models requires 1 more frame to start death animation. Need to also check if the main player didnt kill himself.
                    if (!killCheck && bomb.IsPlantedByMainPlayer && (bomb.SetToDamagePlayers == frameCounter || bomb.SetToDamagePlayers == frameCounter + 1) && enemiesAliveCount < previousEnemiesAliveCount && !IsMainPlayerDead())
                    {
                        enemiesEliminated += previousEnemiesAliveCount - enemiesAliveCount;
                        //With certain powerups, the player can plant many bombs at once, we already give it credits for all instances this way. We only need to check one main player bomb explosion per frames.
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
                //Check if a bomb is already planted at the position. There cant be 2 bombs on the same tile which means that there cant be 2 bombs tracked at the same position.
                if (bomb.XTilePos == x && bomb.YTilePos == y && bomb.IsExpired == false)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns first free index in the bomb Queue. Needed to associate our bombs objects with an index in the game FIFO bomb queue.
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
        /// Free the bomb index in the FIFO Queue.
        /// </summary>
        public void FreeBombIndex(int index)
        {
            bombsPlantedIndex[index] = false;
        }

        private byte ReadSingle(AddressData addressData) => Read(addressData)[0];

        /// <summary>
        /// Reads a specific amount of bytes from the emulator's memory, using the AddressData.
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>

        /// <summary>
        /// Reads a specific amount of bytes from the emulator's memory, using the AddressData.
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
        /// Reads multiple ranges of addresses.
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

        private static uint ToUnsignedInteger(byte[] bytes)
        {
            uint value = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                value += (uint)bytes[i] << i * 8;
            }
            return value;
        }

        private void InitFrameCache()
        {
            List<AddressData> toRead = new()
        {
            GameAddresses.DynamicTiles,
            GameAddresses.BombsPositions,
            GameAddresses.BombsTimers,
            GameAddresses.DestructibleTilesRemaining,
            GameAddresses.GameSecondsTimer,
            GameAddresses.GameMinutesTimer,
            PlayersAddresses.PlayersXPos,
            PlayersAddresses.PlayersYPos,
            PlayersAddresses.PlayersBombsPlantedCount,
            PlayersAddresses.PlayersDeathTimer,
            PowerupsAddresses.ExplosionExpander,
            PowerupsAddresses.Accelerator,
            PowerupsAddresses.ExtraBomb,
            PowerupsAddresses.IsOnLouie,
            PowerupsAddresses.MountedLouieColours,
            PowerupsAddresses.BombermanUpgrade,
        };
            _ = Read(toRead.ToArray());
        }

        /// <summary>
        /// Needs to be called every frame to reset the memory cache.
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();
            internalClock.NextFrame();
            InitFrameCache();
            CheckPlayerDeathStatus();
            enemiesAliveCount = IsMainPlayerDead() ? GetNumberOfPlayersAlive() : GetNumberOfPlayersAlive() - 1;
            destructibleTilesRemaining = GetDestructibleTilesRemaining();
            MapPlayableTiles();
            TrackBombExpired();
            TrackBombPlanted();
            TrackBombExploded();
            previousEnemiesAliveCount = enemiesAliveCount;
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
            enemiesAliveCount = 0;
            previousEnemiesAliveCount = 0;
            enemiesEliminated = 0;
            wallsDestroyed = 0;
            Array.Fill(playersAliveStatus, true);
            Array.Fill(bombsPlantedIndex, false);
            bombsPlanted = new Bomb[MAX_BOMB_ON_MAP];
            for (int i = 0; i < MAX_BOMB_ON_MAP; i++)
            {
                bombsPlanted[i] = new Bomb();
            };
        }
    }
}
