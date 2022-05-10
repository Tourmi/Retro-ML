using Retro_ML.Game;

namespace Retro_ML.Plugin
{
    /// <summary>
    /// Interface to be implemented to add new supported consoles to the application
    /// </summary>
    public interface IConsolePlugin : IPlugin
    {
        /// <summary>
        /// The human readable name of the console this plugin is for
        /// </summary>
        string ConsoleName { get; }
        /// <summary>
        /// The possible filename extensions for the ROM files of this console
        /// </summary>
        string[] ROMExtensions { get; }
        /// <summary>
        /// Returns a new instance of an input for this console
        /// </summary>
        /// <returns></returns>
        IInput GetInput();
    }
}
