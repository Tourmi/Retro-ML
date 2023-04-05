namespace Retro_ML.NEAT.Creatures.Genotype;
internal class ConnectionGene
{
    public int InputNode { get; set; }
    public int OutputNode { get; set; }
    public double Weight { get; set; }
    public bool Enabled { get; set; }
    public long InnovationNumber { get; set; }

    internal ConnectionGene Copy() => new()
    {
        Weight = Weight,
        InnovationNumber = InnovationNumber,
        Enabled = Enabled,
        InputNode = InputNode,
        OutputNode = OutputNode
    };

    internal ConnectionGene WithWeight(double weight) => new()
    {
        Weight = weight,
        InnovationNumber = InnovationNumber,
        Enabled = Enabled,
        InputNode = InputNode,
        OutputNode = OutputNode
    };
}
