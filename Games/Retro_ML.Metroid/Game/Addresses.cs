namespace Retro_ML.Metroid.Game
{
    /// <summary>
    /// <br>RAM addresses used in Metroid.</br>
    /// <br/>
    /// <br>Big thanks to </br>
    /// <br>- DataCrystal Wiki <see href="https://datacrystal.romhacking.net/wiki/Metroid:RAM_map"/></br>
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheDurations
            {
                Frame,
                Room
            }

            public AddressData(uint address, uint length = 1, CacheDurations cacheDuration = CacheDurations.Frame)
            {
                Address = address;
                Length = length;
                CacheDuration = cacheDuration;
            }

            public uint Address;
            public uint Length;
            public CacheDurations CacheDuration;
        }

        public static class Room
        {
            /// <summary>
            /// The currently loaded tiles, split into two rooms of 0x400 tiles.
            /// The final 2 rows of tiles appear to be useless
            /// </summary>
            public static readonly AddressData Tiles = new(0x6000, 0x800, AddressData.CacheDurations.Room);
            /// <summary>
            /// X position of camera scroll
            /// </summary>
            public static readonly AddressData ScrollX = new(0xFD);
            /// <summary>
            /// Y position of camera scroll
            /// </summary>
            public static readonly AddressData ScrollY = new(0xFC);
            /// <summary>
            /// Bit 0000_X000 is set for a vertical room, unset for a horizontal room
            /// </summary>
            public static readonly AddressData HorizontalOrVertical = new(0xFA);
        }

        public static class Gamestate
        {
            /// <summary>
            /// <br>Current game mode                   </br>
            /// <code>
            /// 3: playing                          
            /// 5: paused
            /// </code>
            /// <br>TODO : Figure out values for dying  </br>
            /// </summary>
            public static readonly AddressData Mode = new(0x1E);
            /// <summary>
            /// Current Y position on the map
            /// </summary>
            public static readonly AddressData MapY = new(0x4F);
            /// <summary>
            /// Current X position on the map
            /// </summary>
            public static readonly AddressData MapX = new(0x50);
            /// <summary>
            /// <code>
            /// 0x10 : Brinstar
            /// 0x11 : Norfair
            /// 0x12 : Kraid
            /// 0x13 : Tourian
            /// 0x14 : Ridley
            /// </code>
            /// </summary>
            public static readonly AddressData CurrentLevel = new(0x74);
            /// <summary>
            /// <code>
            /// 0 = not in a door                                         
            /// 1 = in a right-side door                                  
            /// 2 = in a left-side door                                   
            /// 3 = scrolling up to center a door before room transition  
            /// 4 = scrolling down to center a door before room transition
            /// </code>
            /// </summary>
            public static readonly AddressData InADoor = new(0x56);
        }

        public static class Progress
        {
            /// <summary>
            /// Amount of energy tanks
            /// </summary>
            public static readonly AddressData EnergyTanks = new(0x6877);
            /// <summary>
            /// Equipment so far
            /// <code>
            /// bit 0: Bombs
            /// bit 1: High Jump
            /// bit 2: Long Beam
            /// bit 3: Screw Attack
            /// bit 4: Maru Mari(Morph Ball)
            /// bit 5: Varia Suit
            /// bit 6: Wave Beam
            /// bit 7: Ice Beam
            /// </code>
            /// </summary>
            public static readonly AddressData Equipment = new(0x6878);
            /// <summary>
            /// Current missiles
            /// </summary>
            public static readonly AddressData Missiles = new(0x6879);
            /// <summary>
            /// Maximum missiles Samus has
            /// </summary>
            public static readonly AddressData MissileCapacity = new(0x687A);
            /// <summary>
            /// Amount of time Samus has died.
            /// </summary>
            public static readonly AddressData Deaths = new(0x6881, 2);
        }

        public static class Sprites
        {
            /// <summary>
            /// Base data for all the sprites.
            /// </summary>
            public static readonly AddressData AllBaseSprites = new(0x400, 0x100);
            /// <summary>
            /// Extra data for all the sprites
            /// </summary>
            public static readonly AddressData AllExtraSprites = new(0x6AF4, 0x100);
            /// <summary>
            /// Base data for a single sprite
            /// </summary>
            public static readonly AddressData BaseSingleSprite = new(0x400, 0x10);
            /// <summary>
            /// Extra data for a single sprite
            /// </summary>
            public static readonly AddressData ExtraSingleSprite = new(0x6AF4, 0x10);
            /// <summary>
            /// Sprite Y position within room
            /// </summary>
            public static readonly AddressData PosY = new(0x400);
            /// <summary>
            /// Sprite X position within room
            /// </summary>
            public static readonly AddressData PosX = new(0x401);
            /// <summary>
            /// Sprite's current hitpoints
            /// </summary>
            public static readonly AddressData Hitpoints = new(0x40B);
            /// <summary>
            /// Timer used by pickups before despawning
            /// </summary>
            public static readonly AddressData DespawnTimer = new(0x40D);
            /// <summary>
            /// <br>Current status of the enemy</br>
            /// <code>
            /// 0x00 : Not in use
            /// 0x04 : Frozen
            /// </code>
            /// </summary>
            public static readonly AddressData Status = new(0x6AF4);
            /// <summary>
            /// Current name table of the enemy (The screen it is currently on)
            /// </summary>
            public static readonly AddressData NameTable = new(0x6AFB);

            /// <summary>
            /// The projectiles from a Skree explosion, 4 bytes each.
            /// <code>
            /// byte 0 : Despawn timer
            /// byte 1 : Y Position
            /// byte 2 : X Position
            /// byte 3 : Current screen (0 or 1)
            /// </code>
            /// </summary>
            public static readonly AddressData SkreeProjectiles = new(0xA0, 0x10);
        }

        public static class Samus
        {
            /// <summary>
            /// Current status of Samus : 
            /// <code>
            /// 0x00 : Normal
            /// 0x01 : Moving
            /// 0x02 : Jumping
            /// 0x03 : Morph Ball
            /// 0x04 : Aiming Up
            /// 0x05 : ???
            /// 0x06 : Jumping while aiming up
            /// </code>
            /// </summary>
            public static readonly AddressData Status = new(0x300);
            /// <summary>
            /// <br>Samus' current health</br>
            /// <br>Stored as 0xCDA.B, where ABCD are decimal digits</br>
            /// </summary>
            public static readonly AddressData Health = new(0x106, 2);
            /// <summary>
            /// 0 when looking right, 1 when looking left.
            /// </summary>
            public static readonly AddressData LookingDirection = new(0x4D);
            /// <summary>
            /// Set to 1 when Samus is in lava
            /// </summary>
            public static readonly AddressData InLava = new(0x64);
            /// <summary>
            /// Set to 1 when a Metroid is on Samus
            /// </summary>
            public static readonly AddressData HasMetroidOnHead = new(0x92);
            /// <summary>
            /// Samus is invincible from having taken a hit when set to a non-zero value.
            /// </summary>
            public static readonly AddressData InvincibleTimer = new(0x70);
            /// <summary>
            /// 1 if Samus is on an elevator, 0 otherwise
            /// </summary>
            public static readonly AddressData IsOnElevator = new(0x307);
            /// <summary>
            /// Signed byte for Samus' vertical speed
            /// </summary>
            public static readonly AddressData VerticalSpeed = new(0x308);
            /// <summary>
            /// The fractional part of Samus' horizontal speed
            /// </summary>
            public static readonly AddressData VerticalFractionalSpeed = new(0x312);
            /// <summary>
            /// Signed byte for Samus' horizontal speed
            /// </summary>
            public static readonly AddressData HorizontalSpeed = new(0x309);
            /// <summary>
            /// The fractional part of Samus' horizontal speed
            /// </summary>
            public static readonly AddressData HorizontalFractionalSpeed = new(0x313);
            /// <summary>
            /// Set when Samus is hit by an enemy
            /// </summary>
            public static readonly AddressData WasHit = new(0x30A);
            /// <summary>
            /// Screen Samus is currently in. (0 or 1)
            /// </summary>
            public static readonly AddressData CurrentScreen = new(0x30C);
            /// <summary>
            /// Samus Y position within room
            /// </summary>
            public static readonly AddressData YPosition = new(0x30D);
            /// <summary>
            /// Samus X position within room
            /// </summary>
            public static readonly AddressData XPosition = new(0x30E);
            /// <summary>
            /// Set to 1 when Samus is using missiles
            /// </summary>
            public static readonly AddressData UsingMissiles = new(0x10E);
        }

        public static class Elevator
        {
            /// <summary>
            /// Current status of the elevator. 0 if inactive
            /// </summary>
            public static readonly AddressData Status = new(0x320);
            /// <summary>
            /// Elevator vertical radius
            /// 
            /// TODO : figure out what "radius" means, might be height / 2
            /// </summary>
            public static readonly AddressData VerticalRadius = new(0x321);
            /// <summary>
            /// Elevator horizontal radius
            /// 
            /// TODO : figure out what "radius" means, might be width / 2
            /// </summary>
            public static readonly AddressData HorizontalRadius = new(0x322);
            /// <summary>
            /// Signed byte for the elevator's vertical speed
            /// </summary>
            public static readonly AddressData VerticalSpeed = new(0x328);
            /// <summary>
            /// Set to 1 when the elevator is active
            /// </summary>
            public static readonly AddressData OnScreen = new(0x32B);
            /// <summary>
            /// Elevator Y position within room
            /// </summary>
            public static readonly AddressData YPosition = new(0x32D);
            /// <summary>
            /// Elevator X position within room
            /// </summary>
            public static readonly AddressData XPosition = new(0x32E);
        }

        public static class Powerups
        {
            /// <summary>
            /// Bytes for powerup 1
            /// <code>
            /// byte 0 : Power up type
            /// byte 1 : Y Position
            /// byte 2 : X Position
            /// byte 3 : Current screen
            /// </code>
            /// </summary>
            public static readonly AddressData Powerup1 = new(0x748, 4);
            /// <summary>
            /// Bytes for powerup 2
            /// <code>
            /// byte 0 : Power up type
            /// byte 1 : Y Position
            /// byte 2 : X Position
            /// byte 3 : Current screen
            /// </code>
            /// </summary>
            public static readonly AddressData Powerup2 = new(0x748, 4);
        }
    }
}
