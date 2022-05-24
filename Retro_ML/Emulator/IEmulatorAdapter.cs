using Retro_ML.Arduino;
using Retro_ML.Game;

namespace Retro_ML.Emulator
{
    public interface IEmulatorAdapter : IDisposable
    {
        /// <summary>
        /// Event that's triggered whenever the neural network using this emulator is activated (usually once per frame.)
        /// </summary>
        event Action<double[], double[]>? LinkedNetworkActivated;
        /// <summary>
        /// Event that's triggered whenever the neural network that's linked to this emulator changes.
        /// </summary>
        event Action<(int sourceNode, int targetNode, double weight)[][], int[]>? ChangedLinkedNetwork;

        /// <summary>
        /// Reads the game memory at the given address
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        byte ReadMemory(uint addr);
        /// <summary>
        /// Reads the game memory starting from the given address
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        byte[] ReadMemory(uint addr, uint count);
        /// <summary>
        /// Reads the game memory from the given addresses
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        byte[] ReadMemory(params (uint addr, uint count)[] ranges);
        /// <summary>
        /// Sends the given input to the emulator
        /// </summary>
        /// <param name="input"></param>
        void SendInput(IInput input);
        /// <summary>
        /// Advances the emulator by one frame
        /// </summary>
        void NextFrame();
        /// <summary>
        /// Advances the emulator by <paramref name="frameCount"/>. <paramref name="repeatInput"/> repeats the input for every frame if set to true, otherwise, the inputs will be released after the first frame.
        /// </summary>
        /// <param name="frameCount">Advances the emulator by this amount of frames.</param>
        /// <param name="repeatInput">Repeats the input for every frame if set to true, otherwise, the inputs will be released after the first frame.</param>
        void NextFrames(int frameCount, bool repeatInput);
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
        /// Sets the arduino previewer to use with the adapter
        /// </summary>
        /// <param name="arduinoPreviewer"></param>
        internal void SetArduinoPreviewer(ArduinoPreviewer arduinoPreviewer);

        /// <summary>
        /// Called whenever both the inputs and outputs in the network changed
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        void NetworkUpdated(double[] inputs, double[] outputs);
        /// <summary>
        /// Called whenever the trained network is changed.
        /// </summary>
        /// <param name="connectionLayers"></param>
        /// <param name="outputIds"></param>
        void NetworkChanged((int sourceNode, int targetNode, double weight)[][] connectionLayers, int[] outputIds);

        /// <summary>
        /// Returns the data fetcher linked to this emulator.
        /// </summary>
        /// <returns></returns>
        public IDataFetcher GetDataFetcher();
        /// <summary>
        /// Returns the Input Setter related to this emulator
        /// </summary>
        /// <returns></returns>
        public InputSetter GetInputSetter();
        /// <summary>
        /// Returns the Output Getter related to this emulator.
        /// </summary>
        /// <returns></returns>
        public OutputGetter GetOutputGetter();
    }
}
