using SMW_ML.Arduino;
using SMW_ML.Models.Config;
using SMW_ML.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SMW_ML.Emulator
{
    /// <summary>
    /// Class that takes care of instantiating and dealing with multi-threaded access to its emulators.
    /// </summary>
    internal class EmulatorManager
    {
        private const int SOCKET_PORT = 11000;
        private const int MAX_CONNECTIONS = 100;

        private readonly IEmulatorAdapter?[] adapters;
        private readonly bool[] adaptersTaken;

        private bool trainingMode;

        private readonly Semaphore sem;
        private Socket? server;
        private IPAddress? ipAddress;

        private ApplicationConfig applicationConfig;

        public EmulatorManager(ApplicationConfig appConfig) : this(appConfig.Multithread, appConfig)
        {
        }

        public EmulatorManager(int emulatorCount, ApplicationConfig appConfig)
        {
            applicationConfig = appConfig;

            this.adapters = new IEmulatorAdapter[emulatorCount];
            this.adaptersTaken = new bool[emulatorCount];

            sem = new Semaphore(1, 1);
        }

        /// <summary>
        /// Initializes the Emulator Manager, so it is now ready to be requested emulator instances.
        /// </summary>
        public void Init(bool trainingMode)
        {
            this.trainingMode = trainingMode;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList.Last();
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
                sem.WaitOne();

                var index = Array.IndexOf(adaptersTaken, false);

                if (index >= 0)
                {
                    InitEmulator(index);

                    chosen = adapters[index];
                    adaptersTaken[index] = true;
                }

                sem.Release();
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
            sem.WaitOne();

            var index = Array.IndexOf(adapters, adapter);
            adaptersTaken[index] = false;

            sem.Release();
        }

        /// <summary>
        /// Returns the first emulator of the manager. Bypasses waiting, so be careful with it, as it could be in use by another thread.
        /// </summary>
        /// <returns></returns>
        public IEmulatorAdapter GetFirstEmulator()
        {
            sem.WaitOne();
            InitEmulator(0);
            sem.Release();
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

            sem.WaitOne();

            for (int i = 0; i < adapters.Length; i++)
            {
                adapters[i]?.Dispose();
                adapters[i] = null;
                Thread.Sleep(100);
            }

            server?.Dispose();

            sem.Release();
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
                    applicationConfig.NeuralConfig);

                if (index == 0 && ArduinoPreviewer.ArduinoAvailable(applicationConfig.ArduinoCommunicationPort))
                {
                    adapters[0]!.SetArduinoPreviewer(new ArduinoPreviewer(applicationConfig.ArduinoCommunicationPort));
                }
            }
        }
    }
}
