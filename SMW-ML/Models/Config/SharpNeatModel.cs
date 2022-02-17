namespace SMW_ML.Models.Config
{
    public class SharpNeatModel
    {
        public SharpNeatModel()
        {
            ActivationFnName = "LeakyReLU";

            EvolutionAlgorithmSettings = new EvolutionAlgorithmSettings();
            ReproductionAsexualSettings = new ReproductionAsexualSettings();
            ReproductionSexualSettings = new ReproductionSexualSettings();
            ComplexityRegulationStrategy = new ComplexityRegulationStrategy();

        }

        public bool IsAcyclic { get; set; }
        public string ActivationFnName { get; set; }

        public EvolutionAlgorithmSettings EvolutionAlgorithmSettings { get; set; }
        public ReproductionAsexualSettings ReproductionAsexualSettings { get; set; }
        public ReproductionSexualSettings ReproductionSexualSettings { get; set; }
        public ComplexityRegulationStrategy ComplexityRegulationStrategy { get; set; }

        public int PopulationSize { get; set; }
        public double InitialInterconnectionsProportion { get; set; }
        public double ConnectionWeightScale { get; set; }
        public int DegreeOfParallelism { get; set; }
        public bool EnableHardwareAcceleratedNeuralNets { get; set; }
        public bool EnableHardwareAcceleratedActivationFunctions { get; set; }
    }

    public class EvolutionAlgorithmSettings
    {
        public int SpeciesCount { get; set; }
        public double ElitismProportion { get; set; }
        public double SelectionProportion { get; set; }
        public double OffSpringAsexualProportion { get; set; }
        public double OffSpringSexualProportion { get; set; }
        public double InterspeciesMatingProportion { get; set; }
    }

    public class ReproductionAsexualSettings
    {
        public double ConnectionWeightMutationProbability { get; set; }
        public double AddNodeMutationProbability { get; set; }
        public double AddConnectionMutationProbability { get; set; }
        public double DeleteConnectionMutationProbability { get; set; }
    }

    public class ReproductionSexualSettings
    {
        public double SecondaryParentGeneProbability { get; set; }
    }

    public class ComplexityRegulationStrategy
    {
        public ComplexityRegulationStrategy()
        {
            StrategyName = "relative";
        }

        public string StrategyName { get; set; }
        public int RelativeComplexityCeiling { get; set; }
        public int MinSimplifcationGenerations { get; set; }

    }
}
