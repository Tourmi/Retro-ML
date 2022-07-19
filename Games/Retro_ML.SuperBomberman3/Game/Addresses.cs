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
            /// <summary>
            /// Represent bomb positions. 
            /// In theory, there could be a max of 60 bombs if there is 5 players (? at least that's what it looks like in memory), but we should only need around 20 max in the worst-case scenario
            /// Bomb positions vary from 17 to 189.
            /// </summary>
            public static readonly AddressData BombsPosition = new(0x16EF, 20);
            /// <summary>
            /// Represent the countdown remaining for a bomb explosion. Starts at 0x95 (149)
            /// In theory, there could be a max of 60 bombs if there is 5 players (? at least that's what it looks like in memory), but we should only need around 20 max in the worst-case scenario
            /// </summary>
            public static readonly AddressData BombsTimer = new(0x16B3, 20);
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
            public static readonly AddressData AFKTimer = new(0x1467, 1);
        }

        public static class PowerupsAddresses
        {
            /// <summary>
            /// If the bomberman is on a Louie, the value will be equal to 0, when the player is on foot, the value = 0x80
            /// </summary>
            public static readonly AddressData Louie = new(0x1244, 1);
            /// <summary>
            /// Each colour of the Louie has a different ability that can use used by pressing the Y button on the control pad.
            /// Colours are represented on 4 bytes.
            /// /// <code>
            /// 0x35F02DD = Yellow Louie: - He can kick the soft blocks in a straight line.
            /// 0x1BE00AC = Green Louie: - He has a super speed burst ability.
            /// 0x7E805940 = Blue Louie: - He can kick bombs over the blocks.
            /// 0x35B2210D = Brown Louie: - He will lay a line of bombs equivalent of the number of bombs the player has.
            /// 0x7DFF695A = Pink Louie: - He can jump over one block.
            /// <code>
            /// </summary>
            public static readonly AddressData LouieColours = new(0x31BC, 4);
            /// <summary>
            /// Increases the range of explosions when bombs are detonated by one level. Value represent the range in tile.
            /// Value starts at 2 and reach a maximum of 8
            /// </summary>
            public static readonly AddressData ExplosionExpander = new(0x144F, 1);
            /// <summary>
            /// Increases the maximum number of bombs that can be laid on the ground by one.
            /// Value starts at 1 and reach a maximum of 9
            /// </summary>
            public static readonly AddressData ExtraBomb = new(0x48B9, 1);
            /// <summary>
            /// Increases the speed at which Bomberman moves by one level. (Decreased by one level each time a life is lost)
            /// </summary>
            public static readonly AddressData Accelerator = new(0x1257, 1);
            /// <summary>
            /// Represent every upgrade the player has. The upgrade are additive which means that a value of 0x06 = Kick + Glove.
            /// A bomberman cannot hold both sticky and power bomb upgrade at the same time.
            /// <code>
            /// 0x02 = Kick: - Allows the ability to kick bombs by using Dpad. Pressing X Button or A Button stops the kicked bomb.
            /// 0x04 = Glove: - It gives the player the ability to pick up, carry, and then throw bombs.
            /// 0x20 = Slime Bomb: - Slime bomb will bounce on walls until they explode
            /// 0x40 = Power Bomb: - Allow the 1st bomb planted to be a Power Bombs. Power Bombs will explode with the maximum range of explosion possible (8 tiles)
            /// <code>
            /// </summary>
            public static readonly AddressData BombermanUpgrade = new(0x48D1, 1);
            /// <summary>
            ///This is a special battle item that has various effects. Some of the effects are actually quite useful where as others are bad.
            ///You can get rid of the skull by collecting an item or by passing it onto another opponent by touching them.The skull can't be destroyed; instead it'll bounce to another square when hit by a bomb blast.
            ///When the flag is set to 0x31, the skull effect is active.
            /// </summary>
            public static readonly AddressData Skull = new(0x3B6, 1);
        }
    }
}
