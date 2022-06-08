using Retro_ML.Utils;

namespace Retro_ML.Metroid.Game.Navigation
{
    /// <summary>
    /// Represents a single point on a path
    /// </summary>
    internal struct PathPoint
    {
        public int X;
        public int Y;
        public Objectives Objective;

        public PathPoint()
        {
            X = 0;
            Y = 0;
            Objective = Objectives.None;
        }

        public PathPoint(int x, int y, Objectives objective = Objectives.None)
        {
            X = x;
            Y = y;
            Objective = objective;
        }

        /// <summary>
        /// Returns the distance from this point to the other, assuming they share an axis.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int GetDistanceFrom(PathPoint other) => MathUtils.MaximumAbsoluteDifference(X, Y, other.X, other.Y);

        /// <summary>
        /// Returns the direction to go in to get from this point to the other point. Assumes this <see cref="PathPoint"/> shares an axis with the other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public (int xDirr, int yDirr) GetDirectionTo(PathPoint other)
        {
            return (X > other.X ? -1 : X < other.X ? 1 : 0,
                    Y > other.Y ? -1 : Y < other.Y ? 1 : 0);
        }

        /// <summary>
        /// Returns whether or not the given (<paramref name="x"/>,<paramref name="y"/>) coordinates are on the line between this and the other path point, excluding <paramref name="other"/>'s position.
        /// Assumes that this <see cref="PathPoint"/> and the other share an axis.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsOnPathLine(PathPoint other, int x, int y) =>
        (
            y == Y &&
            (
                x >= X && x < other.X ||
                x <= X && x > other.X
            )
        ) ||
        (
            x == X &&
            (
                y >= Y && y < other.Y ||
                y <= Y && y > other.Y
            )
        );


        public bool Equals(int x, int y) => (X, Y) == (x, y);
    }
}
