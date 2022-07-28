using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML.Utils.Game;
public static class Raycast3D
{
    /// <summary>
    /// Returns <c><paramref name="verticalRayCount"/> * <paramref name="horizontalRayCount"/></c> ray distances, 
    /// normalized with <paramref name="viewDistance"/>, 1 being right next to the triangle, 0 if further than the distance. 
    /// between the angles <paramref name="verticalViewAngle"/> and <paramref name="horizontalViewAngle"/>,
    /// offset with the <paramref name="forward"/> <see cref="Ray"/>, based on the given <paramref name="shapes"/>
    /// </summary>
    public static double[,] GetRayDistances(int verticalRayCount, int horizontalRayCount, float verticalViewAngle, float horizontalViewAngle, float viewDistance, Ray forward, IEnumerable<IRaytracable> shapes)
    {
        double[,] rayDistances = new double[verticalRayCount, horizontalRayCount];
        verticalViewAngle *= MathF.Tau;
        horizontalViewAngle *= MathF.Tau;
        float verticalIncrement = verticalViewAngle / (verticalRayCount - 1);
        float horizontalIncrement = horizontalViewAngle / (horizontalRayCount - 1);

        for (int i = 0; i < rayDistances.GetLength(0); i++)
        {
            Ray currRow = forward.RotateVertically(verticalViewAngle / 2f - i * verticalIncrement);
            for (int j = 0; j < rayDistances.GetLength(1); j++)
            {
                var currRay = currRow.RotateXZ(-horizontalViewAngle / 2f + j * horizontalIncrement);
                rayDistances[i, j] = Math.Max(0, 1 - (shapes.Select(s => s.GetRaytrace(currRay)).Where(float.IsFinite).Append(float.PositiveInfinity).Min() / viewDistance));
            }
        }

        return rayDistances;
    }
}
