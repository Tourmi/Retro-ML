namespace Retro_ML.Metroid.Game
{
    /// <summary>
    /// <br>RAM addresses used in Metroid.</br>
    /// <br/>
    /// <br>Big thanks to </br>
    /// <br>- DataCrystal Wiki <see href="https://datacrystal.romhacking.net/wiki/Metroid:RAM_map"/></br>
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheDurations
            {
                Frame,
                Room
            }

            public AddressData(uint address, uint length, CacheDurations cacheDuration = CacheDurations.Frame)
            {
                Address = address;
                Length = length;
                CacheDuration = cacheDuration;
            }

            public uint Address;
            public uint Length;
            public CacheDurations CacheDuration;
        }
    }
}
