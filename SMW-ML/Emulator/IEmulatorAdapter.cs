using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Emulator
{
    public interface IEmulatorAdapter : IDisposable
    {
        /// <summary>
        /// Reads the game memory at the given address
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        byte ReadMemory(int addr);
        /// <summary>
        /// Reads the game memory starting from the given address
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        byte[] ReadMemory(int addr, int count);
        /// <summary>
        /// Sends the given input to the emulator
        /// </summary>
        /// <param name="input"></param>
        void SendInput(Input input);
        /// <summary>
        /// Advances the emulator by one frame
        /// </summary>
        void NextFrame();
        /// <summary>
        /// Loads the given ROM
        /// </summary>
        /// <param name="path"></param>
        void LoadRom(string path);
        /// <summary>
        /// Returns the list of available save states
        /// </summary>
        /// <returns></returns>
        string[] GetStates();
        /// <summary>
        /// Loads the specified save state
        /// </summary>
        /// <param name="saveState"></param>
        void LoadState(string saveState);
        /// <summary>
        /// Reserves the emulator instance
        /// </summary>
        void Reserve();
        /// <summary>
        /// Frees up the emulator instance
        /// </summary>
        void Free();
    }
}
