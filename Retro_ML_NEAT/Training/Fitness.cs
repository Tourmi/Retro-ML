namespace Retro_ML.NEAT.Training;
public readonly struct Fitness : IComparable<Fitness>, IEquatable<Fitness>
{
    private readonly double primaryFitness;
    private readonly double[] secondaryFitnesses;

    public static readonly Fitness Zero = new(0);

    public Fitness(double fitness) : this(fitness, Array.Empty<double>()) { }

    public Fitness(double primaryFitness, params double[] secondaryFitnesses)
    {
        if (primaryFitness < 0) throw new ArgumentException("Fitness cannot be negative", nameof(primaryFitness));
        this.primaryFitness = primaryFitness;
        this.secondaryFitnesses = secondaryFitnesses;
    }

    public double Score => primaryFitness;
    public IReadOnlyCollection<double> SecondaryScores => secondaryFitnesses;

    public static (Fitness max, Fitness min) MaxMin(Fitness fitness1, Fitness fitness2) => fitness1 >= fitness2 ? (fitness1, fitness2) : (fitness2, fitness1);

    #region Overrides
    public int CompareTo(Fitness other)
    {
        int val = primaryFitness.CompareTo(other.primaryFitness);
        if (val != 0) return val;

        for (int i = 0; i < secondaryFitnesses.Length; i++)
        {
            if (i >= (other.secondaryFitnesses.Length)) return 1;

            val = secondaryFitnesses[i].CompareTo(other.secondaryFitnesses[i]);
            if (val != 0) return val;
        }

        return other.secondaryFitnesses.Length > secondaryFitnesses.Length ? -1 : 0;
    }

    public bool Equals(Fitness other) => primaryFitness == other.primaryFitness && EqualityComparer<double[]>.Default.Equals(secondaryFitnesses, other.secondaryFitnesses);
    public override bool Equals(object? obj) => obj is Fitness fitness && Equals(fitness);
    public override int GetHashCode() => HashCode.Combine(primaryFitness, secondaryFitnesses);
    public override string? ToString() => $$"""{{primaryFitness}} {{{string.Join(", ", secondaryFitnesses)}}}""";
    #endregion

    #region Operators
    public static bool operator ==(Fitness left, Fitness right) => left.Equals(right);
    public static bool operator !=(Fitness left, Fitness right) => !(left == right);
    public static bool operator <(Fitness left, Fitness right) => left.CompareTo(right) < 0;
    public static bool operator <=(Fitness left, Fitness right) => left.CompareTo(right) <= 0;
    public static bool operator >(Fitness left, Fitness right) => left.CompareTo(right) > 0;
    public static bool operator >=(Fitness left, Fitness right) => left.CompareTo(right) >= 0;
    public static Fitness operator +(Fitness left, Fitness right)
    {
        var primaryFitness = left.primaryFitness + right.primaryFitness;
        var (longer, shorter) = left.secondaryFitnesses.Length > right.secondaryFitnesses.Length ? (left, right) : (right, left);
        var secondaryFitnesses = new double[longer.secondaryFitnesses.Length];
        Array.Copy(longer.secondaryFitnesses, secondaryFitnesses, longer.secondaryFitnesses.Length);
        for (int i = 0; i < shorter.secondaryFitnesses.Length; i++)
        {
            secondaryFitnesses[i] += shorter.secondaryFitnesses[i];
        }

        return new Fitness(primaryFitness, secondaryFitnesses);
    }
    public static Fitness operator +(Fitness fitness) => fitness;
    public static Fitness operator -(Fitness fitness) => new(-fitness.primaryFitness, fitness.secondaryFitnesses.Select(f => -f).ToArray());
    public static Fitness operator -(Fitness left, Fitness right) => left + -right;
    public static Fitness operator *(Fitness left, double right) => new(left.primaryFitness * right, left.secondaryFitnesses.Select(f => f * right).ToArray());
    public static Fitness operator *(double left, Fitness right) => right * left;
    public static Fitness operator /(Fitness left, double right) => left * (1 / right);
    #endregion
}
