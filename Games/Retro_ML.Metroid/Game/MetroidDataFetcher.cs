using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Metroid.Configuration;
using Retro_ML.Metroid.Game.Data;
using Retro_ML.Metroid.Game.Navigation;
using Retro_ML.Utils;
using static Retro_ML.Metroid.Game.Addresses;

namespace Retro_ML.Metroid.Game;

/// <summary>
/// Takes care of abstracting away the addresses when communicating with the emulator.
/// </summary>
internal class MetroidDataFetcher : IDataFetcher
{
    private const int TILE_SIZE = 0x8;
    private const int META_TILE_SIZE = 0x10;
    private const int ROOM_WIDTH = 0x20;
    private const int CORRECTED_ROOM_WIDTH = ROOM_WIDTH / 2;
    private const int ROOM_HEIGHT = 0x1E;
    private const int CORRECTED_ROOM_HEIGHT = ROOM_HEIGHT / 2;

    public const int MAXIMUM_VERTICAL_SPEED = 0x500;
    public const int MAXIMUM_HORIZONTAL_SPEED = 0x180;
    public const int INVINCIBILITY_TIMER_LENGTH = 50;
    public const int TANK_HEALTH = 1000;

    private readonly IEmulatorAdapter emulator;
    private readonly Dictionary<uint, byte[]> frameCache;
    private readonly Dictionary<uint, byte[]> roomCache;
    private readonly MetroidPluginConfig pluginConfig;
    private readonly InternalClock internalClock;

    private byte previousScrollX;
    private byte previousScrollY;
    private int delayRoomCacheClearTimer;
    private bool wasSamusInDoor;
    private (byte x, byte y) currentMapPosition;
    private Navigator navigator;

    public int RealTileSize => pluginConfig.DoublePrecision ? TILE_SIZE : META_TILE_SIZE;
    public int RealRoomWidth => pluginConfig.DoublePrecision ? ROOM_WIDTH : CORRECTED_ROOM_WIDTH;
    public int RealRoomHeight => pluginConfig.DoublePrecision ? ROOM_HEIGHT : CORRECTED_ROOM_HEIGHT;

    public MetroidDataFetcher(IEmulatorAdapter emulator, NeuralConfig neuralConfig, MetroidPluginConfig pluginConfig)
    {
        this.emulator = emulator;
        frameCache = new();
        roomCache = new();
        this.pluginConfig = pluginConfig;
        previousScrollX = 0;
        previousScrollY = 0;
        navigator = new Navigator();
        currentMapPosition = (0, 0);
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
        VerifyRoomCache();
        UpdateRealMapPosition();
    }

    /// <summary>
    /// Needs to be called every time a save state was loaded to reset the global cache.
    /// </summary>
    public void NextState()
    {
        frameCache.Clear();
        roomCache.Clear();

        previousScrollX = 0;
        previousScrollY = 0;
        currentMapPosition = (0, 0);
        wasSamusInDoor = false;

        navigator = new Navigator();

        internalClock.Reset();
        delayRoomCacheClearTimer = 5;

        UpdateRealMapPosition();
    }

    public bool[,] GetInternalClockState() => internalClock.GetStates();

