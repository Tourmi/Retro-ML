namespace Retro_ML.Metroid.Game.Navigation
{
    internal class Navigator
    {
        private static readonly Objectives[] ObjectivesToComplete = new[] { Objectives.Kill, Objectives.Obtain };

        private static readonly List<Path> DefaultPaths = new()
        {
            new Path( // start to morph
                new PathPoint(3, 14),
                new PathPoint(2, 14, Objectives.Obtain)),
            new Path(new string[] { Requirement.HAS_MORPH_BALL }, // morph to missiles
                new PathPoint(2, 14),
                new PathPoint(11, 14),
                new PathPoint(11, 7),
                new PathPoint(13, 7),
                new PathPoint(13, 11),
                new PathPoint(18, 11, Objectives.Obtain)),
            new Path(new string[] { Requirement.HAS_MORPH_BALL, Requirement.HAS_MISSILES }, // missiles to bombs
                new PathPoint(18, 11),
                new PathPoint(13, 11),
                new PathPoint(13, 7),
                new PathPoint(30, 7),
                new PathPoint(30, 5),
                new PathPoint(25, 5, Objectives.Obtain)),
            new Path(new string[] { Requirement.HAS_MORPH_BALL, Requirement.HAS_BOMBS}, //bombs to brinstar ice beam
                new PathPoint(25,5),
                new PathPoint(30, 5),
                new PathPoint(30, 7),
                new PathPoint(22, 7),
                new PathPoint(22, 9),
                new PathPoint(19, 9, Objectives.Obtain))
        };

        public List<Path> Paths { get; private set; }

        public Navigator()
        {
            Paths = DefaultPaths;
        }

        public (int xDirr, int yDirr, Objectives objective) GetDirectionToGo(int x, int y, MetroidDataFetcher df)
        {
            var path = GetFirstValidPath(x, y, df);
            if (!path.HasValue) return (0, 0, Objectives.None);

            if (path.Value.IsPathComplete(x, y))
            {
                return (0, 0, path.Value.FinalObjective);
            }

            var dirr = path.Value.GetDirection(x, y);

            return (dirr.x, dirr.y, Objectives.Reach);
        }

        private Path? GetFirstValidPath(int x, int y, MetroidDataFetcher df)
        {
            foreach (var path in Paths.Reverse<Path>())
            {
                if (path.RequirementsSatisfied(df))
                {
                    if (path.IsOnPath(x, y)) return path;

                    if (path.IsPathComplete(x, y) && ObjectivesToComplete.Contains(path.FinalObjective))
                    {
                        if (path.FinalObjective == Objectives.Obtain)
                        {
                            //If there are no powerups in the same screen as Samus, then this path is complete
                            var powerups = df.GetPowerups().Where(p => (p[3] == 0) == df.IsSamusInFirstScreen());
                            if (!powerups.Any()) continue;
                        }
                        if (path.FinalObjective == Objectives.Kill)
                        {
                            //If there are no alive enemies, then this path is complete
                            var enemies = df.GetEnemies();
                            if (!enemies.Any()) continue;
                        }

                        return path;
                    }
                }
            }

            return null;
        }
    }
}
