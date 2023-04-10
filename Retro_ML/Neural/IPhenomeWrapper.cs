namespace Retro_ML.Neural;
public interface IPhenomeWrapper
{
    INeuralWrapper InputNodes { get; }
    INeuralWrapper OutputNodes { get; }
    void ResetState();
    void Activate();
}
