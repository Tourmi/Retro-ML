namespace Retro_ML.NEAT.Utils;
public static class ActivationFunctions
{
    private static readonly Dictionary<string, Func<double, double>> activationFunctions = new()
    {
        ["Linear"] = x => x,
        ["ReLU"] = x => Math.Max(0, x),
        ["LeakyReLU"] = x => Math.Max(0.1 * x, x),
        ["Tanh"] = Math.Tanh
    };

    /// <summary>
    /// Returns the activation function with the given <paramref name="name"/>. If the <paramref name="name"/> does not exist or is null, returns a linear activation function
    /// </summary>
    public static Func<double, double> GetActivationFunction(string? name) => name is not null && activationFunctions.TryGetValue(name, out var activationFunction) ? activationFunction : activationFunctions["Linear"];

    /// <summary>
    /// Returns an enumerable of available activation functions
    /// </summary>
    public static IEnumerable<string> GetAvailableActivationFunctions() => activationFunctions.Keys;
}
