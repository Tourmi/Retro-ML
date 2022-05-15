namespace Retro_ML.SuperMarioKart.Game.Data
{
    internal static class TiletypeSurface
    {
        private static byte[] OffroadTiles = new byte[] { 0x18, 0x20, 0x22, 0x24, 0x54, 0x56, 0x58, 0x5A, 0x5C, 0x80 };
        private static byte[] SolidTiles = new byte[] { 0x80 };
        private static byte[] PitTiles = new byte[] { 0x20, 0x22, 0x24 };

        public static bool IsOffroad(byte tiletype) => OffroadTiles.Contains(tiletype);
        public static bool IsSolid(byte tiletype) => SolidTiles.Contains(tiletype);
        public static bool IsPit(byte tiletype) => PitTiles.Contains(tiletype);
    }
}
