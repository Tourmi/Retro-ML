namespace Retro_ML.SuperBomberman3.Game
{
    /// <summary>
    /// RAM addresses used in Super Mario Bros.
    /// 
    /// <br>HUGE thanks to <see href="https://datacrystal.romhacking.net/wiki/Super_Mario_Bros.:RAM_map;game=smw">DataCrystal</see> for making those values available</br>
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheTypes
            {
                Frame,
                Level,
                TilesCache
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
            /// Player state
            /// <code>
            /// 0x02 when crouching
            /// 0x04 when jumping
            /// 0x0A when attacking
            /// 0x0E when hit
            /// 0x14 when staggered
            /// <code>
            /// </summary>
            public static readonly AddressData Lives = new(0x075A, 1);
        }

        public static class BombermanAddresses
        {
            /// <summary>
            /// Player X pos in pixels
            /// </summary>
            public static readonly AddressData XPos = new(0x14AF, 1);
            /// <summary>
            /// Player Y pos in pixels
            /// </summary>
            public static readonly AddressData YPos = new(0x14F7, 1);

        }

        public static class SpriteAddresses
        {
     
        }
    }
}
