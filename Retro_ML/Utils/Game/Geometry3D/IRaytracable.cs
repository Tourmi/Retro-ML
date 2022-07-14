namespace Retro_ML.Utils.Game.Geometry3D;
public interface IRaytracable
{
    /// <summary>
    /// Returns the distance at which the given <paramref name="ray"/> collides with this shape.
    /// Returns <see cref="float.NaN"/> if there is no collision
    /// </summary>
    public float GetRaytrace(Ray ray);
}
