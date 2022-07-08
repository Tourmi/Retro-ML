﻿namespace Retro_ML.SuperMario64.Game;

/// <summary>
/// <br>RAM addresses used in Super Mario 64.</br>
/// <br>Assume that multibyte values are in the Big Endian format</br>
/// <br/>
/// <br>Big thanks to </br>
/// <br>- STROOP contributors <see href="https://github.com/SM64-TAS-ABC/STROOP"/></br>
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

        public AddressData(uint address, uint length = 1, CacheDurations cacheDuration = CacheDurations.Frame, bool isPointer = false)
        {
            Address = address;
            Length = length;
            CacheDuration = cacheDuration;
            IsPointer = isPointer;
        }

        public uint Address;
        public uint Length;
        public CacheDurations CacheDuration;
        public bool IsPointer;
    }

    /// <summary>
    /// Memory addresses relating to the Area
    /// </summary>
    public static class Area
    {
        /// <summary>
        /// Byte representing the Area Mario is currently in
        /// </summary>
        public static readonly AddressData CurrentID = new(0x8033B4BC);
    }

    /// <summary>
    /// Memory addresses relating to the overall progression
    /// </summary>
    public static class Progress
    {
        /// <summary>
        /// The flags for the stars Mario collected so far
        /// Each byte consists of a single course and is formatted like so : 
        /// <code>
        /// bit 0 : unknown
        /// bit 1 : 100 coin star
        /// bit 2 : course star 1
        /// bit 3 : course star 2
        /// bit 4 : course star 3
        /// bit 5 : course star 4
        /// bit 6 : course star 5
        /// bit 7 : course star 6
        /// </code>
        /// </summary>
        public static readonly AddressData StarFlags = new(0x8020770C, 15);
        /// <summary>
        /// Amount of stars Mario has, as displayed. Stored as a signed short
        /// </summary>
        public static readonly AddressData StarCount = new(0x8033B264, 2);
    }

    public static class Mario
    {
        /// <summary>
        /// Mario's current hat flags
        /// <code>
        /// ?? - Regular cap
        /// ?? - no cap
        /// ?? - vanish cap
        /// ?? - wings cap
        /// ?? - metal cap
        /// </code>
        /// </summary>
        public static readonly AddressData HatFlags = new(0x8033B176, 2);
        /// <summary>
        /// Mario's current coins
        /// </summary>
        public static readonly AddressData Coins = new(0x8033B218, 2);
        /// <summary>
        /// Mario's current lives
        /// </summary>
        public static readonly AddressData Lives = new(0x8033B21D);
        /// <summary>
        /// Signed byte that represents Mario's distance from the ground
        /// </summary>
        public static readonly AddressData GroundOffset = new(0x8033B220);
        /// <summary>
        /// Mario's current health (or Power)
        /// High byte is the value, low byte acts as a decimal.
        /// </summary>
        public static readonly AddressData Health = new(0x8033B21E, 2);

        /// <summary>
        /// Mario's current X position, as a float
        /// </summary>
        public static readonly AddressData XPos = new(0x8033B1AC, 4);
        /// <summary>
        /// Mario's current Y position, as a float
        /// </summary>
        public static readonly AddressData YPos = new(0x8033B1B0, 4);
        /// <summary>
        /// Mario's current Z position, as a float
        /// </summary>
        public static readonly AddressData ZPos = new(0x8033B1B4, 4);
        /// <summary>
        /// Mario's current X speed, as a float
        /// </summary>
        public static readonly AddressData XSpeed = new(0x8033B1B8, 4);
        /// <summary>
        /// Mario's current Y speed, as a float
        /// </summary>
        public static readonly AddressData YSpeed = new(0x8033B1BC, 4);
        /// <summary>
        /// Mario's current Z speed, as a float
        /// </summary>
        public static readonly AddressData ZSpeed = new(0x8033B1C0, 4);

        /// <summary>
        /// Mario's current horizontal speed, as a float
        /// </summary>
        public static readonly AddressData HorizontalSpeed = new(0x8033B1C4, 4);
    }

    /// <summary>
    /// Addresses used by in-game objects, such as signs, goombas, elevators, stars, etc.
    /// </summary>
    public static class Object
    {
        /// <summary>
        /// Number of currently loaded objects
        /// </summary>
        public static readonly AddressData ObjectNumber = new(0x8033D270, 4);
    }

    /// <summary>
    /// Addresses used for solid collisions. Information available thanks to <see href="https://github.com/SM64-TAS-ABC/STROOP/blob/Development/STROOP/Structs/Configurations/TriangleConfig.cs"/>
    /// </summary>
    public static class Collision
    {
        /// <summary>
        /// The amount of triangles in the current collision map (static + objects)
        /// </summary>
        public static readonly AddressData TotalTriangleCount = new(0x80361170, 4);
        /// <summary>
        /// Amount of triangle in the current static collision map
        /// </summary>
        public static readonly AddressData StaticTriangleCount = new(0x80361178, 4);
        /// <summary>
        /// Pointer to the current level's triangle list. 
        /// Sorted by static, then dynamic triangles. 
        /// See <see cref="StaticTriangleCount"/> and <see cref="TotalTriangleCount"/> for this list's counts.
        /// </summary>
        public static readonly AddressData TrianglesListPointer = new(0x8038EE9C, 4, isPointer: true);

        /// <summary>
        /// Where the static triangle partition starts. Triangles take up 0x30 bytes each. See <see cref="StaticTriangleCount"/> for triangle count.
        /// </summary>
        public static readonly AddressData StaticTrianglesPartition = new(0x8038BE98);
        /// <summary>
        /// Where the dynamic triangle partition starts. Triangles take up 0x30 bytes each.
        /// </summary>
        public static readonly AddressData DynamicTrianglesPartition = new(0x8038D698);
    }

    /// <summary>
    /// Addresses for the in-game Camera
    /// </summary>
    public static class Camera
    {
        /// <summary>
        /// Camera's X position, as a float
        /// </summary>
        public static readonly AddressData XPos = new(0x8033C6A4, 4);
        /// <summary>
        /// Camera's Y position, as a float
        /// </summary>
        public static readonly AddressData YPos = new(0x8033C6A8, 4);
        /// <summary>
        /// Camera's Z position, as a float
        /// </summary>
        public static readonly AddressData ZPos = new(0x8033C6AC, 4);

        /// <summary>
        /// Camera horizontal angle, as an unsigned short angle.
        /// </summary>
        public static readonly AddressData HorizontalAngle = new(0x8033C6E4, 2);
    }
}