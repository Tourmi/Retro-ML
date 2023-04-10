using Newtonsoft.Json;

namespace Retro_ML.NEAT.Configuration;
public class NEATConfiguration
{
    public NEATConfiguration()
    {
        ReproductionConfig = new();
        SpeciesConfig = new();
        GenomeConfig = new();
    }

    public Reproduction ReproductionConfig { get; set; }
    public Species SpeciesConfig { get; set; }
    public Genome GenomeConfig { get; set; }

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static NEATConfiguration Deserialize(string json) => JsonConvert.DeserializeObject<NEATConfiguration>(json)!;

    public class Reproduction
    {
        public Reproduction()
        {
            TargetPopulation = 100;
            EliteSpeciesCount = 2;
            EliteGenomeCount = 1;

            PreReproductionRemoveRatio = 0.75;

            CrossoverOdds = 0.1;
            GeneRemainsDisabledOdds = 0.5;

            MutationIterations = 2;
            AdjustWeightsOdds = 0.45;
            WeightPerturbationOdds = 0.9;
            WeightPerturbationPercentRange = 0.1;
            MaximumWeightAmplitude = 5.0;

            MutationAddConnectionOdds = 0.1;
            MutationAddNodeOdds = 0.1;
        }

        public int TargetPopulation { get; set; }
        public int EliteSpeciesCount { get; set; }
        public int EliteGenomeCount { get; set; }

        public double PreReproductionRemoveRatio { get; set; }

        public double CrossoverOdds { get; set; }
        public double GeneRemainsDisabledOdds { get; set; }

        public int MutationIterations { get; set; }
        public double AdjustWeightsOdds { get; set; }
        public double WeightPerturbationOdds { get; set; }
        public double WeightPerturbationPercentRange { get; set; }
        public double MaximumWeightAmplitude { get; set; }
        public double MutationAddConnectionOdds { get; set; }
        public double MutationAddNodeOdds { get; set; }
    }

    public class Species
    {
        public Species()
        {
            SpeciesMaxDelta = 1.75;
            DeltaExcessGenesWeight = 1;
            DeltaDisjointGenesWeight = 1;
            MinimumGeneCountToNormalizeExcessDisjoint = 5;
            DeltaAverageWeightDifferenceWeight = 1;

            PruneAfterXGenerationsWithoutProgress = 20;
        }

        public double SpeciesMaxDelta { get; set; }
        public double DeltaExcessGenesWeight { get; set; }
        public double DeltaDisjointGenesWeight { get; set; }
        public int MinimumGeneCountToNormalizeExcessDisjoint { get; set; }
        public double DeltaAverageWeightDifferenceWeight { get; set; }

        public int PruneAfterXGenerationsWithoutProgress { get; set; }
    }

    public class Genome
    {
        public Genome()
        {
            InputActivationFunction = "Linear";
            HiddenActivationFunction = "LeakyReLU";
            OutputActivationFunction = "Tanh";
        }

        public string InputActivationFunction { get; set; }
        public string HiddenActivationFunction { get; set; }
        public string OutputActivationFunction { get; set; }
    }
}
