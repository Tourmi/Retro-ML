using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Plugin
{
    /// <summary>
    /// Interface to be implemented by all plugins
    /// </summary>
    public interface IGamePlugin : IPlugin
    {
        /// <summary>
        /// The human readable name of the game this plugin is for.
        /// </summary>
        string PluginGameName { get; }
        /// <summary>
        /// <br>The expected 21 bytes game name in the ROM header at positions 0x7FC0 or 0xFFC0 for SNES games.</br>
        /// <br>Example (Replace dashes by spaces) : "<c>SUPER MARIOWORLD-----</c>"</br>
        /// <br>If this is shorter than 21 bytes, it is assumed that the rest of the bytes are spaces (0x20)</br>
        /// </summary>
        string PluginROMHeaderName { get; }
        /// <summary>
        /// The exact name of the console plugin this game uses.
        /// </summary>
        string ConsolePluginName { get; }

        /// <summary>
        /// Returns the <see cref="IDataFetcherFactory"/> specific to this plugin
        /// </summary>
        IDataFetcherFactory GetDataFetcherFactory();
        /// <summary>
        /// Returns a new neural player specific to this plugin
        /// </summary>
        /// <returns></returns>
        INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig);
        /// <summary>
        /// Returns a new neural trainer specific to this plugin
        /// </summary>
        /// <returns></returns>
        INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig);
    }
}
