namespace Retro_ML.NEAT.Training;
public interface IPopulationTrainer
{
    /// <summary>
    /// Whether or not the trainer was initialized
    /// </summary>
    public bool IsInitialized { get; }

    /// <summary>
    /// Initializes the trainer before training
    /// </summary>
    /// <param name="evaluatorGenerator">A generator function that allows evaluating phenomes</param>
    public void Initialize(Func<IEvaluator> evaluatorGenerator);
    /// <summary>
    /// Performs 1 generation, first trimming and repoducing the genomes, then evaluates their phenomes.
    /// </summary>
    public void RunOneGeneration();

    /// <summary>
    /// Loads the given population to train
    /// </summary>
    public void LoadPopulation(string filepath);
    /// <summary>
    /// Saves the population to the given filepath
    /// </summary>
    public void SavePopulation(string filepath);

}
