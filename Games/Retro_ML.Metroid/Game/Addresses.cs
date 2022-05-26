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
            /// Stores the layout of tiles for 2 screens (0x400 bytes each)
            /// </summary>
            public static readonly AddressData ScreenTiles = new(0x6000, 0x800);
            /// <summary>
            /// X position of camera scroll
            /// </summary>
            public static readonly AddressData ScrollX = new(0xFC);
            /// <summary>
            /// Y position of camera scroll
            /// </summary>
            public static readonly AddressData ScrollY = new(0xFD);

            /// <summary>
            /// Current Y position on the map
            /// </summary>
            public static readonly AddressData MapY = new(0x4F);
            /// <summary>
            /// Current X position on the map
            /// </summary>
            public static readonly AddressData MapX = new(0x50);
            /// <summary>
            /// <br>0x10 : Brinstar</br>
            /// <br>0x11 : Norfair</br>
            /// <br>0x12 : Kraid</br>
            /// <br>0x13 : Tourian</br>
            /// <br>0x14 : Ridley</br>
            /// </summary>
            public static readonly AddressData CurrentLevel = new(0x74);
            /// <summary>
            /// <br>0 = not in a door                                         </br>
            /// <br>1 = in a right-side door                                  </br>
            /// <br>2 = in a left-side door                                   </br>
            /// <br>3 = scrolling up to center a door before room transition  </br>
            /// <br>4 = scrolling down to center a door before room transition</br>
            /// </summary>
            public static readonly AddressData InADoor = new(0x56);
        }

        public static class Game
        {
            /// <summary>
            /// <br>Current game mode                   </br>
            /// <br>3: playing                          </br>
            /// <br>5: paused                           </br>
            /// <br>TODO : Figure out values for dying  </br>
            /// </summary>
            public static readonly AddressData Mode = new(0x1E);
        }

        public static class Progress
        {
            /// <summary>
            /// Amount of energy tanks
            /// </summary>
            public static readonly AddressData EnergyTanks = new(0x6877);
            /// <summary>
            /// Equipment so far (each bit refers to a specific upgrade)
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

        public static class Enemies
        {
            /// <summary>
            /// Base data for all the enemies.
            /// </summary>
            public static readonly AddressData AllBaseEnemies = new(0x400, 0x100);
            /// <summary>
            /// Base data for a single enemy
            /// </summary>
            public static readonly AddressData SingleEnemy = new(0x400, 0x10);
            /// <summary>
            /// Enemy Y position within room
            /// </summary>
            public static readonly AddressData EnemyPosY = new(0x400);
            /// <summary>
            /// Enemy X position within room
            /// </summary>
            public static readonly AddressData EnemyPosX = new(0x401);
            /// <summary>
            /// Enemy's current hitpoints
            /// </summary>
            public static readonly AddressData EnemyHitpoints = new(0x40B);
        }

        public static class Samus
        {
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
            /// Signed byte for Samus' horizontal speed
            /// </summary>
            public static readonly AddressData HorizontalSpeed = new(0x309);
            /// <summary>
            /// Set when Samus is hit by an enemy
            /// </summary>
            public static readonly AddressData WasHit = new(0x30A);
            /// <summary>
            /// Screen Samus is currently in. (0 or 1)
            /// </summary>
            public static readonly AddressData Screen = new(0x30C);
            /// <summary>
            /// Samus Y position within room
            /// </summary>
            public static readonly AddressData YPosition = new(0x30D);
            /// <summary>
            /// Samus X position within room
            /// </summary>
            public static readonly AddressData XPosition = new(0x30E);
            /// <summary>
            /// Samus' maximum horizontal speed
            /// </summary>
            public static readonly AddressData MaxSpeed = new(0x316);
            /// <summary>
            /// Set to 1 when Samus is using missiles
            /// </summary>
            public static readonly AddressData UsingMissiles = new(0x10E);
        }

        public static class Elevator
        {
            /// <summary>
            /// Current status of the elevator. 
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
    }
}
