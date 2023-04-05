using Retro_ML.NEAT.Creatures.Genotype;

namespace Retro_ML.NEAT.Populations;
public interface IPopulation
{
    /// <summary>
    /// Returns all of the genomes of the population
    /// </summary>
    IReadOnlyCollection<IGenome> AllGenomes { get; }
    /// <summary>
    /// Returns the current species of the population
    /// </summary>
    IReadOnlyCollection<ISpecies> Species { get; }
}
