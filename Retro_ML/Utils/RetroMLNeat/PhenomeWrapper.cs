using Retro_ML.NEAT.Creatures.Phenotype;
using Retro_ML.Neural;

namespace Retro_ML.Utils.RetroMLNeat;
public class PhenomeWrapper : IPhenomeWrapper
{
    private readonly IPhenome phenome;

    public PhenomeWrapper(IPhenome phenome)
    {
        this.phenome = phenome;
    }

    public INeuralWrapper InputNodes => new NeuralWrapper(phenome.Inputs);

    public INeuralWrapper OutputNodes => new NeuralWrapper(phenome.Outputs);

    public void Activate() => phenome.Activate();
    public void ResetState() { }
}
