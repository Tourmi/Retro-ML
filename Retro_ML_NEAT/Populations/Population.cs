using Retro_ML.NEAT.Configuration;
using Retro_ML.NEAT.Creatures.Genotype;

namespace Retro_ML.NEAT.Populations;
internal class Population : IPopulation
{
    public Population()
    {
        AllGenomes = new();
        Species = new();
        PopulationExperiment = new();
    }

    public ExperimentSettings PopulationExperiment { get; set; }

    public List<Genome> AllGenomes { get; set; }
    IReadOnlyCollection<IGenome> IPopulation.AllGenomes => AllGenomes;

    public List<Species> Species { get; set; }
    IReadOnlyCollection<ISpecies> IPopulation.Species => Species;
}
