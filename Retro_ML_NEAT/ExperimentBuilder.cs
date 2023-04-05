using Newtonsoft.Json;
using Retro_ML.NEAT.Configuration;
using Retro_ML.NEAT.Creatures.Genotype;
using Retro_ML.NEAT.Creatures.Phenotype;
using Retro_ML.NEAT.Training;

namespace Retro_ML.NEAT;
public static class ExperimentBuilder
{
    public static IPopulationTrainer GetTrainer(string configPath, ExperimentSettings experimentSettings, Random randomSource)
    {
        var config = JsonConvert.DeserializeObject<NEATConfiguration>(File.ReadAllText(configPath))!;
        return new PopulationTrainer(config, experimentSettings, randomSource);
    }

    public static IPhenome GetPhenome(string genomeFilepath) => JsonConvert.DeserializeObject<Genome>(File.ReadAllText(genomeFilepath))!.GetPhenome();
}
