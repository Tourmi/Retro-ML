using Retro_ML.Configuration;
using Retro_ML.Neural;
using SharpNeat.BlackBox;

namespace Retro_ML.Game
{
    /// <summary>
    /// Gets the state of the output neurons of the neural network, and parses it to a controller input.
    /// </summary>
    public class OutputGetter
    {
        private readonly List<OutputNode> outputNodes;
        private readonly ApplicationConfig config;
        private readonly NeuralConfig neuralConfig;

        private const bool ALLOW_OPPOSITE_DIRECTIONS = false;

        private const double ACTIVATION_THRESHOLD = 0;

        public OutputGetter(ApplicationConfig config)
        {
            this.config = config;
            neuralConfig = config.NeuralConfig;
            outputNodes = neuralConfig.OutputNodes;
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

            return input;
        }
    }
}
