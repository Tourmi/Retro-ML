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
                new PathPoint(25, 5),
                new PathPoint(30, 5),
                new PathPoint(30, 7),
                new PathPoint(22, 7),
                new PathPoint(22, 9),
                new PathPoint(19, 9, Objectives.Obtain)),
            new Path(new string[] { Requirement.HAS_ICE_BEAM, Requirement.HAS_MISSILES }, //ice beam to long beam
                new PathPoint(19, 9),
                new PathPoint(22, 9), //not possible without high jump, maybe skip ice beam or death warp
                new PathPoint(22, 7),
                new PathPoint(11, 7),
                new PathPoint(11, 5),
                new PathPoint(7, 5, Objectives.Obtain)),
            new Path(new string[] {Requirement.HAS_LONG_BEAM, Requirement.HAS_MORPH_BALL, Requirement.HAS_MISSILES, Requirement.HAS_BOMBS, Requirement.HAS_ICE_BEAM}, //long beam to Kraid
                new PathPoint(7, 5),
                new PathPoint(11, 5),
                new PathPoint(11, 14),
                new PathPoint(6, 14),
                new PathPoint(6, 18),
                new PathPoint(7, 18),
                new PathPoint(7, 21),
                new PathPoint(1, 21),
                new PathPoint(1, 27),
                new PathPoint(3, 27),
                new PathPoint(3, 28),
                new PathPoint(9, 28),
                new PathPoint(9, 29),
                new PathPoint(8, 29, Objectives.Kill)),
            new Path(new string[] { Requirement.KILLED_KRAID }, //kraid to high jump
                new PathPoint(8, 29),
                new PathPoint(12, 29),
                new PathPoint(12, 24),
                new PathPoint(7, 24),
                new PathPoint(7, 26),
                new PathPoint(4, 26),
                new PathPoint(4, 24),
                new PathPoint(7, 24),
                new PathPoint(7, 18),
                new PathPoint(6, 18),
                new PathPoint(6, 14),
                new PathPoint(11, 14),
                new PathPoint(11, 7),
                new PathPoint(13, 7),
                new PathPoint(13, 11),
                new PathPoint(22, 11),
                new PathPoint(22, 14),
                new PathPoint(30, 14),
                new PathPoint(30, 17),
                new PathPoint(27, 17, Objectives.Obtain)),
            new Path(new string[] { Requirement.HAS_HIGH_JUMP }, //high jump to screw attack
                new PathPoint(27, 17),
                new PathPoint(18, 17),
                new PathPoint(18, 16),
                new PathPoint(15, 16, Objectives.Obtain)),
            new Path(new string[] {Requirement.HAS_SCREW_ATTACK}, //screw to ridley
                new PathPoint(15, 16),
                new PathPoint(18, 16),
                new PathPoint(18, 17),
                new PathPoint(30, 17),
                new PathPoint(30, 21),
                new PathPoint(24, 21),
                new PathPoint(24, 22),
                new PathPoint(25, 22),
                new PathPoint(25, 25),
                new PathPoint(30, 25),
                new PathPoint(30, 29),
                new PathPoint(17, 29, Objectives.Kill)),
            new Path(new string[] { Requirement.KILLED_RIDLEY }, //ridley to mother brain
                new PathPoint(17, 29),
                new PathPoint(30, 29),
                new PathPoint(30, 25),
                new PathPoint(25, 25),
                new PathPoint(25, 22), //would be faster to death warp
                new PathPoint(24, 22),
                new PathPoint(24, 21),
                new PathPoint(30, 21),
                new PathPoint(30, 14),
                new PathPoint(22, 14),
                new PathPoint(22, 11),
                new PathPoint(13, 11),
                new PathPoint(13, 7),
                new PathPoint(11, 7),
                new PathPoint(11, 2),
                new PathPoint(3, 2),
                new PathPoint(3, 7),
                new PathPoint(10, 7),
                new PathPoint(10, 11),
                new PathPoint(2, 11, Objectives.Kill)),
            new Path(new string[] { Requirement.KILLED_MOTHER_BRAIN },
                new PathPoint(2,11),
                new PathPoint(1, 2, Objectives.Reach)) //1,2 is actually an invalid coordinate, but it tells the AI it should take the elevator
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
