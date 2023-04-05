using Retro_ML.NEAT.Creatures.Genotype;

namespace Retro_ML.NEAT.Populations;
public interface ISpecies
{
    /// <summary>
    /// Genomes that are part of this species
    /// </summary>
    IReadOnlyCollection<IGenome> Genomes { get; }
}
