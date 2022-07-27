namespace Retro_ML.SuperBomberman3.Game
{
    /// <summary>
    /// RAM addresses used in Super Bomberman 3.
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public AddressData(uint address, uint length)
            {
                Address = address;
                Length = length;
            }

            public uint Address;
            public uint Length;
        }

        /// <summary>
        /// Addresses related to the game and / or round itself.
        /// </summary>
        public static class GameAddresses
        {
            /// <summary>
            /// Dynamic Tile Map. 1 byte per tile, 176 tiles total.
            /// <code>
            /// 0x84 = undestructible (sudden death)
            /// 0x80 = undestructible
            /// 0x50 = bomb (danger)
            /// 0x30 = destructible
            /// 0x10 = powerup (goodie)
            /// 0x00 = free
            /// 0x04 = big snowball (danger)
            /// 0x05 = explosion (danger)
            /// 0x07 = explosion (danger)
            /// 0x24 = explosion (danger)
            /// </code>
            /// </summary>
            public static readonly AddressData DynamicTiles = new(0xDF2, 176);
            /// <summary>
            /// Represent bomb positions. 
            /// In theory, there could be a max of 60 bombs if there is 5 players (? at least that's what it looks like in memory and its really hard to test), but we should only need around 20 max in the worst-case scenario.
            /// Bomb positions vary from 17 to 189.
            /// </summary>
            public static readonly AddressData BombsPositions = new(0x16EF, 20);
            /// <summary>
            /// Represent the countdown remaining for a bomb explosion. Starts at 0x95 (149)
            /// In theory, there could be a max of 60 bombs if there is 5 players (? at least that's what it looks like in memory and its really hard to test), but we should only need around 20 max in the worst-case scenario.
            /// </summary>
            public static readonly AddressData BombsTimers = new(0x16B3, 20);
            /// <summary>
            /// Represent the number of minutes on the timer.
            /// </summary>
            public static readonly AddressData GameMinutesTimer = new(0x17E4, 1);
            /// <summary>
            /// Represent the number of seconds on the timer.
            /// </summary>
            public static readonly AddressData GameSecondsTimer = new(0x17E3, 1);
            /// <summary>
            /// Represent the number of destructible tiles left on the map.
            /// </summary>
            public static readonly AddressData DestructibleTilesRemaining = new(0x17E5, 1);
        }

        /// <summary>
        /// Addresses related to the players.
        /// </summary>
        public static class PlayersAddresses
        {
            /// <summary>
            /// Players X pos in pixels. There can be 5 players in one game.
            /// </summary>
            public static readonly AddressData PlayersXPos = new(0x14AF, 5);
            /// <summary>
            /// Players Y pos in pixels. There can be 5 players in one game.
            /// </summary>
            public static readonly AddressData PlayersYPos = new(0x14F7, 5);
            /// <summary>
            /// Number of bombs planted by the players.
            /// </summary>
            public static readonly AddressData PlayersBombsPlantedCount = new(0x13D7, 5);
            /// <summary>
            /// Theses addresses are used as a sort of timer. The timer looks like its used to 3 purposes :
            /// <code>
            /// 1 - When a player dies, a timer is started that represent the time left before his items are dropped on the ground.
            /// 2 - When a player grab an egg, a timer is started associated with the mounting animation.
            /// 3 - When a player gets damaged on a Louie, a timer is started associated with the dismounting animation.
            /// </code>
            /// Since we havent found a better flag / way to know when a player dies, we will use these addresses as a way to find when player dies.
            /// When a player dies, the value starts at 60 (59 for some bomberman types) and goes down to 0.
            /// </summary>
            public static readonly AddressData PlayersDeathTimer = new(0x12CF, 5);
        }

        /// <summary>
        /// Addresses related to the different powerups and upgrades available in battle mode.
        /// </summary>
        public static class PowerupsAddresses
        {
            /// <summary>
            /// If the bomberman is on a Louie, the value will be equal to 0, when the player is on foot, the value will be equal to 0x80.
            /// </summary>
            public static readonly AddressData IsOnLouie = new(0x1244, 5);
            /// <summary>
            /// Theses addresses represent the colour of the Louie mounted by the main player only.
            /// Each colour of the Louie has a different ability that can use used by pressing the Y button on the control pad.
            /// Colours are represented on 4 bytes.
            /// <code>
            /// 0x35F02DD = Yellow Louie: - He can kick the soft blocks in a straight line.
            /// 0x1BE00AC = Green Louie: - He has a super speed burst ability.
            /// 0x7E805940 = Blue Louie: - He can kick bombs over the blocks.
            /// 0x35B2210D = Brown Louie: - He will lay a line of bombs equivalent of the number of bombs the player has.
            /// 0x7DFF695A = Pink Louie: - He can jump over one block.
            /// </code>
            /// </summary>
            public static readonly AddressData MountedLouieColours = new(0x31BC, 4);
            /// <summary>
            /// Represent the explosion level for every players.
            /// Increases the range of explosions when bombs are detonated by one level. Value represent the range in tile.
            /// Value starts at 2 and reach a maximum of 8.
            /// </summary>
            public static readonly AddressData ExplosionExpander = new(0x144F, 5);
            /// <summary>
            /// Represent the numbers of extra bombs for every players.
            /// Increases the maximum number of bombs that can be laid on the ground by one.
            /// Value starts at 1 and reach a maximum of 9.
            /// </summary>
            public static readonly AddressData ExtraBomb = new(0x48B9, 5);
            /// <summary>
            /// Represent the extra speed boost for every players.
            /// Increases the speed at which Bomberman moves by one level. (Decreased by one level each time a life is lost).
            /// </summary>
            public static readonly AddressData Accelerator = new(0x1257, 5);
            /// <summary>
            /// Represent every upgrade each player has. The upgrades are additives which means that a value of 0x02 = Kick, 0x04 = Gloves and 0x06 = Kick + Glove.
            /// A bomberman cannot hold both sticky and power bomb upgrade at the same time.
            /// <code>
            /// 0x02 = Kick: - Allows the ability to kick bombs by using Dpad. Pressing X Button or A Button stops the kicked bomb.
            /// 0x04 = Glove: - It gives the player the ability to pick up, carry, and then throw bombs.
            /// 0x20 = Slime Bomb: - Slime bomb will bounce on walls until they explode
            /// 0x40 = Power Bomb: - Allow the 1st bomb planted to be a Power Bombs. Power Bombs will explode with the maximum range of explosion possible (8 tiles)
            /// </code>
            /// </summary>
            public static readonly AddressData BombermanUpgrade = new(0x48D1, 5);
        }

        /// <summary>
        /// Some of the address discovered were not used for the scope of my implementation. I still decided to keep them here for future uses if need be.
        /// </summary>
        public static class UnusedAddresses
        {
            /// <summary>
            /// Players idle timer, vary from 0 to 255. Only incerement when the player does not move.
            /// When reaching 255, player starts dancing.
            /// </summary>
            public static readonly AddressData PlayersIdleTimer = new(0x1467, 5);
            /// <summary>
            /// Represent the cart position in level 8. 
            /// </summary>
            public static readonly AddressData Level8CartPosition = new(0x1695, 1);
            /// <summary>
            /// Static Tile Map. 1 byte per tile, 176 tiles total. The tiles are static and represent what the map should look like when loaded.
            /// I havent used it, but some level contains sprites that cant be seen using the dynamic tile map like teleporters and other dangers.
            /// </summary>
            public static readonly AddressData StaticTiles = new(0x2710, 176);
        }
    }
}
