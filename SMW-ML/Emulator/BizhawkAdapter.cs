using SMW_ML.Arduino;
using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMW_ML.Emulator
{
    internal class BizhawkAdapter : IEmulatorAdapter
    {
        private static class Commands
        {
            public const string EXIT = "exit";
            public const string EXIT_CODE = "exit {0}";
            public const string LOAD_ROM = "load_rom {0}";
            public const string LOAD_STATE = "load_state {0}";
            public const string NEXT_FRAME = "next_frame";
            public const string READ_MEMORY = "read_memory {0}";
            public const string READ_MEMORY_RANGE = "read_memory_range {0} {1}";
            public const string SEND_INPUT = "send_input {0}";
        }

        private const int SOCKET_PORT = 11000;
        private const int MAX_CONNECTIONS = 1;

        private readonly Semaphore sem;
        private readonly Socket server;
        private readonly Socket client;
        private readonly ArduinoPreviewer? arduinoPreviewer;

        private readonly string savestatesPath;
        private readonly string[] savestates;

        public BizhawkAdapter(string pathToEmulator, string pathToLuaScript, string pathToROM, string pathToBizhawkConfig, string savestatesPath)
        {
            sem = new Semaphore(1, 1);

            if (ArduinoPreviewer.ArduinoAvailable())
            {
                arduinoPreviewer = new ArduinoPreviewer();
            }

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList.Last();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, SOCKET_PORT);

            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(localEndPoint);
            server.Listen(MAX_CONNECTIONS);

            ProcessStartInfo startInfo = new(pathToEmulator);
            startInfo.ArgumentList.Add($"--socket_port={SOCKET_PORT}");
            startInfo.ArgumentList.Add($"--socket_ip={ipAddress}");
            startInfo.ArgumentList.Add($"--lua={pathToLuaScript}");
            startInfo.ArgumentList.Add($"--chromeless");
            startInfo.ArgumentList.Add($"--config={pathToBizhawkConfig}");
            startInfo.ArgumentList.Add(pathToROM);
            Process.Start(startInfo);

            client = server.Accept();
            this.savestatesPath = savestatesPath;
            savestates = Directory.GetFiles(savestatesPath);
        }

        public void LoadRom(string path)
        {
            SendCommand(Commands.LOAD_ROM, path);
        }

        public string[] GetStates()
        {
            return savestates;
        }

        public void LoadState(string saveState)
        {
            SendCommand(Commands.LOAD_STATE, Path.Combine(savestatesPath, saveState));
        }

        public void NextFrame()
        {
            SendCommand(Commands.NEXT_FRAME);
        }

        public byte ReadMemory(int addr)
        {
            SendCommand(Commands.READ_MEMORY, addr);

            return Read(1)[0];
        }

        public byte[] ReadMemory(int addr, int count)
        {
            SendCommand(Commands.READ_MEMORY_RANGE, addr, count);

            return Read(count);
        }

        public void SendInput(Input input)
        {
            arduinoPreviewer?.SendInput(input);
            SendCommand(Commands.SEND_INPUT, input);
        }

        public void Reserve() => sem.WaitOne();

        public void Free() => sem.Release();

        public void Dispose()
        {
            sem.Dispose();

            SendCommand(Commands.EXIT);

            client.Shutdown(SocketShutdown.Both);
            client.Close();
            client.Dispose();

            server.Shutdown(SocketShutdown.Both);
            server.Close();
            server.Dispose();
        }

        private void SendCommand(string command, params object[] args)
        {
            string newCommand = string.Format(command, args);
            newCommand = newCommand.Length + " " + newCommand;
            client.Send(newCommand.Select(s => (byte)s).ToArray());
            byte[] okayByte = new byte[1];
            client.Receive(okayByte);
        }

        private byte[] Read(int amount)
        {
            byte[] buffer = new byte[amount];
            client.Receive(buffer, amount, SocketFlags.None);

            return buffer;
        }
    }
}
