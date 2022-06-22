using Retro_ML.Configuration;
using Retro_ML.Neural;
using Retro_ML.Neural.Memory;
using SharpNeat.BlackBox;

namespace Retro_ML.Game;

/// <summary>
/// Gets the state of the output neurons of the neural network, and parses it to a controller input.
/// </summary>
public class OutputGetter
{
    private readonly List<OutputNode> outputNodes;
    private readonly ApplicationConfig config;
    private readonly NeuralConfig neuralConfig;
    private readonly NeuralMemory neuralMemory;

    public OutputGetter(ApplicationConfig config, NeuralMemory neuralMemory)
    {
        this.config = config;
        neuralConfig = config.NeuralConfig;
        outputNodes = neuralConfig.OutputNodes;
        this.neuralMemory = neuralMemory;
    }
    /// <summary>
    /// Gets the state of the output neurons of the neural network, and parses it to a controller input.
    /// </summary>
    /// <param name="outputs"></param>
    /// <returns></returns>
    public IInput GetControllerInput(IVector<double> outputs)
    {
        int currIndex = 0;
        int controllerIndex = 0;
        IInput input = config.GetConsolePlugin().GetInput();

        int outputCount = neuralConfig.GetOutputCount();

        for (; currIndex < outputCount && controllerIndex < input.ButtonCount; currIndex++, controllerIndex++)
        {
            if (!outputNodes[controllerIndex].ShouldUse)
            {
                currIndex--;
                continue;
            }

            input.SetButton(controllerIndex, outputs[currIndex]);
        }

        input.ValidateButtons();

        neuralMemory.WriteMemory(outputs, currIndex);

        return input;
    }
}
