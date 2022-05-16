namespace Retro_ML.SuperMarioKart.Game.Data
{
    internal class TrackObject
    {
        private const int MAX_COLLIDE_HEIGHT = 5;

        public short XPos { get; set; }
        public short XTilePos => (short)(XPos / 8);
        public short YPos { get; set; }
        public short YTilePos => (short)(YPos / 8);
        public short ZPos { get; set; }
        public short ZVelocity { get; set; }

        public bool IsThreatTo(int xTilePos, int yTilePos)
        {
            if (xTilePos != XTilePos || yTilePos != YTilePos) return false;

            if (ZPos > MAX_COLLIDE_HEIGHT && ZVelocity >= 0) return false;
            if (ZPos < 0 && ZVelocity <= 0) return false;

            return true;
        }
    }
}
