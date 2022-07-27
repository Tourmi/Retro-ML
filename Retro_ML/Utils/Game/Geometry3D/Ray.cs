namespace Retro_ML.Utils.Game.Geometry3D;
public struct Ray
{
    public readonly Vector P;
    public readonly Vector Direction;

    /// <summary>
    /// Assumes <paramref name="dir"/> is already normalized.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="dir"></param>
    public Ray(Vector p, Vector dir)
    {
        P = p;
        Direction = dir;

        cachedInverseDirection = null;
    }

    private Vector? cachedInverseDirection;
    public Vector InverseDirection
    {
        get
        {
            if (!cachedInverseDirection.HasValue) cachedInverseDirection = Direction.Inverse();
            return cachedInverseDirection.Value;
        }
    }

    public Ray RotateVertically(float radAngle) => new(P, Direction.RotateVertically(radAngle).Normalized());
    public Ray RotateXY(float radAngle) => new(P, Direction.RotateXY(radAngle).Normalized());
    public Ray RotateXZ(float radAngle) => new(P, Direction.RotateXZ(radAngle).Normalized());
    public Ray RotateYZ(float radAngle) => new(P, Direction.RotateYZ(radAngle).Normalized());

    public override string ToString() => $"{{\nPoint:{P};\nDirection:{Direction}}}";
}
