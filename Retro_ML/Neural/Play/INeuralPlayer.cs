namespace Retro_ML.Neural.Play
{
    /// <summary>
    /// Takes care of the play mode of a neural network.
    /// </summary>
    public interface INeuralPlayer : IDisposable
    {
        /// <summary>
        /// Called once the player goes through all save states and all genomes
        /// </summary>
        event Action? FinishedPlaying;

        /// <summary>
        /// Whether or not the player is currently running
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// Starts a play instance
        /// </summary>
        void StartPlaying();
        /// <summary>
        /// Stops playing. Can safely dispose the player after calling this.
        /// </summary>
        void StopPlaying();
        /// <summary>
        /// Loads the genomes at the given paths.
        /// </summary>
        /// <param name="paths"></param>
        bool LoadGenomes(string[] paths);
        /// <summary>
        /// The save states to load
        /// </summary>
        /// <param name="paths"></param>
        void LoadStates(string[] paths);
    }
}
