namespace Retro_ML.SuperMarioKart.Game
{
    /// <summary>
    /// <br>RAM addresses used in Super Mario World.</br>
    /// <br/>
    /// <br>Big thanks to </br>
    /// <br>- smkdan <see href="https://smkdan.eludevisibility.org/smk.html#Starting"/></br>
    /// <br>- MrL314 <see href="https://www.smwcentral.net/?p=viewthread&amp;t=93458"/></br>
    /// <br>- DataCrystal Wiki <see href="https://datacrystal.romhacking.net/wiki/Super_Mario_Kart:RAM_map"/></br>
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheDurations
            {
                Frame,
                Race
            }

            public AddressData(uint address, uint length, CacheDurations cacheDuration = CacheDurations.Frame, uint highByteLocation = 0)
            {
                Address = address;
                Length = length;
                CacheDuration = cacheDuration;
                HighByteAddress = highByteLocation;
            }

            public uint Address;
            public uint Length;
            public CacheDurations CacheDuration;
            public uint HighByteAddress;
        }

        /// <summary>
        /// Regroups addresses relating to the current race
        /// </summary>
        public static class Race
        {
            /// <summary>
            /// <br>0: 50cc</br>
            /// <br>2: 100cc</br>
            /// <br>4: 150cc</br>
            /// </summary>
            public static AddressData CC => new(0x30, 1);
        }

        /// <summary>
        /// Regroups addresses relating to the current racetrack
        /// </summary>
        public static class Racetrack
        {
            /// <summary>
            /// Tile Sprite Map (128x128)
            /// </summary>
            public static AddressData TileMap => new(0x1_0000, 0x4000, AddressData.CacheDurations.Race);
            /// <summary>
            /// Flow Map of "Forward" angles of map for each 2x2 tile area. (64x64)
            /// </summary>
            public static AddressData FlowMap => new(0x1_4000, 0x1000, AddressData.CacheDurations.Race);
            /// <summary>
            /// Checkpoint pointer for each 2x2 tile area (64x64)
            /// 
            /// See <see cref="CheckpointsSpeedAngle"/>, <see cref="CheckPointsLakituX"/>, <see cref="CheckPointsLakituY"/>
            /// </summary>
            public static AddressData CheckpointData => new(0x1_5000, 0x1000, AddressData.CacheDurations.Race);
            /// <summary>
            /// Checkpoint Speed and Angle Table (2 bytes, Low: Speed, High: Angle)
            /// </summary>
            public static AddressData CheckpointsSpeedAngle => new(0x0800, 0x100, AddressData.CacheDurations.Race);
            /// <summary>
            /// Checkpoint Lakitu Drop X-Coordinate Table (2 bytes each)
            /// </summary>
            public static AddressData CheckPointsLakituX => new(0x0800, 0x100, AddressData.CacheDurations.Race);
            /// <summary>
            /// Checkpoint Lakitu Drop Y-Coordinate Table (2 bytes each)
            /// </summary>
            public static AddressData CheckPointsLakituY => new(0x0800, 0x100, AddressData.CacheDurations.Race);
        }

        /// <summary>
        /// Regroups addresses relating to a racer
        /// </summary>
        public static class Racer
        {
            /// <summary>
            /// Table of all 8 racers, 256 bytes each.
            /// </summary>
            public static AddressData Racers => new(0x1000, 0x800);
            /// <summary>
            /// All of the information for the main racer
            /// </summary>
            public static AddressData MainRacer => new(0x1000, 0x100);
            /// <summary>
            /// What character this racer is. [00: Mario, 02: Luigi, 04: Bowser, 06: Peach, 08: DK, 0A: Koopa, 0C: Toad 0E: Yoshi]
            /// </summary>
            public static AddressData CharacterNumber => new(0x1012, 1);
            /// <summary>
            /// X position of the racer (To tile position -> (XPosition-1)/16)
            /// </summary>
            public static AddressData XPosition => new(0x1018, 2);
            /// <summary>
            /// Y position of the racer (To tile position -> (YPosition-1)/16)
            /// </summary>
            public static AddressData YPosition => new(0x101C, 4);
            /// <summary>
            /// X velocity of the racer
            /// </summary>
            public static AddressData XVelocity => new(0x1022, 2);
            /// <summary>
            /// Y velocity of the racer
            /// </summary>
            public static AddressData YVelocity => new(0x1024, 2);
            /// <summary>
            /// Current heading direction of the racer
            /// </summary>
            public static AddressData HeadingAngle => new(0x102A, 2);
            /// <summary>
            /// Racer rank, given as (Rank-1)*2 (So possible values are 0x0,0x2,0x4,0x6,0x8,0xA,0xC,0xE)
            /// </summary>
            public static AddressData CurrentRank => new(0x1040, 1);
            /// <summary>
            /// [00: On ground, 02: Jump/Hop/Ramp, 04: Fallen off, 06: In Lava, 08: In water, 0A: currently unknown, 0C: Lakitu taking coins from driver]
            /// </summary>
            public static AddressData KartStatus => new(0x10A0, 1);
            /// <summary>
            /// Current checkpoint number of the racer
            /// </summary>
            public static AddressData CurrentCheckpointNumber => new(0x10C0, 1);
            /// <summary>
            /// Given as 128+Lap
            /// </summary>
            public static AddressData CurrentLap => new(0x10C1, 1);
            /// <summary>
            /// Flowmap direction, aka "Desired" angle. Points directly to Checkpoint Coordinates
            /// </summary>
            public static AddressData FlowmapDirection => new(0x10D0, 2);
            /// <summary>
            /// Maximum speed of the racer.
            /// </summary>
            public static AddressData MaximumSpeed => new(0x10D6, 2);
            /// <summary>
            /// Absolute speed of the racer
            /// </summary>
            public static AddressData AbsoluteSpeed => new(0x10EA, 2);
            /// <summary>
            /// Timer for the mini-turbo
            /// </summary>
            public static AddressData MiniTurboTimer => new(0x10FC, 1);
        }
    }
}