    public ushort GetSamusHealth() => (ushort)ReadNybbleDigitsToUlong(Samus.Health);
    public ushort GetMaximumHealth() => (ushort)((ReadSingle(Progress.EnergyTanks) + 1) * TANK_HEALTH - 1);
    public double GetSamusHealthRatio() => GetSamusHealth() / (double)GetMaximumHealth();
    public short GetSamusYSpeed() => (short)(((sbyte)ReadSingle(Samus.VerticalSpeed)) * 0x100 + ReadSingle(Samus.VerticalFractionalSpeed));
    public short GetSamusXSpeed() => (short)(((sbyte)ReadSingle(Samus.HorizontalSpeed)) * 0x100 + ReadSingle(Samus.HorizontalFractionalSpeed));
    public byte GetSamusXPosition() => ReadSingle(Samus.XPosition);
    public byte GetSamusYPosition() => ReadSingle(Samus.YPosition);
    public (int xPos, int yPos) GetSamusScreensPosition() => GetScreensPosition(GetSamusXPosition() / RealTileSize, GetSamusYPosition() / RealTileSize, IsSamusInFirstScreen());
    public double SamusLookDirection() => (ReadSingle(Samus.LookingDirection) * -2.0) + 1;
    public bool IsSamusInMorphBall() => ReadSingle(Samus.Status) == 0x3;
    public bool IsSamusInLava() => ReadSingle(Samus.InLava) == 1;
    public bool IsMetroidOnSamusHead() => ReadSingle(Samus.HasMetroidOnHead) == 1;
    public bool IsSamusOnElevator() => ReadSingle(Samus.IsOnElevator) == 1;
    public bool IsSamusUsingMissiles() => ReadSingle(Samus.UsingMissiles) == 1;
    public bool IsSamusInDoor() => ReadSingle(Gamestate.InADoor) != 0;
    public bool IsSamusInFirstScreen() => ReadSingle(Samus.CurrentScreen) == 0;
    public bool IsSamusFrozen() => ReadSingle(Gamestate.FreezeTimer) != 0;
    public bool IsSamusGrounded() => GetSamusYSpeed() == 0;
    public bool IsBossPresent() => ReadSingle(Gamestate.IsMiniBossPresent) != 0;

    public bool HasBombs() => (ReadSingle(Progress.Equipment) & 0b0000_0001) != 0;
    public bool HasHighJump() => (ReadSingle(Progress.Equipment) & 0b0000_0010) != 0;
    public bool HasLongBeam() => (ReadSingle(Progress.Equipment) & 0b0000_0100) != 0;
    public bool HasScrewAttack() => (ReadSingle(Progress.Equipment) & 0b0000_1000) != 0;
    public bool HasMorphBall() => (ReadSingle(Progress.Equipment) & 0b0001_0000) != 0;
    public bool HasVariaSuit() => (ReadSingle(Progress.Equipment) & 0b0010_0000) != 0;
    public bool HasWaveBeam() => (ReadSingle(Progress.Equipment) & 0b0100_0000) != 0;
    public bool HasIceBeam() => (ReadSingle(Progress.Equipment) & 0b1000_0000) != 0;
    public bool HasMissiles() => ReadSingle(Progress.MissileCapacity) > 0;
    public bool KilledKraid() => false; //TODO : Detect whether or not kraid was killed
    public bool KilledRidley() => false; //TODO : detect whether or not ridley was killed
    public bool KilledMotherBrain() => false; //TODO : detect whether or not mother brain was killed (maybe use timer?)

    public double GetSamusInvincibilityTimerRatio() => ReadSingle(Samus.InvincibleTimer) / (double)INVINCIBILITY_TIMER_LENGTH;
    public double GetCurrentMissileRatio() => ReadSingle(Progress.Missiles) / Math.Max(1.0, ReadSingle(Progress.MissileCapacity));
    public double GetSamusYSpeedRatio() => GetSamusYSpeed() / (double)MAXIMUM_VERTICAL_SPEED;
    public double GetSamusXSpeedRatio() => GetSamusXSpeed() / (double)MAXIMUM_HORIZONTAL_SPEED;

    public bool IsHorizontalRoom() => (ReadSingle(Room.HorizontalOrVertical) & 0b0000_1000) == 0;
    public bool IsOnNameTable3() => (ReadSingle(Room.PPUCTL0) & 0b0000_0011) != 0;
    public bool DoorOnNameTable0(bool leftSide) => (ReadSingle(Room.DoorOnNameTable0) & (leftSide ? 0b0001 : 0b0010)) != 0;
    public bool DoorOnNameTable3(bool leftSide) => (ReadSingle(Room.DoorOnNameTable3) & (leftSide ? 0b0001 : 0b0010)) != 0;
    public byte GetScrollX() => ReadSingle(Room.ScrollX);
    public byte GetScrollY() => ReadSingle(Room.ScrollY);
    public (byte x, byte y) GetMapPosition() => (ReadSingle(Gamestate.MapX), ReadSingle(Gamestate.MapY));
    public (byte x, byte y) GetRealMapPosition() => currentMapPosition;

