using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML.SuperMario64.Game.Data;

/// <summary>
/// Struct representing a single collision triangle in SM64 memory.
/// </summary>
internal struct CollisionTri
{
    private Triangle? cachedTriangle;
    public byte SurfaceType { get; private set; }
    public byte Flags { get; private set; }
    public byte Room { get; private set; }

    public short YMinMinus5 { get; private set; }
    public short YMaxPlus5 { get; private set; }

    public short Vertex1X { get; private set; }
    public short Vertex1Y { get; private set; }
    public short Vertex1Z { get; private set; }
    public short Vertex2X { get; private set; }
    public short Vertex2Y { get; private set; }
    public short Vertex2Z { get; private set; }
    public short Vertex3X { get; private set; }
    public short Vertex3Y { get; private set; }
    public short Vertex3Z { get; private set; }

    public float NormalX { get; private set; }
    public float NormalY { get; private set; }
    public float NormalZ { get; private set; }
    public float NormalOffset { get; private set; }

    public uint AssociatedObjectAddress { get; private set; }

    /// <summary>
    /// Creates a new SM64 collision triangle, using the 48 bytes given.
    /// </summary>
    public CollisionTri(byte[] bytes)
    {
        SurfaceType = bytes[0x00];
        Flags = bytes[0x04];
        Room = bytes[0x05];

        var offset = 0x06;
        YMinMinus5 = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        YMaxPlus5 = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;

        Vertex1X = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex1Y = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex1Z = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex2X = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex2Y = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex2Z = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex3X = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex3Y = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;
        Vertex3Z = GetShort(bytes[offset..(offset + 2)]);
        offset += 2;

        NormalX = GetFloat(bytes[offset..(offset + 4)]);
        offset += 4;
        NormalY = GetFloat(bytes[offset..(offset + 4)]);
        offset += 4;
        NormalZ = GetFloat(bytes[offset..(offset + 4)]);
        offset += 4;
        NormalOffset = GetFloat(bytes[offset..(offset + 4)]);
        offset += 4;

        AssociatedObjectAddress = GetUint(bytes[offset..(offset + 4)]);
        cachedTriangle = null;
    }

    public bool IsGround => NormalY > 0.01;
    public bool IsCeiling => NormalY < -0.01;
    public bool IsWall => !IsGround && !IsCeiling;

    public Triangle Triangle
    {
        get
        {
            if (!cachedTriangle.HasValue) cachedTriangle = new Triangle(new(Vertex1X, Vertex1Y, Vertex1Z), new(Vertex2X, Vertex2Y, Vertex2Z), new(Vertex3X, Vertex3Y, Vertex3Z), new(NormalX, NormalY, NormalZ));
            return cachedTriangle.Value;
        }
    }

    private static short GetShort(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToInt16(bytes, 0);
    }
    private static uint GetUint(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private static float GetFloat(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToSingle(bytes, 0);
    }
}
