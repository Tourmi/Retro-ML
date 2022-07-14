namespace Retro_ML.Utils.Game.Geometry3D;
public struct Ray
{
    private const float EPSILON = 0.00001f;

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
    }

    public Ray RotateVertically(float radAngle) => new(P, Direction.RotateVertically(radAngle).Normalized());
    public Ray RotateXY(float radAngle) => new(P, Direction.RotateXY(radAngle).Normalized());
    public Ray RotateXZ(float radAngle) => new(P, Direction.RotateXZ(radAngle).Normalized());
    public Ray RotateYZ(float radAngle) => new(P, Direction.RotateYZ(radAngle).Normalized());

    public override string ToString() => $"{{\nPoint:{P};\nDirection:{Direction}}}";

    public static Ray operator +(Ray a) => a;
    public static Ray operator +(Ray a, Vector b) => new(a.P + b, a.Direction);
    public static Ray operator -(Ray a) => new(-a.P, -a.Direction);
    public static Ray operator -(Ray a, Vector b) => new(a.P - b, a.Direction);
    public static Vector operator *(float b, Ray a) => a.P + b * a.Direction;
    public static Vector operator /(Ray a, float b) => a.P + a.Direction / b;
}
