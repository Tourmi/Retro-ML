namespace Retro_ML.NEAT.Training;
public class Fitness : IComparable<Fitness>
{
    private readonly double primaryFitness;
    private readonly double[] secondaryFitnesses;

    public Fitness(double fitness) : this(fitness, Array.Empty<double>()) { }

    public Fitness(double primaryFitness, params double[] secondaryFitnesses)
    {
        if (primaryFitness < 0) throw new ArgumentException("Fitness cannot be negative", nameof(primaryFitness));
        this.primaryFitness = primaryFitness;
        this.secondaryFitnesses = secondaryFitnesses;
    }

    public double Score => primaryFitness;
    public IReadOnlyCollection<double> SecondaryScores => secondaryFitnesses;

    public int CompareTo(Fitness? other)
    {
        int val = primaryFitness.CompareTo(other?.primaryFitness ?? 0);
        if (val != 0) return val;

        for (int i = 0; i < secondaryFitnesses.Length; i++)
        {
            if (i >= (other?.secondaryFitnesses.Length ?? 0)) return 0;

            val = secondaryFitnesses[i].CompareTo(other!.secondaryFitnesses[i]);
            if (val != 0) return val;
        }

        return 0;
    }

    public static Fitness Max(Fitness fitness1, Fitness fitness2) => fitness1.Score > fitness2.Score ? fitness1 : fitness2;
}