    public (int x, int y) GetLastScrollDirection() => ReadSingle(Gamestate.LastScrollDirection) switch
    {
        0 => (0, -1),
        1 => (0, 1),
        2 => (-1, 0),
        3 => (1, 0),
        _ => (0, 0)
    };

    public bool CanSamusAct() => (IsSamusInDoor(), IsSamusFrozen()) == (false, false);

    /// <summary>
    /// Returns whether or not Samus can pass under the blocks in front of her. Can tell her when she has to use her Morph ball
    /// <code>
    /// S : Samus
    /// X : Solid
    /// - : Space
    /// . : Anything
    /// 
    /// Left side
    /// XS.
    /// -S.
    /// XX.
    /// 
    /// Right side
    /// .SX
    /// .S-
    /// .XX
    /// </code>
    /// </summary>
    public bool CanPassUnder()
    {
        if (!IsSamusGrounded()) return false;
        var tiles = GetTiles(1, 1);
        int dirr = (int)SamusLookDirection();

        return Tile.IsSolid(tiles[0, 1 + dirr])
               && !Tile.IsSolid(tiles[1, 1 + dirr])
               && Tile.IsSolid(tiles[2, 1])
               && Tile.IsSolid(tiles[2, 1 + dirr]);
    }

    /// <summary>
    /// Returns whether or not samus is in front of a door
    /// </summary>
    public bool IsInFrontOfDoor() => GetDoorInfo().isInFront;

    /// <summary>
    /// Returns whether or not Samus is in front of a missile door
    /// </summary>
    public bool IsInFrontOfMissileDoor() => GetDoorInfo() switch
    {
        (true, true, false) => true,
        _ => false
    };

    private (bool isInFront, bool isMissile, bool isOpen) GetDoorInfo()
    {
        byte xPos = GetSamusXPosition();
        var samusDirr = SamusLookDirection();
        //if xPos > 128, then Samus is in the right half of the screen
        (var doorStatusDirr, var doorFrameAddress, bool lookingTowardsDoor) = xPos > 128 ?
                                                                                        (-1, Room.RightDoorFrameOffset, samusDirr > 0) :
                                                                                        (1, Room.LeftDoorFrameOffset, samusDirr < 0);
        if (!lookingTowardsDoor) return (false, false, false);

        byte yPos = GetSamusYPosition();
        //120 is too low, 95 is too high
        if (yPos is > 120 or < 95) return (false, false, false);
        if (doorStatusDirr > 0 && (xPos is < 16 or > 48)) return (false, false, false);
        if (doorStatusDirr < 0 && (xPos is < 208 or > 240)) return (false, false, false);

        if (!IsSamusInFirstScreen()) doorFrameAddress = new AddressData(doorFrameAddress.Address + (Room.TilesNamespace3.Address - Room.TilesNamespace0.Address));

        byte[] res = Read(doorFrameAddress, new AddressData((uint)(doorFrameAddress.Address + doorStatusDirr)));
        (byte doorFrame, byte doorStatus) = (res[0], res[1]);

        if (!Tile.IsDoorFrame(doorFrame)) return (false, false, false);
        bool isOpen = !Tile.IsDoor(doorStatus);
        bool isMissile = Tile.IsMissileDoor(doorFrame);

        return (true, isMissile, isOpen);
    }

    /// <summary>
    /// Returns a 1xEquipment array containing the acquisition status of progression items. Here's the order of the items : 
    /// <code>
    /// Bombs
    /// High Jump
    /// Long Beam
    /// Screw Attack
    /// Morph Ball
    /// Varia Suit
    /// Wave Beam
    /// Ice Beam
    /// Missiles
    /// </code>
    /// Note that items that are disabled are just skipped
    /// </summary>
    /// <returns></returns>
    public bool[,] GetEquipment()
    {
        bool[,] result = new bool[1, pluginConfig.EquipmentCount];
        var equipFuncs = new Func<bool>[] {
           HasBombs, HasHighJump, HasLongBeam,
           HasScrewAttack, HasMorphBall, HasVariaSuit,
           HasWaveBeam, HasIceBeam, HasMissiles
        };
        int currIndex = 0;
        var equipStatus = pluginConfig.EquipmentStatus;
        for (int i = 0; i < equipStatus.Length; i++)
        {
            if (equipStatus[i]) result[0, currIndex++] = equipFuncs[i]();
        }

        return result;
    }

