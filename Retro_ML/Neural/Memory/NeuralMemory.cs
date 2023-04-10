namespace Retro_ML.Neural.Memory;
/// <summary>
/// Class that deals with a single creature's memory cells. They can be short term, long term, or permanent
/// </summary>
public class NeuralMemory
{
    private readonly double[] shortTerm;
    private readonly double[] longTerm;
    private readonly double[] permanent;
    private readonly double maxValue;

    public NeuralMemory(int shortTermMemoryNodes, int longTermMemoryNodes, int permanentMemoryNodes, double maximumValue)
    {
        shortTerm = new double[shortTermMemoryNodes];
        longTerm = new double[longTermMemoryNodes];
        permanent = new double[permanentMemoryNodes];
        maxValue = maximumValue;

        Reset();
    }

    /// <summary>
    /// Commits to memory the current neural network output, depending on the <paramref name="nodes"/> states, starting from <paramref name="offset"/>
    /// </summary>
    public void WriteMemory(INeuralWrapper nodes, int offset)
    {
        for (int i = 0; i < shortTerm.Length; i++)
        {
            shortTerm[i] = Math.Clamp(nodes[offset + i], -maxValue, maxValue);
        }
        offset += shortTerm.Length;

        for (int i = 0; i < longTerm.Length; i++)
        {
            longTerm[i] = Math.Clamp(nodes[offset + i * 2] > 0.5 ? nodes[offset + i * 2 + 1] : longTerm[i], -maxValue, maxValue);
        }
        offset += longTerm.Length * 2;

        for (int i = 0; i < permanent.Length; i++)
        {
            //only write to permanent memory if the current value is NaN, and the AI wants to write to it. 
            permanent[i] = Math.Clamp(nodes[offset + i] > 0.5 && double.IsNaN(permanent[i]) ? nodes[offset + i + permanent.Length] : permanent[i], -maxValue, maxValue);
        }
    }

    /// <summary>
    /// Reads from the memory, into the given <paramref name="nodes"/>, starting from <paramref name="offset"/>
    /// </summary>
    public void SetMemory(INeuralWrapper nodes, int offset)
    {
        for (int i = 0; i < shortTerm.Length; i++)
        {
            nodes[offset + i] = shortTerm[i];
        }
        offset += shortTerm.Length;

        for (int i = 0; i < longTerm.Length; i++)
        {
            nodes[offset + i] = longTerm[i];
        }
        offset += longTerm.Length;

        for (int i = 0; i < permanent.Length; i++)
        {
            nodes[offset + i] = double.IsNaN(permanent[i]) ? 0.0 : permanent[i];
        }
    }

    public void Reset()
    {
        for (int i = 0; i < shortTerm.Length; i++)
        {
            shortTerm[i] = 0.0;
        }
        for (int i = 0; i < longTerm.Length; i++)
        {
            longTerm[i] = 0.0;
        }
        for (int i = 0; i < permanent.Length; i++)
        {
            permanent[i] = double.NaN;
        }
    }
}
