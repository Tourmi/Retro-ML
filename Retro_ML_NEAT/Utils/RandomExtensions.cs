namespace Retro_ML.NEAT.Utils;
internal static class RandomExtensions
{
    public static T RandomFromList<T>(this Random rand, IList<T> collection) => collection[rand.Next(0, collection.Count)];
    public static T PickRandomFromWeightedList<T>(this Random rand, IList<T> collection, IList<double> weights, double totalWeights)
    {
        var chosen = rand.NextDouble() * totalWeights;
        double currWeight = 0;

        for (int i = 0; i < collection.Count; i++)
        {
            currWeight += weights[i];
            if (currWeight > chosen)
            {
                return collection[i];
            }
        }

        return collection.Last();
    }

    public static int RandomNode(this Random rand, int[] nodesDepth, int minimumDepth, int maximumDepth) =>
        rand.RandomFromList(nodesDepth
            .Select((nodeDepth, index) => (nodeDepth, index))
            .Where((x) => x.nodeDepth >= minimumDepth && x.nodeDepth <= maximumDepth)
            .ToList()).index;
}
