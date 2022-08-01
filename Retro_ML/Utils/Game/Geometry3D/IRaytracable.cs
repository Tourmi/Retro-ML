namespace Retro_ML.Utils.Game.Geometry3D;
public interface IRaytracable
{
    /// <summary>
    /// Returns the distance at which the given <paramref name="ray"/> collides with this shape.
    /// Returns <see cref="float.NaN"/> if there is no collision
    /// </summary>
    float GetRaytrace(Ray ray);
    /// <summary>
    /// Returns true if this object contains the given point
    /// </summary>
    bool Contains(Vector vector);

    float MinX { get; }
    float MaxX { get; }
    float MinY { get; }
    float MaxY { get; }
    float MinZ { get; }
    float MaxZ { get; }

    /// <summary>
    /// Whether or not this object can move in the scene
    /// </summary>
    bool Static { get; }
    /// <summary>
    /// The bounding box of this object
    /// </summary>
    AABB AABB { get; }
}
