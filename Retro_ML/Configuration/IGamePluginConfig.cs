using Retro_ML.Neural.Scoring;

namespace Retro_ML.Configuration
{
    public interface IGamePluginConfig : IPluginConfig
    {
        /// <summary>
        /// Initializes the neural config
        /// </summary>
        /// <param name="neuralConfig"></param>
        void InitNeuralConfig(NeuralConfig neuralConfig);
        /// <summary>
        /// The score factors used to determine the score of training sessions.
        /// </summary>
        List<IScoreFactor> ScoreFactors { get; }
    }
}
