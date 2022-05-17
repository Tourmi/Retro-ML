namespace Retro_ML.Tetris.Game
{
    /// <summary>
    /// RAM addresses used in Tetris.
    /// 
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public AddressData(uint address, uint length, uint highByteLocation = 0)
            {
                Address = address;
                Length = length;
                HighByteAddress = highByteLocation;
            }

            public uint Address;
            public uint Length;
            public uint HighByteAddress;
        }

        /// <summary>
        /// Where background tiles start in VRAM
        /// </summary>
        public static AddressData BackgroundTiles = new(0x9800, 1024); 

    }
}
