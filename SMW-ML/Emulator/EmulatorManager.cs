using SMW_ML.Arduino;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public EmulatorManager(int numberOfEmulators)
        {
            this.adapters = new IEmulatorAdapter[numberOfEmulators];
            this.adaptersTaken = new bool[adapters.Length];

            sem = new Semaphore(1, 1);
        }

        public void Init()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList.Last();
            IPEndPoint localEndPoint = new(ipAddress, SOCKET_PORT);

            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(localEndPoint);
            server.Listen(MAX_CONNECTIONS);

            for (int i = 0; i < adapters.Length; i++)
            {
                this.adapters[i] = new BizhawkAdapter(
                    pathToEmulator: DefaultPaths.EMULATOR,
                    pathToLuaScript: DefaultPaths.EMULATOR_ADAPTER,
                    pathToROM: DefaultPaths.ROM,
                    pathToBizhawkConfig: DefaultPaths.EMULATOR_CONFIG,
                    savestatesPath: DefaultPaths.SAVESTATES_DIR,
                    socketIP: ipAddress.ToString(),
                    socketPort: SOCKET_PORT.ToString(),
                    server);
            }


            if (ArduinoPreviewer.ArduinoAvailable())
            {
                adapters[0]!.SetArduinoPreviewer(new ArduinoPreviewer());
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
                    chosen = adapters[index];
                    adaptersTaken[index] = true;
                }

                sem.Release();
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

        public int GetInputCount() => new InputSetter(null).GetInputCount();

        public int GetOutputCount() => new OutputGetter().GetOutputCount();
    }
}
