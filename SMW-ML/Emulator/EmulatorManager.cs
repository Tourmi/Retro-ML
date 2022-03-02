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
    internal class EmulatorManager
    {
        private const int SOCKET_PORT = 11000;
        private const int MAX_CONNECTIONS = 100;

        private readonly IEmulatorAdapter?[] adapters;
        private readonly bool[] adaptersTaken;

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

        public void Init()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList.Last();
            IPEndPoint localEndPoint = new(ipAddress, SOCKET_PORT);

            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(localEndPoint);
            server.Listen(MAX_CONNECTIONS);

            if (ArduinoPreviewer.ArduinoAvailable(applicationConfig.ArduinoCommunicationPort))
            {
                adapters[0]!.SetArduinoPreviewer(new ArduinoPreviewer(applicationConfig.ArduinoCommunicationPort));
            }
        }

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

        public void FreeOne(IEmulatorAdapter adapter)
        {
            sem.WaitOne();

            var index = Array.IndexOf(adapters, adapter);
            adaptersTaken[index] = false;

            sem.Release();
        }

        public IEmulatorAdapter GetFirstEmulator()
        {
            InitEmulator(0);
            return adapters[0]!;
        }

        /// <summary>
        /// Only call this once training has fully stopped
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

        public int GetInputCount() => applicationConfig.NeuralConfig.GetInputCount();

        public int GetOutputCount() => applicationConfig.NeuralConfig.GetOutputCount();

        private void InitEmulator(int index)
        {
            if (adapters[index] == null)
            {
                adapters[index] = new BizhawkAdapter(pathToEmulator: DefaultPaths.EMULATOR,
                    pathToLuaScript: DefaultPaths.EMULATOR_ADAPTER,
                    pathToROM: DefaultPaths.ROM,
                    pathToBizhawkConfig: DefaultPaths.EMULATOR_CONFIG,
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
