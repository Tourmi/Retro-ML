﻿using SMW_ML.Arduino;
using SMW_ML.Game;
using SMW_ML.Game.SuperMarioWorld;
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

        private readonly Socket client;
        private ArduinoPreviewer? arduinoPreviewer;

        private readonly DataFetcher dataFetcher;
        private readonly InputSetter inputSetter;
        private readonly OutputGetter outputGetter;

        private readonly string[] savestates;

        public BizhawkAdapter(string pathToEmulator, string pathToLuaScript, string pathToROM, string pathToBizhawkConfig, string savestatesPath, string socketIP, string socketPort, Socket server)
        {
            ProcessStartInfo startInfo = new(pathToEmulator);
            startInfo.ArgumentList.Add($"--socket_port={socketPort}");
            startInfo.ArgumentList.Add($"--socket_ip={socketIP}");
            startInfo.ArgumentList.Add($"--lua={pathToLuaScript}");
            //startInfo.ArgumentList.Add($"--chromeless");
            startInfo.ArgumentList.Add($"--config={pathToBizhawkConfig}");
            startInfo.ArgumentList.Add(pathToROM);
            Process.Start(startInfo);

            client = server.Accept();
            savestates = Directory.GetFiles(savestatesPath);

            dataFetcher = new DataFetcher(this);
            inputSetter = new InputSetter(dataFetcher);
            outputGetter = new OutputGetter();
        }

        public void SetArduinoPreviewer(ArduinoPreviewer arduinoPreviewer)
        {
            this.arduinoPreviewer = arduinoPreviewer;
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
            SendCommand(Commands.LOAD_STATE, Path.GetFullPath(saveState));
        }

        public void NextFrame()
        {
            SendCommand(Commands.NEXT_FRAME);
        }

        public byte ReadMemory(uint addr)
        {
            SendCommand(Commands.READ_MEMORY, addr);

            return Read(1)[0];
        }

        public byte[] ReadMemory(uint addr, uint count)
        {
            SendCommand(Commands.READ_MEMORY_RANGE, addr, count);

            return Read(count);
        }

        public void SendInput(Input input)
        {
            arduinoPreviewer?.SendInput(input);
            SendCommand(Commands.SEND_INPUT, input);
        }

        public void Dispose()
        {
            SendCommand(Commands.EXIT);

            client.Shutdown(SocketShutdown.Both);
            client.Close();
            client.Dispose();

            arduinoPreviewer?.Dispose();
        }

        private void SendCommand(string command, params object[] args)
        {
            string newCommand = string.Format(command, args);
            newCommand = newCommand.Length + " " + newCommand;
            client.Send(newCommand.Select(s => (byte)s).ToArray());
            byte[] okayByte = new byte[1];
            client.Receive(okayByte);
        }

        private byte[] Read(uint amount)
        {
            byte[] buffer = new byte[amount];
            client.Receive(buffer, (int)amount, SocketFlags.None);

            return buffer;
        }

        public DataFetcher GetDataFetcher() => dataFetcher;

        public InputSetter GetInputSetter() => inputSetter;

        public OutputGetter GetOutputGetter() => outputGetter;
    }
}