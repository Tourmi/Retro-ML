namespace Retro_ML.Game.SuperMarioWorld.Data
{
    /// <summary>
    /// The different animation states the player (Mario) can be in.
    /// </summary>
    internal static class PlayerAnimationStates
    {
        public const byte NONE = 0x00;
        public const byte FLASHING = 0x01;
        public const byte GET_MUSHROOM = 0x02;
        public const byte GET_FEATHER = 0x03;
        public const byte GET_FIRE_FLOWER = 0x04;
        public const byte ENTER_HORIZONTAL_PIPE = 0x05;
        public const byte ENTER_VERTICAL_PIPE = 0x06;
        public const byte SHOOT_FROM_PIPE = 0x07;
        public const byte YOSHI_WINGS = 0x08;
        public const byte DYING = 0x09;
        public const byte CASTLE_ENTRANCE_MOVE = 0x0A;
        public const byte PLAYER_FROZEN = 0x0B;
        public const byte CASTLE_DESTRUCTIONS_MOVE = 0x0C;
        public const byte ENTER_DOOR = 0x0D;
    }

}
