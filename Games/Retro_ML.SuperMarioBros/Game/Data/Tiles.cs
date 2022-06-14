namespace Retro_ML.SuperMarioBros.Game.Data
{
    /// <summary>
    /// Class that keeps track of the level tilesets, which tiles are solid, good or bad.
    /// </summary>
    internal class Tiles
    {
        /// <summary>
        /// Good tiles that Mario would like to access : 194 coins - 192 ? block with coins - 193 ? block with powerup - 93 block with many coins
        /// </summary>
        public static readonly int[] GoodTile = new int[] { 0xC0, 0xC1, 0xC2, 0x5D };
    }
}
