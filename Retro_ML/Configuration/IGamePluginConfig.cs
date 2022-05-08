namespace Retro_ML.Configuration
{
    public interface IGamePluginConfig : IPluginConfig
    {
        /// <summary>
        /// Initializes the neural config
        /// </summary>
        /// <param name="neuralConfig"></param>
        void InitNeuralConfig(NeuralConfig neuralConfig);
    }
}
