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

    public class Reproduction
    {
        public Reproduction()
        {
            TargetPopulation = 100;

            CrossoverOdds = 0.1;
            EliteSpeciesCount = 2;
            EliteGenomeCount = 1;

            RemoveRatio = 0.75;

            GeneRemainsDisabledOdds = 0.5;
            AdjustWeightsOdds = 0.8;
            WeightPerturbationOdds = 0.9;
            WeightPerturbationPercentRange = 0.1;
            MaximumWeightAmplitude = 5.0;

            MutationAddConnectionOdds = 0.1;
            MutationAddNodeOdds = 0.1;
        }

        public int TargetPopulation { get; set; }
        public int EliteSpeciesCount { get; set; }
        public int EliteGenomeCount { get; set; }

        public double RemoveRatio { get; set; }

        public double CrossoverOdds { get; set; }
        public double GeneRemainsDisabledOdds { get; set; }

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
            SpeciesMaxDelta = 10;
            DeltaExcessGenesWeight = 1;
            DeltaDisjointGenesWeight = 1;
            MinimumGeneCountToNormalizeExcessDisjoint = 20;
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
            HiddenActivationFunction = "ReLU";
            OutputActivationFunction = "Tanh";
        }

        public string InputActivationFunction { get; set; }
        public string HiddenActivationFunction { get; set; }
        public string OutputActivationFunction { get; set; }
    }
}
