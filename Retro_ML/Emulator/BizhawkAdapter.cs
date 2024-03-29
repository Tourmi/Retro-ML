﻿using Retro_ML.Arduino;
using Retro_ML.Configuration;
using Retro_ML.Game;
using Retro_ML.Neural.Memory;
using Retro_ML.Utils;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Retro_ML.Emulator
{
    internal class BizhawkAdapter : IEmulatorAdapter
    {
        public event Action<double[], double[]>? LinkedNetworkActivated;
        public event Action<(int sourceNode, int targetNode, double weight)[][], int[]>? ChangedLinkedNetwork;

        /// <summary>
        /// The available commands that can be sent to the emulator
        /// </summary>
        private static class Commands
        {
            public const string EXIT = "exit";
            public const string EXIT_CODE = "exit {0}";
            public const string LOAD_ROM = "load_rom {0}";
            public const string LOAD_STATE = "load_state {0}";
            public const string NEXT_FRAME = "next_frame ";
            public const string NEXT_FRAMES = "next_frames {0} {1}";
            public const string WRITE_MEMORY = "write_memory {0} {1}";
            public const string READ_MEMORY = "read_memory {0}";
            public const string READ_MEMORY_RANGE = "read_memory_range {0} {1}";
            public const string READ_MEMORY_RANGES = "read_memory_ranges {0}";
            public const string SEND_INPUT = "send_input {0}";
        }

        private readonly Socket client;
        private ArduinoPreviewer? arduinoPreviewer;

        private readonly IDataFetcher dataFetcher;
        private readonly NeuralMemory neuralMemory;
        private readonly InputSetter inputSetter;
        private readonly OutputGetter outputGetter;

        private readonly string[] savestates;

        private bool waitForOkay = true;

        public BizhawkAdapter(string pathToEmulator,
                              string pathToLuaScript,
                              string pathToROM,
                              string pathToBizhawkConfig,
                              string savestatesPath,
                              string socketIP,
                              string socketPort,
                              Socket server,
                              ApplicationConfig config,
                              IDataFetcherFactory dataFetcherFactory)
        {
            ProcessStartInfo startInfo = new(pathToEmulator);
            //startInfo.ArgumentList.Add($"--socket_port={socketPort}");
            //startInfo.ArgumentList.Add($"--socket_ip={socketIP}");
            startInfo.ArgumentList.Add($"--lua={pathToLuaScript}");
            //startInfo.ArgumentList.Add($"--chromeless");
            startInfo.ArgumentList.Add($"--config={pathToBizhawkConfig}");
            startInfo.ArgumentList.Add(pathToROM);
            Process.Start(startInfo);

            client = server.Accept();
            client.ReceiveBufferSize = 10_000_000;
            client.SendBufferSize = 10_000_000;
            savestates = Directory.GetFiles(savestatesPath);

            dataFetcher = dataFetcherFactory.GetDataFetcher(config, this);
            neuralMemory = new NeuralMemory(config.NeuralConfig.ShortTermMemoryNodeCount, config.NeuralConfig.LongTermMemoryNodeCount, config.NeuralConfig.PermanentMemoryNodeCount, config.NeuralConfig.MaximumMemoryNodeValue);
            inputSetter = new InputSetter(dataFetcher, config.NeuralConfig, neuralMemory);
            outputGetter = new OutputGetter(config, neuralMemory);
        }
        public void SetArduinoPreviewer(ArduinoPreviewer arduinoPreviewer)
        {
            this.arduinoPreviewer = arduinoPreviewer;
        }

        public void LoadRom(string path)
        {
            SendCommand(Commands.LOAD_ROM, path);
            neuralMemory.Reset();
        }

        public string[] GetStates()
        {
            return savestates;
        }

        public void LoadState(string saveState)
        {
            SendCommand(Commands.LOAD_STATE, Path.GetFullPath(saveState));
            neuralMemory.Reset();
        }

        public void NextFrame()
        {
            SendCommand(Commands.NEXT_FRAME);
        }

        public void NextFrames(int frameCount, bool repeatInput)
        {
            SendCommand(Commands.NEXT_FRAMES, frameCount, repeatInput);
        }

        public void WriteMemory(uint addr, byte value)
        {
            SendCommand(Commands.WRITE_MEMORY, addr, value);
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

        public byte[] ReadMemory(params (uint addr, uint count)[] ranges)
        {
            StringBuilder s = new();
            uint totalCount = 0;
            foreach ((uint addr, uint count) in ranges)
            {
                s.Append($"{addr} {count};");
                totalCount += count;
            }

            SendCommand(Commands.READ_MEMORY_RANGES, s.ToString());
            return Read(totalCount);
        }

        public void SendInput(IInput input)
        {
            arduinoPreviewer?.SendInput(input.ToArduinoBytes());
            SendCommand(Commands.SEND_INPUT, input.GetString());
            DebugInfo.AddInfo("Input sent to emulator", input.GetString());
        }

        public void Dispose()
        {
            waitForOkay = false;
            SendCommand(Commands.EXIT);

            client.Close();
            client.Dispose();

            arduinoPreviewer?.Dispose();
        }

        /// <summary>
        /// Sends the given <paramref name="command"/> to the emulator, formatting it with the given <paramref name="args"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        private void SendCommand(string command, params object[] args)
        {
            string newCommand = string.Format(command, args);
            newCommand += "\n";
            client.Send(newCommand.Select(s => (byte)s).ToArray());
            if (waitForOkay)
            {
                byte[] okayByte = new byte[1];
                client.Receive(okayByte);
            }
        }

        /// <summary>
        /// Reads the amount of bytes specified from the serial port
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private byte[] Read(uint amount)
        {
            byte[] buffer = new byte[amount];
            int received = 0;
            while (received < amount)
            {
                received += client.Receive(buffer, received, (int)amount - received, SocketFlags.None);
            }

            return buffer;
        }

        public void NetworkUpdated(double[] inputs, double[] outputs)
        {
            LinkedNetworkActivated?.Invoke(inputs, outputs);
        }

        public void NetworkChanged((int sourceNode, int targetNode, double weight)[][] connectionLayers, int[] outputIds)
        {
            ChangedLinkedNetwork?.Invoke(connectionLayers, outputIds);
        }

        public IDataFetcher GetDataFetcher() => dataFetcher;

        public InputSetter GetInputSetter() => inputSetter;

        public OutputGetter GetOutputGetter() => outputGetter;

    }
}