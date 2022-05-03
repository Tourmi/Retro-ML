namespace Retro_ML.Game.SuperMarioWorld.Data
{
    /// <summary>
    /// The different statuses a sprite can have. Anything above 0x08 is considered alive.
    /// </summary>
    internal static class SpriteStatuses
    {
        public const byte FREE = 0x00;
        public const byte INITIAL_PHASE = 0x01;
        public const byte KILLED = 0x02;
        public const byte SMUSHED = 0x03;
        public const byte KILLED_BY_SPINJUMP = 0x04;
        public const byte SINKING_IN_LAVA_MUD = 0x05;
        public const byte TURNING_INTO_COIN_LEVEL_END = 0x06;
        public const byte STAY_IN_YOSHI_MOUTH = 0x07;
        public const byte NORMAL = 0x08;
        public const byte STATIONARY_OR_CARRYABLE = 0x09;
        public const byte KICKED = 0x0A;
        public const byte CARRIED = 0x0B;
        public const byte POWERUP_GOAL_TAPE = 0x0C;

        /// <summary>
        /// Returns whether or not the sprite is alive depending on the status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool IsAlive(byte status) => status >= NORMAL;
        /// <summary>
        /// Returns whether or not the sprite could be dangerous to the player, assuming it is a dangerous sprite, depending on the status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool CanBeDangerous(byte status) => status == NORMAL || status == KICKED;
    }
}
