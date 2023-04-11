using Retro_ML.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.Utils.SharpNeat;
public class PhenomeWrapper : IPhenomeWrapper
{
    private readonly IBlackBox<double> blackbox;

    public PhenomeWrapper(IBlackBox<double> blackbox)
    {
        this.blackbox = blackbox;
    }

    public INeuralWrapper InputNodes => new VectorWrapper(blackbox.InputVector);

    public INeuralWrapper OutputNodes => new VectorWrapper(blackbox.OutputVector);

    public void Activate() => blackbox.Activate();
    public void ResetState() => blackbox.ResetState();
    public ((int input, int output, double weight)[][], int[] outputIds) GetConnectionLayers()
    {
        int[] outputMap = new int[blackbox!.OutputCount];
        Array.Copy(blackbox.OutputVector.GetField<int[]>("_map"), outputMap, blackbox.OutputCount);

        return (SharpNeatUtils.GetConnectionLayers(blackbox), outputMap);
    }
}