    /// <summary>
    /// Returns a 2x2 array representing the direction to go in, and the objective
    /// <code>
    /// 0,0 : x direction
    /// 0,1 : y direction
    /// 1,0 : obtain the powerup
    /// 1,1 : kill the enemy in the room
    /// </code>
    /// </summary>
    /// <returns></returns>
    public double[,] GetNavigationDirection()
    {
        var (xDirr, yDirr, objective) = navigator.GetDirectionToGo(currentMapPosition.x, currentMapPosition.y, this);

        return new double[,] { { xDirr, yDirr }, { objective == Objectives.Obtain ? 1 : 0, objective == Objectives.Kill ? 1 : 0 } };
    }

    public Objectives GetNavigationObjective() => navigator.GetDirectionToGo(currentMapPosition.x, currentMapPosition.y, this).objective;

    public bool[,] GetDangerousTilesAroundPosition()
    {
        var dist = pluginConfig.ViewDistance * (pluginConfig.DoublePrecision ? 2 : 1);
        var result = new bool[dist * 2 + 1, dist * 2 + 1];

        FetchSprites(result, GetEnemies());
        FetchSkreeProjectiles(result);

        return result;
    }

    /// <summary>
    /// Returns the direction to the nearest goodie, prioritizing powerups
    /// </summary>
    public double[,] GetDirectionToNearestGoodTile()
    {
        (int x, int y) = (0, 0);
        double minSquaredDist = double.PositiveInfinity;

        var pickups = GetPickups();
        var powerups = GetPowerups();
        (var samusX, var samusY) = GetSamusScreensPosition();

        foreach (var powerup in powerups)
        {
            (var xPos, var yPos) = GetScreensPosition(powerup[2] / RealTileSize, powerup[1] / RealTileSize, powerup[3] == 0);
            xPos -= samusX;
            yPos -= samusY;

            double currDist = MathUtils.Squared(xPos) + MathUtils.Squared(yPos);
            if (currDist <= minSquaredDist)
            {
                minSquaredDist = currDist;
                (x, y) = (xPos, yPos);
            }
        }

        if (minSquaredDist == double.PositiveInfinity)
        {
            foreach (var pickup in pickups)
            {
                (var xPos, var yPos) = GetScreensPosition(pickup.XPos / RealTileSize, pickup.YPos / RealTileSize, pickup.IsInFirstScreen);
                xPos -= samusX;
                yPos -= samusY;

                double currDist = MathUtils.Squared(xPos) + MathUtils.Squared(yPos);
                if (currDist <= minSquaredDist)
                {
                    minSquaredDist = currDist;
                    (x, y) = (xPos, yPos);
                }
            }
        }

        double dist = Math.Max(1, MathUtils.ApproximateSquareRoot(minSquaredDist));

        return new double[,] { { x / dist, y / dist } };
    }

    public bool[,] GetGoodTilesAroundPosition()
    {
        var dist = pluginConfig.ViewDistance * (pluginConfig.DoublePrecision ? 2 : 1);
        var result = new bool[dist * 2 + 1, dist * 2 + 1];

        FetchSprites(result, GetPickups());
        FetchPowerUps(result);

        return result;
    }

