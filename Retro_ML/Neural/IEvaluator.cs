namespace Retro_ML.Neural;
public interface IEvaluator : IDisposable
{
    bool ShouldStop { get; set; }

    double Evaluate();
}
