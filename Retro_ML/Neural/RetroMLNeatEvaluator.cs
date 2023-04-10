using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.NEAT.Creatures.Phenotype;
using Retro_ML.NEAT.Training;
using Retro_ML.Utils.RetroMLNeat;

namespace Retro_ML.Neural;
public class RetroMLNeatEvaluator : BaseEvaluator, NEAT.Training.IEvaluator
{
    public RetroMLNeatEvaluator(ApplicationConfig appConfig,
                                  IPhenomeWrapper? phenome,
                                  IEnumerable<string> saveStates,
                                  EmulatorManager emulatorManager) : base(appConfig, phenome, saveStates, emulatorManager) { }

    public Fitness Evaluate(IPhenome phenome)
    {
        this.phenome = new PhenomeWrapper(phenome);
        var score = base.Evaluate();
        var fitness = new Fitness(score);
        return fitness;
    }
}
