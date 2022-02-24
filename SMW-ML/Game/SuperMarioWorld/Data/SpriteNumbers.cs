namespace SMW_ML.Game.SuperMarioWorld.Data
{
    internal static class SpriteNumbers
    {
        public const byte KEYHOLE = 0x0E;
        public const byte DISPLAY_LEVEL_MESSAGE_1 = 0x19;
        public const byte MOVING_COIN = 0x21;
        public const byte YOSHI_EGG = 0x2C;
        public const byte BABY_YOSHI = 0x2D;
        public const byte SPRINGBOARD = 0x2F;
        public const byte YOSHI = 0x35;
        public const byte PSWITCH = 0x3E;
        public const byte DOLPHIN_LONG = 0x41;
        public const byte DOLPHIN_SHORT = 0x42;
        public const byte DOLPHIN_VERTICAL = 0x43;
        public const byte DIRECTIONAL_COINS = 0x45;
        public const byte GROWING_PIPE = 0x49;
        public const byte GOAL_SPHERE = 0x4A;
        public const byte THROWBLOCK = 0x53;
        public const byte NET_DOOR = 0x54;
        public const byte ROPE_MECHANISM = 0x64;
        public const byte COIN_GAME_CLOUD = 0x6A;
        public const byte WALL_SPRING_LEFT = 0x6B;
        public const byte WALL_SPRING_RIGHT = 0x6C;
        public const byte INVISIBLE_SOLID_BLOCK = 0x6D;
        public const byte MUSHROOM = 0x74;
        public const byte FLOWER = 0x75;
        public const byte STAR = 0x76;
        public const byte FEATHER = 0x77;
        public const byte ONE_UP = 0x78;
        public const byte GOAL_TAPE = 0x7B;
        public const byte P_BALLOON = 0x7D;
        public const byte FLYING_RED_COIN_OR_WINGS = 0x7E;
        public const byte FLYING_1_UP = 0x7F;
        public const byte FLYING_KEY = 0x80;
        public const byte FLYING_BLOCK_1 = 0x83;
        public const byte FLYING_BLOCK_2 = 0x84;
        public const byte LAKITU_CLOUD = 0x87;
        public const byte LAYER_3_CAGE = 0x88;
        public const byte LAYER_3_SMASH = 0x89;
        public const byte HOUSE_BIRD = 0x8A;
        public const byte HOUSE_SMOKE = 0x8B;
        public const byte SIDE_EXIT = 0x8C;
        public const byte GHOST_HOUSE_EXIT = 0x8D;
        public const byte INVISIBLE_WARP_HOLE = 0x8E;
        public const byte SCALE_PLATFORMS = 0x8F;
        public const byte HAMMER_BRO_PLATFORM = 0x9C;
        public const byte CHAINED_GREY_PLATFORM = 0xA3;
        public const byte IGGY_BALL = 0xA7;
        public const byte CREATING_EATING_BLOCK = 0xB1;
        public const byte CARROT_TOP_UP_RIGHT = 0xB7;
        public const byte CARROT_TOP_UP_LEFT = 0xB8;
        public const byte MESSAGE_BOX = 0xB9;
        public const byte TIMED_LIFT = 0xBA;
        public const byte CASTLE_BLOCK = 0xBB;
        public const byte LAVA_PLATFORM = 0xC0;
        public const byte FLYING_GREY_PLATFORM = 0xC1;
        public const byte FALLING_GREY_PLATFORM = 0xC4;
        public const byte SPOTLIGHT = 0xC6;
        public const byte INVISIBLE_MUSHROOM = 0xC7;
        public const byte LIGHT_SWITCH = 0xC8;

        public const byte PLATFORMS_MIN = 0x55;
        public const byte REVOLVING_PLATFORM = 0x5F;
        public const byte PLATFORMS_MAX = 0x63;
        public const byte POWERUPS_ETC_MIN = 0x74;
        public const byte POWERUPS_ETC_MAX = 0x84;

        public static bool IsDangerous(byte number)
        {
            if (number >= PLATFORMS_MIN && number <= PLATFORMS_MAX) return false;
            if (number >= POWERUPS_ETC_MIN && number <= POWERUPS_ETC_MAX) return false;

            return number switch
            {
                KEYHOLE or
                DISPLAY_LEVEL_MESSAGE_1 or
                MOVING_COIN or
                YOSHI_EGG or
                BABY_YOSHI or
                SPRINGBOARD or
                YOSHI or
                PSWITCH or
                DOLPHIN_LONG or
                DOLPHIN_SHORT or
                DOLPHIN_VERTICAL or
                DIRECTIONAL_COINS or
                GROWING_PIPE or
                GOAL_SPHERE or
                THROWBLOCK or
                NET_DOOR or
                ROPE_MECHANISM or
                COIN_GAME_CLOUD or
                WALL_SPRING_LEFT or
                WALL_SPRING_RIGHT or
                INVISIBLE_SOLID_BLOCK or
                LAKITU_CLOUD or
                LAYER_3_CAGE or
                LAYER_3_SMASH or
                HOUSE_BIRD or
                HOUSE_SMOKE or
                SIDE_EXIT or
                GHOST_HOUSE_EXIT or
                INVISIBLE_WARP_HOLE or
                SCALE_PLATFORMS or
                HAMMER_BRO_PLATFORM or
                CHAINED_GREY_PLATFORM or
                IGGY_BALL or
                CREATING_EATING_BLOCK or
                CARROT_TOP_UP_RIGHT or
                CARROT_TOP_UP_LEFT or
                MESSAGE_BOX or
                TIMED_LIFT or
                CASTLE_BLOCK or
                LAVA_PLATFORM or
                FLYING_GREY_PLATFORM or
                FALLING_GREY_PLATFORM or
                SPOTLIGHT or
                INVISIBLE_MUSHROOM or
                LIGHT_SWITCH
                => false,
                _ => true,
            };
        }

        public static bool IsGood(byte number)
        {
            return number switch
            {
                MOVING_COIN or
                YOSHI_EGG or
                BABY_YOSHI or
                SPRINGBOARD or
                YOSHI or
                PSWITCH or
                DIRECTIONAL_COINS or
                GOAL_SPHERE or
                LAKITU_CLOUD or
                INVISIBLE_SOLID_BLOCK or
                MUSHROOM or
                FLOWER or
                STAR or
                FEATHER or
                ONE_UP or
                GOAL_TAPE or
                P_BALLOON or
                FLYING_RED_COIN_OR_WINGS or
                FLYING_1_UP or
                FLYING_KEY or
                FLYING_BLOCK_1 or
                FLYING_BLOCK_2
                => true,
                _ => false
            };
        }

        public static bool IsSolid(byte number)
        {
            if (number >= PLATFORMS_MIN && number <= PLATFORMS_MAX) return true;

            return number switch
            {
                DOLPHIN_LONG or
                DOLPHIN_SHORT or
                DOLPHIN_VERTICAL or
                GROWING_PIPE or
                SCALE_PLATFORMS or
                HAMMER_BRO_PLATFORM or
                CHAINED_GREY_PLATFORM or
                CREATING_EATING_BLOCK or
                CARROT_TOP_UP_LEFT or
                CARROT_TOP_UP_RIGHT or
                MESSAGE_BOX or
                TIMED_LIFT or
                CASTLE_BLOCK or
                LAVA_PLATFORM or
                FLYING_GREY_PLATFORM or
                FALLING_GREY_PLATFORM or
                LIGHT_SWITCH => true,
                _ => false
            };
        }

        public static bool RotatesAroundOrigin(byte number)
        {
            return number switch
            {
                REVOLVING_PLATFORM or
                CHAINED_GREY_PLATFORM => true,
                _ => false
            };
        }
    }
}
