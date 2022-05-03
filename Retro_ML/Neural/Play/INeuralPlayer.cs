using System;

namespace Retro_ML.Neural.Play
{
    /// <summary>
    /// Takes care of the play mode of a neural network.
    /// </summary>
    public interface INeuralPlayer : IDisposable
    {
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
        /// Loads the genome at the given path.
        /// </summary>
        /// <param name="path"></param>
        bool LoadGenome(string path);
        /// <summary>
        /// The save state to load
        /// </summary>
        /// <param name="path"></param>
        void LoadState(string path);
    }
}
