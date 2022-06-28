namespace Retro_ML.SuperMario64.Game;

/// <summary>
/// <br>RAM addresses used in Super Mario 64.</br>
/// <br/>
/// <br>Big thanks to </br>
/// <br>- Hack64 <see href="https://hack64.net/wiki/doku.php?id=super_mario_64:ram_memory_map"/></br>
/// <br>- SMWCentral <see href="https://www.smwcentral.net/?p=memorymap&amp;game=sm64&amp;region=ram"/></br>
/// </summary>
internal static class Addresses
{
    public struct AddressData
    {
        public enum CacheDurations
        {
            Frame,
            Level
        }

        public AddressData(uint address, uint length = 1, CacheDurations cacheDuration = CacheDurations.Frame)
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
