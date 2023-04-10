namespace Retro_ML.NEAT.Training;
public class Statistics
{
    public int GenerationCount { get; internal set; }
    public double BestGenomeFitness { get; internal set; }
    public double BestSpeciesAverageFitness { get; internal set; }
    public double AverageFitness { get; internal set; }
    public int BestGenomeComplexity { get; internal set; }
    public double AverageGenomeComplexity { get; internal set; }
    public int MaximumGenomeComplexity { get; internal set; }
    public int TotalSpecies { get; internal set; }
    public int BestSpeciesPopulation { get; internal set; }
    public double AverageSpeciesPopulation { get; internal set; }
}
