﻿using static Retro_ML.Utils.MathUtils;

namespace Retro_ML.Utils.Game.Geometry3D;
public struct Triangle : IRaytracable
{
    private const float EPSILON = 0.00001f;

    private AABB? cachedAABB;

    public readonly Vector V1;
    public readonly Vector V2;
    public readonly Vector V3;
    public readonly Vector Normal;

    /// <summary>
    /// Assumes that <paramref name="normal"/> is already normalized.
    /// </summary>
    public Triangle(Vector v1, Vector v2, Vector v3, Vector normal, bool isStatic = true)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        Normal = normal;

        Static = isStatic;
        cachedAABB = null;
    }

    public float MinX => Min(V1.X, V2.X, V3.X);
    public float MaxX => Max(V1.X, V2.X, V3.X);
    public float MinY => Min(V1.Y, V2.Y, V3.Y);
    public float MaxY => Max(V1.Y, V2.Y, V3.Y);
    public float MinZ => Min(V1.Z, V2.Z, V3.Z);
    public float MaxZ => Max(V1.Z, V2.Z, V3.Z);

    public bool Static { get; private set; }

    public AABB AABB
    {
        get
        {
            if (!cachedAABB.HasValue) cachedAABB = new(0.5f * new Vector(MinX + MaxX, MinY + MaxY, MinZ + MaxZ), new Vector(MaxX - MinX, MaxY - MinY, MaxZ - MinZ));
            return cachedAABB.Value;
        }
    }

    public float GetRaytrace(Ray ray)
    {
        //return early if this ray is within 90 degrees of the triangle's normal, or almost parallel
        var dot = ray.Direction.Dot(Normal);
        if (dot > -EPSILON) return float.NaN;

        var edge1 = V2 - V1;
        var edge2 = V3 - V1;

        var perpendicular = ray.Direction.Cross(edge2);
        var invDeterminant = 1.0f / edge1.Dot(perpendicular);

        var distV1 = ray.P - V1;
        var u = distV1.Dot(perpendicular) * invDeterminant;
        if (u is < 0 or > 1) return float.NaN;

        var q = distV1.Cross(edge1);
        var v = ray.Direction.Dot(q) * invDeterminant;
        if (v < 0 || v + u > 1) return float.NaN;

        var dist = invDeterminant * edge2.Dot(q);

        return dist > EPSILON ? dist : float.NaN;
    }
    public bool Contains(Vector p) => false;

    public override string ToString() => $"{{\nVertex1:{V1};\nVertex2:{V2};\nVertex3:{V3};\nNormal:{Normal}}}";
}
