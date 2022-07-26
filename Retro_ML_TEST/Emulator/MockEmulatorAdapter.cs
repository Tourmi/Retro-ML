using Retro_ML.Arduino;
using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Memory;
using Retro_ML.SuperMarioWorld.Configuration;
using Retro_ML.SuperMarioWorld.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Retro_ML_TEST.Emulator
{
    internal class MockEmulatorAdapter : IEmulatorAdapter
    {
        public event Action<double[], double[]>? LinkedNetworkActivated;
        public event Action<(int sourceNode, int targetNode, double weight)[][], int[]>? ChangedLinkedNetwork;

        public SMWDataFetcher DataFetcher;
        public NeuralMemory NeuralMemory;
        public InputSetter InputSetter;
        public OutputGetter OutputGetter;

        public Dictionary<uint, byte> Memory;

        public MockEmulatorAdapter()
        {
            DataFetcher = new SMWDataFetcher(this, new NeuralConfig(), new SMWPluginConfig());
            NeuralMemory = new NeuralMemory(0, 0, 0, 1);
            InputSetter = new InputSetter(DataFetcher, new NeuralConfig(), NeuralMemory);
            OutputGetter = new OutputGetter(new ApplicationConfig(), NeuralMemory);

            Memory = new Dictionary<uint, byte>();
        }

        public int DisposeCallCount = 0;
        public void Dispose()
        {
            DisposeCallCount++;
        }

        public IDataFetcher GetDataFetcher() => DataFetcher;

        public InputSetter GetInputSetter() => InputSetter;

        public OutputGetter GetOutputGetter() => OutputGetter;

        public string[] GetStates() => new string[] { "state1", "state2", "state3" };


        public int LoadRomCallCount = 0;
        public void LoadRom(string path) => LoadRomCallCount++;

        public int LoadStateCallCount = 0;
        public void LoadState(string saveState)
        {
            LoadStateCallCount++;

            NeuralMemory.Reset();

            if (!GetStates().Any(state => saveState.EndsWith(state)))
            {
                throw new Exception($"Invalid save state : {saveState}");
            }
        }

        public int NextFrameCallCount = 0;
        public void NextFrame()
        {
            NextFrameCallCount++;
        }

        public void NextFrames(int frameCount, bool repeatInput)
        {
            NextFrameCallCount += frameCount;
        }

        public void SetMemory(uint addr, byte b)
        {
            Memory[addr] = b;
        }

        public void SetMemory(uint addr, params byte[] bs)
        {
            for (uint i = 0; i < bs.Length; i++)
            {
                Memory[addr + i] = bs[i];
            }
        }

        public void WriteMemory(uint addr, byte val) => SetMemory(addr, val);

        public int ReadMemoryCallCount = 0;
        public byte ReadMemory(uint addr)
        {
            ReadMemoryCallCount++;

            Memory.TryGetValue(addr, out byte result);

            return result;
        }

        public byte[] ReadMemory(uint addr, uint count)
        {
            ReadMemoryCallCount++;

            var result = new byte[count];

            for (uint i = 0; i < count; i++)
            {
                Memory.TryGetValue(addr + i, out result[i]);
            }

            return result;
        }

        public int SendInputCallCount = 0;

        public void SendInput(IInput input)
        {
            SendInputCallCount++;
        }

        public void SetArduinoPreviewer(ArduinoPreviewer arduinoPreviewer) { }

        public void NetworkUpdated(double[] inputs, double[] outputs)
        {
            throw new NotImplementedException();
        }

        public void NetworkChanged((int sourceNode, int targetNode, double weight)[][] connectionLayers, int[] outputIds)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadMemory(params (uint addr, uint count)[] ranges)
        {
            List<byte> res = new();
            foreach (var range in ranges)
            {
                res.AddRange(ReadMemory(range.addr, range.count));
            }
            return res.ToArray();
        }
    }
}