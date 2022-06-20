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
            /// Player 1 X position
            /// </summary>
            public static readonly AddressData Player1XPos = new(0x23, 1);
            /// <summary>
            /// Player 2 X position
            /// </summary>
            public static readonly AddressData Player2XPos = new(0x27, 1);
            /// <summary>
            /// Player 1 is in what screen? A screen is 255 pixels wide
            /// </summary>
            public static readonly AddressData Player1CurrentScreen = new(0x22, 1);
            /// <summary>
            /// Player 2 is in what screen? A screen is 255 pixels wide
            /// </summary>
            public static readonly AddressData Player2CurrentScreen = new(0x26, 1);
            /// <summary>
            /// Player 1 state
            /// == 101 if the player 1 is airborn
            /// == 109 if the Player 1 is crouched
            /// </summary>
            public static readonly AddressData Player1State = new(0xE3, 2);
            /// <summary>
            /// == 101 if the player 2 is airborn
            /// == 109 if the Player 2 is crouched
            /// </summary>
            public static readonly AddressData Player2State = new(0xE6, 2);
            /// <summary>
            /// Round Timer, goes down to 0 when round time is over 
            /// </summary>
            public static readonly AddressData RoundTimer = new(0x18F3, 1);
            /// <summary>
            /// Player 1 HP bar represented on 11 bytes
            /// = 7 when byte is depleted and 8 if full
            /// </summary>
            public static readonly AddressData Player1HP = new(0x178E, 1);
            /// <summary>
            /// Player 2 HP bar represented on 11 bytes
            /// = 7 when byte is depleted and 8 if full
            /// </summary>
            public static readonly AddressData Player2HP = new(0x1771, 1);
            ///// <summary>
            ///// Player 1 HP bar represented on 11 bytes
            ///// = 7 when byte is depleted and 8 if full
            ///// </summary>
            //public static readonly AddressData Player1HP = new(0x1784, 11);
            ///// <summary>
            ///// Player 2 HP bar represented on 11 bytes
            ///// = 7 when byte is depleted and 8 if full
            ///// </summary>
            //public static readonly AddressData Player2HP = new(0x1771, 11);
        }
    }
}
