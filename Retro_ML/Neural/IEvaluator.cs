namespace Retro_ML.Neural;
public interface IEvaluator
{
    bool ShouldStop { get; set; }

    double Evaluate();
}
