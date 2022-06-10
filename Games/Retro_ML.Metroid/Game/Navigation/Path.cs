namespace Retro_ML.Metroid.Game.Navigation
{
    internal struct Path
    {
        public PathPoint[] PathPoints;
        public string[] Requirements;

        /// <summary>
        /// Returns the final <see cref="Objectives"/> of this <see cref="Path"/>
        /// </summary>
        public Objectives FinalObjective => PathPoints[^1].Objective;

        public Path()
        {
            PathPoints = Array.Empty<PathPoint>();
            Requirements = Array.Empty<string>();
        }

        public Path(params PathPoint[] pathPoints) : this(Enumerable.Empty<string>(), pathPoints) { }

        public Path(IEnumerable<string> requirements, params PathPoint[] pathPoints)
        {
            PathPoints = pathPoints;
            Requirements = requirements.ToArray();
        }

        /// <summary>
        /// Returns whether or not the requirements for this path are satisfied
        /// </summary>
        /// <param name="df"></param>
        /// <returns></returns>
        public bool RequirementsSatisfied(MetroidDataFetcher df)
        {
            return Requirement.SatisfyRequirements(df, Requirements);
        }

        /// <summary>
        /// Returns the direction to go in
        /// </summary>
        public (int x, int y) GetDirection(int currX, int currY)
        {
            for (int i = PathPoints.Length - 2; i >= 0; i--)
            {
                if (!PathPoints[i].IsOnPathLine(PathPoints[i + 1], currX, currY)) continue;

                return PathPoints[i].GetDirectionTo(PathPoints[i + 1]);
            }

            return (0, 0);
        }

        /// <summary>
        /// Returns whether or not the given (<paramref name="x"/>,<paramref name="y"/>) coordinates are within this <see cref="Path"/>
        /// </summary>
        public bool IsOnPath(int x, int y) => GetDirection(x, y) != (0, 0);

        /// <summary>
        /// Returns whether or not the given (<paramref name="x"/>,<paramref name="y"/>) coordinates are the final point of this <see cref="Path"/>
        /// </summary>
        public bool IsPathComplete(int x, int y) => PathPoints[^1].X == x && PathPoints[^1].Y == y;
    }
}
