namespace Retro_ML.StreetFighter2Turbo.Game
{
    /// <summary>
    /// RAM addresses used in Street Fighter 2 Turbo
    /// 
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheTypes
            {
                Frame,
            }

            public AddressData(uint address, uint length, CacheTypes cacheTypes = CacheTypes.Frame, uint highByteLocation = 0)
            {
                Address = address;
                Length = length;
                CacheType = cacheTypes;
                HighByteAddress = highByteLocation;
            }

            public uint Address;
            public uint Length;
            public CacheTypes CacheType;
            public uint HighByteAddress;
        }

        public static class GameAddresses
        {
            /// <summary>
            /// Round Timer, runs from 0x9928 to 0x0000, but jumps back up to 0x0028 when time is up
            /// </summary>
            public static readonly AddressData RoundTimer = new(0x18F2, 2);
            
        }

        public static class Player1Addresses
        {
            /// <summary>
            /// Player X position
            /// </summary>
            public static readonly AddressData XPos = new(0x506, 2);
            /// <summary>
            /// Player Y position
            /// </summary>
            public static readonly AddressData YPos = new(0x509, 2);
            /// <summary>
            /// Flag set to 1 if the player is in the right screen. If the player is in the left screen, set to 0.
            /// </summary>
            public static readonly AddressData CurrentScreen = new(0x508, 1);
            /// <summary>
            /// Player state
            /// 0x02 when crouching
            /// 0x04 when jumping
            /// 0x0A when attacking
            /// 0x0E when hit
            /// 0x14 when staggered
            /// </summary>
            public static readonly AddressData State = new(0x503, 1);
            /// <summary>
            /// Player input
            /// 0x03 when going backward (blocking)
            /// </summary>
            public static readonly AddressData Input = new(0x53A, 1);
            /// <summary>
            /// Player HP. Starts at 0xB0 and set to 0xFF when dead
            /// </summary>
            public static readonly AddressData HP = new(0x530, 1);
            /// <summary>
            /// Player round win count. Game finished when player win 2 rounds (best of 3)
            /// </summary>
            public static readonly AddressData RoundsWin = new(0x5D0, 1);
            /// <summary>
            /// Set to 1 if the round is over, could be either via K.O or run out of time.
            /// </summary>
            public static readonly AddressData EndRoundStatus = new(0x5F2, 1);
        }

        public static class Player2Addresses
        {
            /// <summary>
            /// Player X position
            /// </summary>
            public static readonly AddressData XPos = new(0x706, 2);
            /// <summary>
            /// Player Y position
            /// </summary>
            public static readonly AddressData YPos = new(0x709, 2);
            /// <summary>
            /// Flag set to 1 if the player is in the right screen. If the player is in the left screen, set to 0.
            /// </summary>
            public static readonly AddressData CurrentScreen = new(0x708, 1);
            /// <summary>
            /// Player state
            /// 0x02 when crouching
            /// 0x04 when jumping
            /// 0x0A when attacking
            /// 0x0E when hit
            /// 0x14 when staggered
            /// </summary>
            public static readonly AddressData State = new(0x703, 1);
            /// Player input
            /// 0x03 when going backward (blocking)
            /// </summary>
            public static readonly AddressData Input = new(0x73A, 1);
            /// <summary>
            /// Player HP. Starts at 0xB0 and set to 0xFF when dead
            /// </summary>
            public static readonly AddressData HP = new(0x730, 1);
            /// <summary>
            /// Player round win count. Game finished when player win 2 rounds (best of 3)
            /// </summary>
            public static readonly AddressData RoundsWin = new(0x7D0, 1);
            /// <summary>
            /// Set to 1 if the round is over, could be either via K.O or run out of time.
            /// </summary>
            public static readonly AddressData EndRoundStatus = new(0x7F2, 1);
        }
    }
}
