using Retro_ML.NEAT.Creatures.Phenotype;
using Retro_ML.NEAT.Training;

namespace Retro_ML.NEAT.Creatures.Genotype;
public interface IGenome
{
    /// <summary>
    /// Total input node count in the genome
    /// </summary>
    public int InputNodeCount { get; }
    /// <summary>
    /// Total output node count in the genome
    /// </summary>
    public int OutputNodeCount { get; }
    /// <summary>
    /// Total node count in the genome
    /// </summary>
    public int TotalNodeCount { get; }

    /// <summary>
    /// Generates and returns the phenome for this genome
    /// </summary>
    /// <returns></returns>
    public IPhenome GetPhenome();
    /// <summary>
    /// Latest fitness of the genome
    /// </summary>
    public Fitness Fitness { get; }
}
