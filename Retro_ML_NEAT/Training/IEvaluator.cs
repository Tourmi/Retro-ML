using Retro_ML.NEAT.Creatures.Phenotype;

namespace Retro_ML.NEAT.Training;

public interface IEvaluator : IDisposable
{
    /// <summary>
    /// Evaluates a single phenome and returns its Fitness
    /// </summary>
    /// <param name="phenome"></param>
    /// <returns></returns>
    public Fitness Evaluate(IPhenome phenome);
}
