namespace Retro_ML.SuperMarioKart.Game.Data
{
    internal class Obstacle
    {
        private const int MAX_COLLIDE_HEIGHT = 5;

        public short XPos { get; set; }
        public short XTilePos => (short)(XPos / 8);
        public short YPos { get; set; }
        public short YTilePos => (short)(YPos / 8);
        public short ZPos { get; set; }
        public short XVelocity { get; set; }
        public short YVelocity { get; set; }
        public short ZVelocity { get; set; }

        public IEnumerable<(int x, int y)> GetThreateningTiles()
        {
            if (ZPos > MAX_COLLIDE_HEIGHT && ZVelocity >= 0) yield break;
            if (ZPos < 0 && ZVelocity <= 0) yield break;

            yield return new(XTilePos, YTilePos);
        }
    }
}
