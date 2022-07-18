namespace Retro_ML.SuperBomberman3.Game
{
    /// <summary>
    /// RAM addresses used in Super Bomberman 3.
    /// 
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheTypes
            {
                Frame,
            }

            public AddressData(uint address, uint length, CacheTypes cacheTypes = CacheTypes.Frame)
            {
                Address = address;
                Length = length;
                CacheType = cacheTypes;
            }

            public uint Address;
            public uint Length;
            public CacheTypes CacheType;
        }

        public static class GameAddresses
        {
            /// <summary>
            /// Tile Map. 1 byte per tile, 176 tiles total
            /// <code>
            /// 0x84 = undestructible (sudden death)
            /// 0x80 = undestructible
            /// 0x50 = bomb
            /// 0x30 = destructible
            /// 0x10 = powerup
            /// 0x00 = free
            /// 0x05 = explosion
            /// 0x07 = explosion
            /// 0x24 = explosion
            /// <code>
            /// </summary>
            public static readonly AddressData Tiles = new(0xDF2, 176);
        }

        public static class PlayersAddresses
        {
            /// <summary>
            /// Players X pos in pixels. There can be 5 players in one game.
            /// </summary>
            public static readonly AddressData XPos = new(0x14AF, 5);
            /// <summary>
            /// Players Y pos in pixels. There can be 5 players in one game.
            /// </summary>
            public static readonly AddressData YPos = new(0x14F7, 5);
            /// <summary>
            /// Players AFK timer, vary from 0 to 255. Only incerement when the player does not move.
            /// When reaching 255, player starts dancing. Useful for the StoppedMoving ScoreFactor.
            /// </summary>
            public static readonly AddressData AFKTimer = new(0x1467, 5);
        }

        public static class PowerupsAddresses
        {
            /// <summary>
            /// If the bomberman is on a Louie, the value will be equal to 0, when the player is on foot, the value = 0x80
            /// </summary>
            public static readonly AddressData Louie = new(0x1244, 1);
            /// <summary>
            /// Increases the range of explosions when bombs are detonated by one level. (Maximum ten)
            /// </summary>
            public static readonly AddressData ExplosionExpander = new(0x144F, 1);
            /// <summary>
            /// Increases the maximum number of bombs that can be laid on the ground by one. (Maximum ten)
            /// </summary>
            public static readonly AddressData ExtraBomb = new(0x48B9, 1);
            /// <summary>
            /// Increases the speed at which Bomberman moves by one level. (Decreased by one level each time a life is lost)
            /// </summary>
            public static readonly AddressData Accelerator = new(0x1257, 1);
            /// <summary>
            /// Allows bombs to be detonated by remote control by pressing B Button.
            /// </summary>
            public static readonly AddressData RemoteControl = new(0x1257, 1);
            /// <summary>
            /// Allows the ability to kick bombs by using Dpad. Pressing X Button or A Button stops the kicked bomb.
            /// </summary>
            public static readonly AddressData Kick = new(0x48D1, 1);
            /// <summary>
            /// Allows the ability to punch bombs and send them flying with Y Button or L Button.
            /// </summary>
            public static readonly AddressData BoxingGlove = new(0x1257, 1);
            /// <summary>
            /// Power Bombs will explode with a maximum Fire range
            /// </summary>
            public static readonly AddressData PowerBomb = new(0x1237, 1);
            /// <summary>
            /// It gives the player the ability to pick up, carry, and then throw bombs.
            /// </summary>
            public static readonly AddressData PowerGloves = new(0x1437, 1);
        }
    }
}
