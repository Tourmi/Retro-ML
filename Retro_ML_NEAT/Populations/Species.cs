using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Training;

namespace Retro_ML.NEAT.Populations;
internal class Species : ISpecies
{
    public Species()
    {
        Genomes = new List<Genome>();
        Representative = new();

        AdjustedFitnessSum = new Fitness(double.MinValue);
        BestFitness = new Fitness(double.MinValue);
    }

    public List<Genome> Genomes { get; set; }
    IReadOnlyCollection<IGenome> ISpecies.Genomes => Genomes;

    /// <summary>
    /// The representative changes to a new random creature from the previous generation that was part of the species
    /// </summary>
    public Genome Representative { get; set; }

    /// <summary>
    /// When this species was created
    /// </summary>
    public int GensSinceLastProgress { get; set; }

    /// <summary>
    /// Sum of all the adjusted fitnesses in the species
    /// </summary>
    public Fitness AdjustedFitnessSum { get; set; }
    public Fitness BestFitness { get; set; }

    public void SortGenomes() => Genomes = Genomes.OrderByDescending(g => g.Fitness).ToList();
}
