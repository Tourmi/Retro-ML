using SharpNeat.BlackBox;
using SharpNeat.Graphs;
using SharpNeat.Graphs.Acyclic;

namespace Retro_ML.Utils.SharpNeat
{
    /// <summary>
    /// Utility functions useful when dealing with the SharpNeat library
    /// </summary>
    public static class SharpNeatUtils
    {
        /// <summary>
        /// Returns an array equivalent to the given SharpNeat Vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static double[] VectorToArray(IVector<double> vector)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = vector[i];
            }

            return result;
        }

        /// <summary>
        /// Returns the connections separated by layers of the phenome
        /// </summary>
        /// <param name="phenome"></param>
        /// <returns></returns>
        public static (int sourceNode, int targetNode, double weight)[][] GetConnectionLayers(IBlackBox<double> phenome)
        {
            ConnectionIds connectionIds = phenome.GetField<ConnectionIds>("_connIds");
            double[] weights = phenome.GetField<double[]>("_weightArr");
            LayerInfo[] layerInfos = phenome.GetField<LayerInfo[]>("_layerInfoArr");

            var result = new (int sourceNode, int targetNode, double weight)[layerInfos.Length][];
            int currIndex = 0;
            for (int i = 0; i < result.Length; i++)
            {
                var layerInfo = layerInfos[i];
                result[i] = new (int sourceNode, int targetNode, double weight)[layerInfo.EndConnectionIdx - currIndex];
                int layerIndex = currIndex;
                for (; currIndex < layerInfo.EndConnectionIdx; currIndex++)
                {
                    result[i][currIndex - layerIndex] = (connectionIds.GetSourceId(currIndex), connectionIds.GetTargetId(currIndex), weights[currIndex]);
                }
            }

            return result;
        }
    }
}
