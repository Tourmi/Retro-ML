namespace Retro_ML.Utils.Game.Geometry3D;
public struct Sphere : IRaytracable
{
    public readonly Vector Pos;
    public readonly float Radius;
    public readonly float RadiusSquared;

    public Sphere(Vector pos, float radius)
    {
        Pos = pos;
        Radius = radius;
        RadiusSquared = radius * radius;
    }

    public float MinX => Pos.X - Radius;
    public float MaxX => Pos.X + Radius;
    public float MinY => Pos.Y - Radius;
    public float MaxY => Pos.Y + Radius;
    public float MinZ => Pos.Z - Radius;
    public float MaxZ => Pos.Z + Radius;
    public bool Static => true;

    public AABB AABB => new(Pos, Radius * 2);

    public bool Contains(Vector point) => (point - Pos).SquaredLength <= RadiusSquared;
    public float GetRaytrace(Ray ray) => throw new NotImplementedException();
}
