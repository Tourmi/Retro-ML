namespace Retro_ML.Metroid.Game.Navigation
{
    internal static class Requirement
    {
        public const string HAS_MORPH_BALL = "HAS_MORPHBALL";
        public const string HAS_MISSILES = "HAS_MISSILES";
        public const string HAS_BOMBS = "HAS_BOMBS";
        public const string HAS_ICE_BEAM = "HAS_ICEBEAM";
        public const string HAS_HIGH_JUMP = "HAS_HIGHJUMP";

        private static readonly Dictionary<string, Func<MetroidDataFetcher, bool>> Requirements = new()
        {
            [HAS_MORPH_BALL] = df => df.HasMorphBall(),
            [HAS_MISSILES] = df => df.HasMissiles(),
            [HAS_BOMBS] = df => df.HasBombs(),
            [HAS_ICE_BEAM] = df => df.HasIceBeam(),
            [HAS_HIGH_JUMP] = df => df.HasHighJump(),
        };

        /// <summary>
        /// Returns true if the current game state satisfies all the specified requirements
        /// </summary>
        public static bool SatisfyRequirements(MetroidDataFetcher df, IEnumerable<string> requirements)
        {
            foreach (var requirement in requirements)
            {
                if (!Requirements.ContainsKey(requirement))
                {
                    throw new ArgumentException($"The requirement {requirement} does not exist.");
                }

                if (!Requirements[requirement](df))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
