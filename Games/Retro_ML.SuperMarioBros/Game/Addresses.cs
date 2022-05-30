namespace Retro_ML.SuperMarioBros.Game
{
    /// <summary>
    /// RAM addresses used in Super Mario Bros.
    /// 
    /// <br>HUGE thanks to <see href="https://datacrystal.romhacking.net/wiki/Super_Mario_Bros.:RAM_map;game=smw">DataCrystal</see> for making those values available</br>
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheTypes
            {
                Frame,
                Level,
                TilesCache
            }

            public AddressData(uint address, uint length, CacheTypes cacheTypes = CacheTypes.Frame, uint highByteLocation = 0)
            {
                Address = address;
                Length = length;
                CacheType = cacheTypes;
                HighByteAddress = highByteLocation;
            }

            public uint Address;
            public uint Length;
            public CacheTypes CacheType;
            public uint HighByteAddress;
        }

        public static class GameState
        {
            /// <summary>
            /// Number of lives available
            /// </summary>
            public static readonly AddressData Lives = new(0x075A, 1);
            /// <summary>
            /// Number of coins earned
            /// 0x00 - No
            /// 0x2E - Yes
            /// </summary>
            public static readonly AddressData Coins = new(0x075E, 1);
            /// <summary>
            /// Current Mario score (1000000 100000 10000 1000 100 10)
            /// </summary>
            public static readonly AddressData Score = new(0x07DD, 6);
            /// <summary>
            /// Level
            /// </summary>
            public static readonly AddressData Level = new(0x0760, 1, AddressData.CacheTypes.Level);
            /// <summary>
            /// Current Mario score
            /// </summary>
            public static readonly AddressData World = new(0x075F, 1, AddressData.CacheTypes.Level);
            /// <summary>
            /// Current game Timer (0100 0000 0000)
            /// </summary>
            public static readonly AddressData GameTimer = new(0x07F8, 3);
            /// <summary>
            /// Type of levels
            /// 00 - Normal
            /// 01 - Underwater
            /// 02 - Night
            /// 03 - Underground
            /// 04 - Castle
            /// </summary>
            public static readonly AddressData LevelType = new(0x0773, 1, AddressData.CacheTypes.Level);
            /// <summary>
            /// Screen Number. Increment by 1 for every 255 pixel traveled on the right by mario.
            /// </summary>
            public static readonly AddressData CurrentScreen = new(0x6D, 1);
            /// <summary>
            /// Index of page position
            /// </summary>
            public static readonly AddressData PagePosition = new(0x0725, 1);
            /// <summary>
            /// ColumnPosition in page
            /// </summary>
            public static readonly AddressData ColumnPosition = new(0x0726, 1);
            /// <summary>
            /// Curently loaded tiles
            /// </summary>
            public static readonly AddressData Tiles = new(0x0500, 0x1A0, AddressData.CacheTypes.TilesCache);
            /// <summary>
            /// == 2 if level won via axe
            /// </summary>
            public static readonly AddressData WonCondition = new(0x770, 1);
        }

        public static class Player
        {
            /// <summary>
            /// Mario's state. Useful to know if he's in an incapacitated state in which he cannot act
            /// 0x00 - Leftmost of screen
            /// 0x01 - Climbing vine
            /// 0x02 - Entering reversed-L pipe
            /// 0x03 - Going down a pipe
            /// 0x04 - Autowalk
            /// 0x05 - Autowalk
            /// 0x06 - Player dies
            /// 0x07 - Entering area
            /// 0x08 - Normal
            /// 0x09 - Transforming from Small to Large(cannot move)
            /// 0x0A - Transforming from Large to Small(cannot move)
            /// 0x0B - Dying
            /// 0x0C - Transforming to Fire Mario(cannot move)
            /// </summary>
            public static readonly AddressData MarioActionState = new(0x000E, 1);
            /// <summary>
            /// Mario state when he is "Floating".
            /// 0x00 - Standing on solid/else
            /// 0x01 - Airborn by jumping
            /// 0x02 - Airborn by walking of a ledge
            /// 0x03 - Sliding down flagpole
            /// </summary>
            public static readonly AddressData MarioState = new(0x001D, 1);
            /// <summary>
            /// Horizontal position of the Mario, in pixels. Value will fluctuate between 0 and 255 and reset for every new Screen? 
            /// To get the real horizontal position through the entire level, multiply CurrentScreen*255(0x100) + PlayerPositionX
            /// </summary>
            public static readonly AddressData MarioPositionX = new(0x0086, 1);
            /// <summary>
            /// Vertical position of the Mario, in pixels. Same as the mario's position in the screen for now (will need to review)
            /// </summary>
            public static readonly AddressData MarioPositionY = new(0x03B8, 1);
            /// <summary>
            /// Horizontal position of Mario in the screen, in pixels. Value cap at 114 when Mario is on the middle of the screen, the level
            /// move to the right with mario when he's in the middle of the screen and he keeps going right.
            /// </summary>
            public static readonly AddressData MarioScreenPositionX = new(0x03AD, 1);
            /// <summary>
            /// Vertical position of Mario in the screen, in pixels.
            /// </summary>
            public static readonly AddressData MarioScreenPositionY = new(0x03B8, 1);
            /// <summary>
            ///Mario X-Speed Absolute,mario speed in either direction (0 - 0x28)
            /// </summary>
            public static readonly AddressData MarioMaxVelocity = new(0x0700, 1);
            /// <summary>
            ///Swimming flag set to 0 if player is swimming
            /// </summary>
            public static readonly AddressData IsSwimming = new(0x0704, 1);
            /// <summary>
            ///Invicibility timer when mario get hits by an enemy
            /// </summary>
            public static readonly AddressData FlashingTimer = new(0x079E, 1);
            /// <summary>
            ///Mario powerup state
            ///0 - Small
            ///1 - Big
            ///>2 - fiery
            /// </summary>
            public static readonly AddressData MarioPowerupState = new(0x0756, 1);
            /// <summary>
            /// Boolean, true if falling to death
            /// </summary>
            public static readonly AddressData IsFalling = new(0x712, 1);
        }

        public static class Sprite
        {
            /// <summary>
            /// Enemy type when enemy is present
            /// 0x00 - Green Koopa
            /// 0x01 - Red Koopa
            /// 0x02 - Buzzy beetle
            /// 0x03 - Red Koopa
            /// 0x04 - Green Koopa
            /// 0x05 - Hammer brother
            /// 0x06 - Goomba
            /// 0x07 - Blooper
            /// 0x08 - BulletBill FrenzyVar
            /// 0x09 - Green Koopa paratroopa
            /// 0x0A - Grey CheepCheep
            /// 0x0B - Red CheepCheep
            /// 0x0C - Pobodoo
            /// 0x0D - Piranha Plant
            /// 0x0E - Green Paratroopa Jump
            /// 0x0F - Crashes game(status bar margin)
            /// 0x10 - Bowser's flame
            /// 0x11 - Lakitu
            /// 0x12 - Spiny Egg
            /// 0x13 - Nothing
            /// 0x14 - Fly CheepCheep
            /// 0x15 - Bowser's Flame
            /// 0x16 - Fireworks
            /// 0x17 - BulletBill Frenzy
            /// 0x18 - Stop Frenzy
            /// 0x19 - ?
            /// 0x20/0x21/0x22 - Firebar
            /// 0x21 - Long Firebar(castle) AND sets previous enemy slot to 0x20 or else only half of the line shows
            /// 0x23 - ?
            /// 0x24/0x25 - Static lift
            /// 0x26/0x27 - Vertical going lift
            /// 0x28 - Horizontal going lift
            /// 0x29 - Static lift(Will Fall if Player stays on it for too long)
            /// 0x2A - Horizontal forward moving lift with strange hitbox
            /// 0x2B/0x2C - Halves of double lift(like 1.2)
            /// 0x2D - Bowser(special), will try to set previous addr to 2D as well, if unable only his ass shows :) He also tries to reach a certain height, if not there already, before starting his routine.
            /// 0x2E - PowerUp Object
            /// 0x2F - Vine Object
            /// 0x30 - Flagpole Flag Object
            /// 0x31 - StarFlag Object
            /// 0x32 - Jump spring Object
            /// 0x33 - BulletBill CannonVar
            /// 0x34 - Warpzone
            /// 0x35 - Retainer Object
            /// 0x36 - Crash
            /// 0x37 - 2 Little Goomba.
            /// 0x38 - 3 Little Goomba.
            /// 0x3A - Skewed goomba.
            /// 0x3B - 2 Koopa Troopa.
            /// 0x3C - 3 Koopa Troopa.
            /// </summary>
            public static readonly AddressData EnemyType = new(0x0016, 5);
            /// <summary>
            /// Is there enemies drawn? (max 5)
            /// 0 - No
            /// 1 - Yes
            /// </summary>
            public static readonly AddressData IsEnemyUpPresent = new(0x000F, 5);
            /// <summary>
            /// Enemy hitboxes (5x4 bytes, x1,y1 x2,y2)
            /// </summary>
            public static readonly AddressData EnemyPositions = new(0x04B0, 20);
            /// <summary>
            /// Is there a powerup on the screen?
            /// 0x00 - No
            /// 0x2E - Yes
            /// </summary>
            public static readonly AddressData IsPowerUpPresent = new(0x001B, 1);
            /// <summary>
            /// Powerup hitboxes (4 bytes, x1,y1 x2,y2)
            /// </summary>
            public static readonly AddressData PowerUpPositions = new(0x04C4, 4);
        }
    }
}