    public bool[,] GetSolidTilesAroundPosition()
    {
        var dist = pluginConfig.ViewDistance * (pluginConfig.DoublePrecision ? 2 : 1);
        var result = new bool[dist * 2 + 1, dist * 2 + 1];
        var tiles = GetTiles(dist, dist);

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                result[i, j] = Tile.IsSolid(tiles[i, j]);
            }
        }

        FetchSprites(result, GetEnemies().Where(e => e.IsFrozen));

        return result;
    }

    private void FetchSprites(bool[,] tiles, IEnumerable<Sprite> sprites)
    {
        int xDist = (tiles.GetLength(1) - 1) / 2;
        int yDist = (tiles.GetLength(0) - 1) / 2;

        (var samusX, var samusY) = GetSamusScreensPosition();

        foreach (var sprite in sprites)
        {
            var correctedXRadius = Math.Max(0, sprite.XRadius - 1);
            var correctedYRadius = Math.Max(0, sprite.YRadius - 1);

            byte left = (byte)(sprite.XPos - correctedXRadius);
            byte right = (byte)(sprite.XPos + correctedXRadius);
            byte top = (byte)(sprite.YPos - correctedYRadius);
            byte bottom = (byte)(sprite.YPos + correctedYRadius);

            if (sprite.YPos < top) top = byte.MinValue;
            if (sprite.YPos > bottom) bottom = byte.MaxValue;
            if (sprite.XPos < left) left = byte.MinValue;
            if (sprite.XPos > right) right = byte.MaxValue;

            (var xMin, var yMin) = GetScreensPosition(left / RealTileSize, top / RealTileSize, sprite.IsInFirstScreen);
            (var xMax, var yMax) = GetScreensPosition(right / RealTileSize, bottom / RealTileSize, sprite.IsInFirstScreen);

            xMin -= samusX;
            xMax -= samusX;
            yMin -= samusY;
            yMax -= samusY;

            for (int i = yMin; i <= yMax; i++)
            {
                for (int j = xMin; j <= xMax; j++)
                {
                    if (j >= -xDist && j <= xDist && i >= -yDist && i <= yDist)
                    {
                        tiles[i + yDist, j + xDist] = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns all alive enemies
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Sprite> GetEnemies()
    {
        var baseByteGroups = ReadMultiple(Sprites.BaseSingleSprite, Sprites.BaseSingleSprite, Sprites.AllBaseSprites).ToArray();
        var extraByteGroups = ReadMultiple(Sprites.ExtraSingleSprite, Sprites.ExtraSingleSprite, Sprites.AllExtraSprites).ToArray();

        for (int i = 0; i < baseByteGroups.Length; i++)
        {
            Sprite enemy = new(baseByteGroups[i], extraByteGroups[i]);

            if (enemy.IsAlive)
            {
                yield return enemy;
            }
        }
    }

    private void FetchSkreeProjectiles(bool[,] tiles)
    {
        int xDist = (tiles.GetLength(1) - 1) / 2;
        int yDist = (tiles.GetLength(0) - 1) / 2;

        var projectiles = SkreeProjectile.FromBytes(Read(Sprites.SkreeProjectiles));

        (var samusX, var samusY) = GetSamusScreensPosition();

        foreach (var projectile in projectiles)
        {
            if (!projectile.IsActive()) continue;

            (var xPos, var yPos) = GetScreensPosition(projectile.XPos / RealTileSize, projectile.YPos / RealTileSize, projectile.IsInFirstScreen);

            xPos -= samusX;
            yPos -= samusY;

            if (xPos >= -xDist && xPos <= xDist && yPos >= -yDist && yPos <= yDist)
            {
                tiles[yPos + yDist, xPos + xDist] = true;
            }
        }
    }

    private IEnumerable<Sprite> GetPickups()
    {
        var baseByteGroups = ReadMultiple(Sprites.BaseSingleSprite, Sprites.BaseSingleSprite, Sprites.AllBaseSprites).ToArray();
        var extraByteGroups = ReadMultiple(Sprites.ExtraSingleSprite, Sprites.ExtraSingleSprite, Sprites.AllExtraSprites).ToArray();

        for (int i = 0; i < baseByteGroups.Length; i++)
        {
            Sprite sprite = new(baseByteGroups[i], extraByteGroups[i]);

            if (sprite.IsPickup)
            {
                yield return sprite;
            }
        }
    }

    private void FetchPowerUps(bool[,] tiles)
    {
        int xDist = (tiles.GetLength(1) - 1) / 2;
        int yDist = (tiles.GetLength(0) - 1) / 2;

        (var samusX, var samusY) = GetSamusScreensPosition();
        var powerups = GetPowerups().ToArray();

        for (int i = 0; i < powerups.Length; i++)
        {
            var powerup = powerups[i];
            (var xPos, var yPos) = GetScreensPosition(powerup[2] / RealTileSize, powerup[1] / RealTileSize, powerup[3] == 0);

            xPos -= samusX;
            yPos -= samusY;

            if (xPos >= -xDist && xPos <= xDist && yPos >= -yDist && yPos <= yDist)
            {
                tiles[yPos + yDist, xPos + xDist] = true;
            }
        }
    }

    /// <summary>
    /// Returns all active powerups
    /// Bytes for powerup
    /// <code>
    /// byte 0 : Power up type
    /// byte 1 : Y Position
    /// byte 2 : X Position
    /// byte 3 : Current screen
    /// </code>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<byte[]> GetPowerups()
    {
        var addresses = new AddressData[] { Powerups.Powerup1, Powerups.Powerup2 };

        for (int i = 0; i < addresses.Length; i++)
        {
            byte[] powerup = Read(addresses[i]);

            if (powerup[0] == 0x00 || powerup[0] == 0xFF) continue;

            yield return powerup;
        }
    }

    private byte[,] GetTiles(int xDist, int yDist)
    {
        var result = new byte[yDist * 2 + 1, xDist * 2 + 1];
        var roomsTiles = GetLoadedRoomsTiles();

        (var xPos, var yPos) = GetSamusScreensPosition();

        for (int y = -yDist; y <= yDist; y++)
        {
            var tileY = Math.Min(roomsTiles.GetLength(0) - 1, Math.Max(0, y + yPos));

            if (!IsHorizontalRoom())
            {
                tileY = (y + yPos).PosModulo(roomsTiles.GetLength(0));
            }

            for (int x = -xDist; x <= xDist; x++)
            {
                var tileX = Math.Min(roomsTiles.GetLength(1) - 1, Math.Max(0, x + xPos));
                if (IsHorizontalRoom())
                {
                    tileX = (x + xPos).PosModulo(roomsTiles.GetLength(1));
                }

                result[y + yDist, x + xDist] = roomsTiles[tileY, tileX];
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the loaded tiles from the loaded rooms, with their correct positions
    /// </summary>
    /// <returns></returns>
    private byte[,] GetLoadedRoomsTiles()
    {
        var tiles = Read(Room.Tiles);
        //Exclude the final 2 rows of tiles.
        var firstScreen = tiles[0x0..0x3C0].To2DArray(ROOM_HEIGHT, ROOM_WIDTH);
        var secondScreen = tiles[0x400..0x7C0].To2DArray(ROOM_HEIGHT, ROOM_WIDTH);
        if (!pluginConfig.DoublePrecision)
        {
            firstScreen = firstScreen.QuarterArray();
            secondScreen = secondScreen.QuarterArray();
        }

        if (IsOnNameTable3())
        {
            (secondScreen, firstScreen) = (firstScreen, secondScreen);
        }

        (int horizontalCorrection, int verticalCorrection) = IsHorizontalRoom() ? (RealRoomWidth, 0) : (0, RealRoomHeight);

        var result = new byte[RealRoomHeight + verticalCorrection, RealRoomWidth + horizontalCorrection];

        for (int i = 0; i < RealRoomHeight; i++)
        {
            for (int j = 0; j < RealRoomWidth; j++)
            {
                result[i, j] = firstScreen[i, j];
                result[i + verticalCorrection, j + horizontalCorrection] = secondScreen[i, j];
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the position of the values given within the currently loaded screens.
    /// </summary>
    private (int xPos, int yPos) GetScreensPosition(int x, int y, bool isInFirstScreen) => (IsInSecondLoadedScreen(isInFirstScreen), IsHorizontalRoom()) switch
    {
        (true, true) => (x + RealRoomWidth, y),
        (true, false) => (x, y + RealRoomHeight),
        _ => (x, y)
    };

    /// <summary>
    /// Returns whether or not the given position with its screen number is in the first or second screen of tiles.
    /// </summary>
    private bool IsInSecondLoadedScreen(bool isInScreen0) => isInScreen0 == IsOnNameTable3();

    /// <summary>
    /// Reads a single byte from the emulator's memory
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private byte ReadSingle(AddressData addressData) => Read(addressData)[0];

    /// <summary>
    /// Reads up to 8 bytes from the address, assuming little endian.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private ulong ReadULong(AddressData addressData)
    {
        var bytes = Read(addressData);
        ulong value = 0;
        for (int i = 0; i < bytes.Length && i < 8; i++)
        {
            value += (ulong)bytes[i] << i * 8;
        }
        return value;
    }

    /// <summary>
    /// <br>Reads up to 8 bytes from the address, assuming byte-wise little endian, and interprets all nybbles as decimal digits.</br>
    /// <br>Examples:</br>
    /// <code>
    /// 0x563412 -> 123456
    /// 0x90     -> 90    
    /// 0x0180   -> 8001  
    /// 0x4      -> 4     
    /// 0xA      -> 10    
    /// </code>
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private ulong ReadNybbleDigitsToUlong(AddressData addressData)
    {
        var bytes = Read(addressData);
        ulong value = 0;
        for (int i = 0; i < bytes.Length && i < 8; i++)
        {
            var currByte = bytes[i];

            var smallDigit = currByte & 0b0000_1111;
            var bigDigit = (currByte & 0b1111_0000) >> 4;
            value += ((ulong)(smallDigit + bigDigit * 10)) * 100.PosPow(i);
        }

        return value;
    }

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
        var toRead = GetAddressesToRead(addressData, offset, total);
        var result = Read(toRead.ToArray());
        var bytesPerItem = addressData.Length;
        for (long i = 0; i < result.Length; i += bytesPerItem)
        {
            var bytes = new byte[bytesPerItem];
            Array.Copy(result, i, bytes, 0, bytesPerItem);
            yield return bytes;
        }
    }

    /// <summary>
    /// Returns the addresses to read when reading from a RAM Table
    /// </summary>
    /// <param name="addressData"></param>
    /// <param name="offset"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    private static IEnumerable<AddressData> GetAddressesToRead(AddressData addressData, AddressData offset, AddressData total)
    {
        uint count = total.Length / offset.Length;
        for (int i = 0; i < count; i++)
        {
            yield return new AddressData((uint)(addressData.Address + i * offset.Length), addressData.Length, addressData.CacheDuration);
        }
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
            var cacheToUse = GetCacheToUse(address);
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

            var cacheToUse = GetCacheToUse(address);
            if (!cacheToUse.ContainsKey(address.Address))
            {
                cacheToUse[address.Address] = data[dataIndex..(dataIndex + count)];
                dataIndex += count;
            }

            bytes.AddRange(cacheToUse[address.Address]);
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// Which cache to use depending on the AddressData
    /// </summary>
    /// <param name="addressData"></param>
    /// <returns></returns>
    private Dictionary<uint, byte[]> GetCacheToUse(AddressData addressData) => addressData.CacheDuration switch
    {
        AddressData.CacheDurations.Frame => frameCache,
        AddressData.CacheDurations.Room => roomCache,
        _ => frameCache,
    };

    private void InitFrameCache()
    {
        List<AddressData> toRead = new()
        {
            Samus.Health,
            Progress.EnergyTanks,
            Samus.XPosition,
            Samus.YPosition,
            Samus.LookingDirection,
            Samus.Status,
            Samus.InLava,
            Samus.HasMetroidOnHead,
            Samus.IsOnElevator,
            Samus.UsingMissiles,
            Gamestate.FreezeTimer,
            Gamestate.IsMiniBossPresent,
            Gamestate.InADoor,
            Room.PPUCTL0,
            Samus.InvincibleTimer,
            Progress.Missiles,
            Progress.MissileCapacity,
            Samus.CurrentScreen,
            Room.HorizontalOrVertical,
            Room.ScrollX,
            Room.ScrollY,
            Room.LeftDoorFrameOffset,
            new AddressData(Room.LeftDoorFrameOffset.Address + 1),
            new AddressData(Room.LeftDoorFrameOffset.Address + (Room.TilesNamespace3.Address - Room.TilesNamespace0.Address)),
            new AddressData(Room.LeftDoorFrameOffset.Address + (Room.TilesNamespace3.Address - Room.TilesNamespace0.Address) + 1),
            Room.RightDoorFrameOffset,
            new AddressData(Room.RightDoorFrameOffset.Address - 1),
            new AddressData(Room.RightDoorFrameOffset.Address - (Room.TilesNamespace3.Address - Room.TilesNamespace0.Address)),
            new AddressData(Room.RightDoorFrameOffset.Address - (Room.TilesNamespace3.Address - Room.TilesNamespace0.Address) - 1),
            Gamestate.MapX,
            Gamestate.MapY,
            Samus.VerticalSpeed,
            Samus.VerticalFractionalSpeed,
            Samus.HorizontalSpeed,
            Samus.HorizontalFractionalSpeed,
            Sprites.SkreeProjectiles,
            Powerups.Powerup1,
            Powerups.Powerup2,
        };

        toRead.AddRange(GetAddressesToRead(Sprites.BaseSingleSprite, Sprites.BaseSingleSprite, Sprites.AllBaseSprites));

        _ = Read(toRead.ToArray());
    }

    private void VerifyRoomCache()
    {
        //Weird bug going from Bomb room going to the right, need to fix
        delayRoomCacheClearTimer--;
        if (delayRoomCacheClearTimer == 0)
        {
            roomCache.Clear();
        }

        byte scrollX = GetScrollX();
        byte scrollY = GetScrollY();

        var scrollDiff = MathUtils.MaximumAbsoluteDifference(previousScrollX, previousScrollY, scrollX, scrollY);

        if (scrollDiff > 128)
        {
            delayRoomCacheClearTimer = Math.Max(delayRoomCacheClearTimer, 5);
        }

        bool isInDoor = IsSamusInDoor();
        if (!isInDoor && wasSamusInDoor)
        {
            delayRoomCacheClearTimer = Math.Max(delayRoomCacheClearTimer, 10);
        }

        wasSamusInDoor = isInDoor;
        previousScrollX = scrollX;
        previousScrollY = scrollY;
    }

    /// <summary>
    /// Updates Samus' real map position, based the current state
    /// </summary>
    private void UpdateRealMapPosition()
    {
        if (IsSamusInDoor()) return;

        var mapPos = GetMapPosition();
        var (scrollXDirr, scrollYDirr) = GetLastScrollDirection();
        bool samusInSecondScreen = IsInSecondLoadedScreen(IsSamusInFirstScreen());
        bool shouldntFixPosition = false;

        if (GetScrollX() == 0 && scrollXDirr > 0)
        {
            shouldntFixPosition |= true;
        }

        if (IsHorizontalRoom())
        {
            if (IsOnNameTable3())
            {
                shouldntFixPosition |= DoorOnNameTable3(scrollXDirr < 0) || DoorOnNameTable3(scrollXDirr > 0);
            }
            else
            {
                shouldntFixPosition |= DoorOnNameTable0(scrollXDirr < 0) || DoorOnNameTable0(scrollXDirr > 0);
            }
        }

        if (GetScrollY() == 0 && scrollYDirr > 0)
        {
            shouldntFixPosition |= true;
        }

        if (!shouldntFixPosition)
        {
            if (samusInSecondScreen && (scrollXDirr < 0 || scrollYDirr < 0) ||
                !samusInSecondScreen && (scrollXDirr > 0 || scrollYDirr > 0))
            {
                mapPos = ((byte)(mapPos.x - scrollXDirr), (byte)(mapPos.y - scrollYDirr));
            }
        }

        currentMapPosition = mapPos;
    }

    private static IEnumerable<AddressData> GetCalculatedAddresses(uint totalLength, uint offset, params AddressData[] baseAddresses)
    {
        for (uint i = 0; i < totalLength; i += offset)
        {
            for (int j = 0; j < baseAddresses.Length; j++)
            {
                yield return new AddressData()
                {
                    Address = baseAddresses[j].Address + i,
                    CacheDuration = baseAddresses[j].CacheDuration,
                    Length = baseAddresses[j].Length
                };
            }
        }
    }
}
