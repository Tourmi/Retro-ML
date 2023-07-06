namespace Retro_ML.NEAT.Creatures.Genotype;
internal struct ConnectionGene
{
    public int InputNode { get; set; }
    public int OutputNode { get; set; }
    public double Weight { get; set; }
    public bool Enabled { get; set; }
    public long InnovationNumber { get; set; }

    internal ConnectionGene WithWeight(double weight)
    {
        var c = this;
        c.Weight = weight;
        return c;
    }
}
