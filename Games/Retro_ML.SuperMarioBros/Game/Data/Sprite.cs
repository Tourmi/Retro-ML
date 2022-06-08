namespace Retro_ML.SuperMarioBros.Game.Data
{
    /// <summary>
    /// A sprite is any interactable item in the game. Includes enemies, powerups, etc.
    /// </summary>
    internal struct Sprite
    {
        /// <summary>
        /// Walkable sprite :
        /// 0x24/0x25 - Static lift
        /// 0x26/0x27 - Vertical going lift 
        /// 0x28 - Horizontal going lift 
        /// 0x29 - Static lift (Will Fall if Player stays on it for too long
        /// 0x2A - Horizontal forward moving lift with strange hitbox
        /// 0x2B/0x2C - Halves of double lift (like 1.2)
        /// </summary>
        public static readonly int[] WalkableSprite = new int[] { 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C };
        /// <summary>
        /// Good Sprite :
        /// 0x2E - PowerUp Object
        /// 0x2F - Vine Object
        /// 0x30 - Flagpole Flag Object
        /// 0x31 - StarFlag Object
        /// 0x32 - Jump spring Object
        /// 0x34 - Warpzone
        /// 0x35 - Retainer Object
        /// </summary>
        public static readonly int[] GoodSprite = new int[] { 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x34, 0x35 };
        /// <summary>
        /// Firebar sprite :
        /// 0x1B/0x1C/0x1D/0x1E - Firebar
        /// 0x1F - Long Firebar (castle) AND sets previous enemy slot to 0x20 or else only half of the line shows
        /// </summary>
        public static readonly int[] FireBarSprite = new int[] { 0x1B, 0x1C, 0x1D, 0x1E, 0x1 };
    }
}
