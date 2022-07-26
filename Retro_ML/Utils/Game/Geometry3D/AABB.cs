using static Retro_ML.Utils.MathUtils;

namespace Retro_ML.Utils.Game.Geometry3D;
public struct AABB : IRaytracable
{
    public readonly Vector Position;
    public readonly Vector Size;
    public readonly Vector MinCorner;
    public readonly Vector MaxCorner;

    public float MinX => MinCorner.X;
    public float MaxX => MaxCorner.X;
    public float MinY => MinCorner.Y;
    public float MaxY => MaxCorner.Y;
    public float MinZ => MinCorner.Z;
    public float MaxZ => MaxCorner.Z;
    public bool Static { get; private set; }

    AABB IRaytracable.AABB => this;

    public AABB(Vector position, Vector size, bool isStatic = true)
    {
        Position = position;
        Size = size.Absolute();

        MinCorner = position - (0.5f * Size);
        MaxCorner = position + (0.5f * Size);
        Static = isStatic;
    }

    public AABB(Vector position, float size, bool isStatic = true)
    {
        this.Position = position;
        Size = new(MathF.Abs(size));

        MinCorner = position - (0.5f * Size);
        MaxCorner = position + (0.5f * Size);
        Static = isStatic;
    }

    public bool FullyContains(AABB other) =>
               MinX <= other.MinX
            && MinY <= other.MinY
            && MinZ <= other.MinZ
            && MaxX >= other.MaxX
            && MaxY >= other.MaxY
            && MaxZ >= other.MaxZ;

    public float GetRaytrace(Ray ray)
    {
        var invDir = ray.InverseDirection;

        var txmin = (MinCorner.X - ray.P.X) * invDir.X;
        var txmax = (MaxCorner.X - ray.P.X) * invDir.X;
        var tymin = (MinCorner.Y - ray.P.Y) * invDir.Y;
        var tymax = (MaxCorner.Y - ray.P.Y) * invDir.Y;
        var tzmin = (MinCorner.Z - ray.P.Z) * invDir.Z;
        var tzmax = (MaxCorner.Z - ray.P.Z) * invDir.Z;

        var tmin = Max(Min(txmin, txmax), Min(tymin, tymax), Min(tzmin, tzmax));
        var tmax = Min(Max(txmin, txmax), Max(tymin, tymax), Max(tzmin, tzmax));

        return tmax < 0 || tmin > tmax ? float.NaN : tmin;
    }

    public bool Contains(Vector p) =>
           MinX <= p.X
        && MinY <= p.Y
        && MinZ <= p.Z
        && MaxX >= p.X
        && MaxY >= p.Y
        && MaxZ >= p.Z;
}
