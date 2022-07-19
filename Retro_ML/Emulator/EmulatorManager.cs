using Retro_ML.Arduino;
using Retro_ML.Configuration;
using Retro_ML.Game;
using Retro_ML.Utils;
using System.Net;
using System.Net.Sockets;

namespace Retro_ML.Emulator
{
    /// <summary>
    /// Class that takes care of instantiating and dealing with multi-threaded access to its emulators.
    /// </summary>
    public class EmulatorManager
    {
        private const int SOCKET_PORT = 11000;
        private const int MAX_CONNECTIONS = 100;

        private readonly IEmulatorAdapter?[] adapters;
        private readonly bool[] adaptersTaken;

        private bool trainingMode;

        private readonly Mutex mutex;
        private Socket? server;
        private IPAddress? ipAddress;
        private IDataFetcherFactory dataFetcherFactory;

        private ApplicationConfig applicationConfig;

        public EmulatorManager(ApplicationConfig appConfig, IDataFetcherFactory dataFetcherFactory) : this(appConfig.Multithread, appConfig, dataFetcherFactory)
        {
        }

        public EmulatorManager(int emulatorCount, ApplicationConfig appConfig, IDataFetcherFactory dataFetcherFactory)
        {
            applicationConfig = appConfig;

            this.adapters = new IEmulatorAdapter[emulatorCount];
            this.adaptersTaken = new bool[emulatorCount];
            this.dataFetcherFactory = dataFetcherFactory;

            mutex = new Mutex();
        }

        /// <summary>
        /// Initializes the Emulator Manager, so it is now ready to be requested emulator instances.
        /// </summary>
        public void Init(bool trainingMode)
        {
            this.trainingMode = trainingMode;

            ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new(ipAddress, SOCKET_PORT);

            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(localEndPoint);
            server.Listen(MAX_CONNECTIONS);
        }

        /// <summary>
        /// Returns an emulator instance. If none are available, waits until one becomes available.
        /// </summary>
        /// <returns></returns>
        public IEmulatorAdapter WaitOne()
        {
            IEmulatorAdapter? chosen = null;

            while (chosen == null)
            {
                mutex.WaitOne();

                var index = Array.IndexOf(adaptersTaken, false);

                if (index >= 0)
                {
                    InitEmulator(index);

                    chosen = adapters[index];
                    adaptersTaken[index] = true;
                }

                mutex.ReleaseMutex();
                Thread.Sleep(100);
            }

            return chosen;
        }

        /// <summary>
        /// Frees up the emulator so that other threads may now use it. Make sure to not use the emulator after calling this.
        /// </summary>
        /// <param name="adapter"></param>
        public void FreeOne(IEmulatorAdapter adapter)
        {
            mutex.WaitOne();

            var index = Array.IndexOf(adapters, adapter);
            adaptersTaken[index] = false;

            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Returns the first emulator of the manager. Bypasses waiting, so be careful with it, as it could be in use by another thread.
        /// </summary>
        /// <returns></returns>
        public IEmulatorAdapter GetFirstEmulator()
        {
            mutex.WaitOne();
            InitEmulator(0);
            mutex.ReleaseMutex();
            return adapters[0]!;
        }

        /// <summary>
        /// Only call this once training has fully stopped. Closes the emulators once they've all been freed.
        /// </summary>
        public void Clean()
        {
            while (adaptersTaken.Any(a => a))
            {
                Thread.Sleep(100);
            }

            mutex.WaitOne();

            for (int i = 0; i < adapters.Length; i++)
            {
                adapters[i]?.Dispose();
                adapters[i] = null;
                Thread.Sleep(100);
            }

            server?.Dispose();
            server = null;

            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Takes care of booting an emulator instance, if it does not exist at the given index.
        /// </summary>
        /// <param name="index"></param>
        private void InitEmulator(int index)
        {
            if (adapters[index] == null)
            {
                adapters[index] = new BizhawkAdapter(pathToEmulator: DefaultPaths.EMULATOR,
                    pathToLuaScript: DefaultPaths.EMULATOR_ADAPTER,
                    pathToROM: applicationConfig.RomPath,
                    pathToBizhawkConfig: trainingMode ? DefaultPaths.EMULATOR_CONFIG : DefaultPaths.EMULATOR_PLAY_CONFIG,
                    savestatesPath: DefaultPaths.SAVESTATES_DIR,
                    socketIP: ipAddress!.ToString(),
                    socketPort: SOCKET_PORT.ToString(),
                    server!,
                    applicationConfig,
                    dataFetcherFactory);

                if (index == 0 && ArduinoPreviewer.ArduinoAvailable(applicationConfig.ArduinoCommunicationPort))
                {
                    adapters[0]!.SetArduinoPreviewer(new ArduinoPreviewer(applicationConfig.ArduinoCommunicationPort));
                }
            }
        }
    }
}
