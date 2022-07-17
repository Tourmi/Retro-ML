using static Retro_ML.Utils.MathUtils;

namespace Retro_ML.Utils.Game.Geometry3D;
public struct AABB : IRaytracable
{
    public readonly Vector Position;
    public readonly Vector Size;
    public readonly Vector MinCorner;
    public readonly Vector MaxCorner;

    public AABB(Vector position, Vector size)
    {
        Position = position;
        Size = size.Absolute();

        MinCorner = position - (Size / 2f);
        MaxCorner = position + (Size / 2f);
    }

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
}
