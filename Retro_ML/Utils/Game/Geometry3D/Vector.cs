namespace Retro_ML.Utils.Game.Geometry3D;
public struct Vector
{
    private const float EPSILON = 0.0001f;

    private float? cachedSquaredLength;
    private float? cachedLength;

    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public Vector(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;

        cachedSquaredLength = null;
        cachedLength = null;
    }

    public Vector() : this(0, 0, 0) { }
    public Vector(float val) : this(val, val, val) { }

    public Vector WithX(float xVal) => new(xVal, Y, Z);
    public Vector WithY(float yVal) => new(X, yVal, Z);
    public Vector WithZ(float zVal) => new(X, Y, zVal);
    public Vector WithXY(float xVal, float yVal) => new(xVal, yVal, Z);
    public Vector WithXZ(float xVal, float zVal) => new(xVal, Y, zVal);
    public Vector WithYZ(float yVal, float zVal) => new(X, yVal, zVal);
    public Vector Inverse() => new(1f / X, 1f / Y, 1f / Z);
    public Vector Absolute() => new(MathF.Abs(X), MathF.Abs(Y), MathF.Abs(Z));

    /// <summary>
    /// Rotates the vector vertically based on the given angle
    /// </summary>
    /// <param name="radAngle"></param>
    /// <returns></returns>
    public Vector RotateVertically(float radAngle)
    {
        if (MathF.Abs(X) + MathF.Abs(Z) < EPSILON)
            return this;

        var norm = WithY(0).Normalized();
        var dot = norm.X;
        var det = -norm.Z;

        var oldHorizontalAngle = MathF.Atan2(det, dot);

        var step1 = RotateXZ(oldHorizontalAngle);
        var step2 = step1.RotateXY(radAngle);
        var step3 = step2.RotateXZ(-oldHorizontalAngle);

        return step3;
    }

    /// <summary>
    /// Returns a rotated <see cref="Ray"/> along the XY plane
    /// </summary>
    /// <param name="radAngle">An angle in radians.</param>
    public Vector RotateXY(float radAngle)
    {
        (float sin, float cos) = MathF.SinCos(radAngle);

        return WithXY(cos * X - sin * Y, sin * X + cos * Y);
    }

    /// <summary>
    /// Returns a rotated <see cref="Ray"/> along the XZ plane
    /// </summary>
    /// <param name="radAngle">An angle in radians.</param>
    public Vector RotateXZ(float radAngle)
    {
        (float sin, float cos) = MathF.SinCos(radAngle);

        return WithXZ(cos * X - sin * Z, sin * X + cos * Z);
    }

    /// <summary>
    /// Returns a rotated <see cref="Ray"/> along the YZ plane
    /// </summary>
    /// <param name="radAngle">An angle in radians.</param>
    public Vector RotateYZ(float radAngle)
    {
        (float sin, float cos) = MathF.SinCos(radAngle);

        return WithYZ(cos * Y - sin * Z, sin * Y + cos * Z);
    }

    public float AngleBetween(Vector other) => MathF.Acos(Normalized().Dot(other.Normalized()));

    /// <summary>
    /// Returns a normalized <see cref="Vector"/>. If the vector's length is zero, returns default vector.
    /// </summary>
    /// <returns></returns>
    public Vector Normalized() => (SquaredLength is 0f or (< 1 + EPSILON and > 1 - EPSILON)) ? this : this / Length;

    public float Length
    {
        get
        {
            if (!cachedLength.HasValue) cachedLength = MathF.Sqrt(SquaredLength);
            return cachedLength.Value;
        }
    }

    public float SquaredLength
    {
        get
        {
            if (!cachedSquaredLength.HasValue) cachedSquaredLength = X * X + Y * Y + Z * Z;
            return cachedSquaredLength.Value;
        }
    }

    public float Dot(Vector other) => X * other.X + Y * other.Y + Z * other.Z;
    public Vector Cross(Vector other) => new(Y * other.Z - other.Y * Z,
                                             Z * other.X - other.Z * X,
                                             X * other.Y - other.X * Y);

    public override string ToString() => $"\n{{\n    X:{X};\n    Y:{Y};\n    Z:{Z}\n}}";

    public static readonly Vector Origin = new(0);

    public static Vector operator +(Vector a) => a;
    public static Vector operator +(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector operator +(Vector a, float val) => new(a.X + val, a.Y + val, a.Z + val);
    public static Vector operator -(Vector a) => new(-a.X, -a.Y, -a.Z);
    public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector operator -(Vector a, float val) => new(a.X - val, a.Y - val, a.Z - val);
    public static Vector operator *(float val, Vector a) => new(a.X * val, a.Y * val, a.Z * val);
    public static Vector operator /(Vector a, float val)
    {
        float inv = 1f / val;
        return new(a.X * inv, a.Y * inv, a.Z * inv);
    }
}
