using SharpNeat.BlackBox;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;
using SMW_ML.Emulator;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using SMW_ML.Utils;
using SMW_ML.Utils.SharpNeat;
using System;
using System.IO;
using System.Threading;

namespace SMW_ML.Neural.Play.SharpNeat
{
    internal class SharpNeatPlayer : INeuralPlayer
    {
        private readonly Semaphore syncSemaphore;
        private readonly EmulatorManager emulatorManager;
        private readonly IEmulatorAdapter emulator;

        private MetaNeatGenome<double> metaGenome;
        private IBlackBox<double>? blackBox;
        private string? state;

        private DataFetcher dataFetcher;
        private InputSetter inputSetter;
        private OutputGetter outputGetter;

        private bool shouldStop;

        public bool IsPlaying { get; private set; }

        public SharpNeatPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig)
        {
            metaGenome = new MetaNeatGenome<double>(
                    inputNodeCount: appConfig.NeuralConfig.GetInputCount(),
                    outputNodeCount: appConfig.NeuralConfig.GetOutputCount(),
                    isAcyclic: true,
                    activationFn: new LeakyReLU());
            this.emulatorManager = emulatorManager;
            emulatorManager.Init(false);
            emulator = emulatorManager.WaitOne();
            syncSemaphore = new Semaphore(1, 1);

            dataFetcher = emulator.GetDataFetcher();
            inputSetter = emulator.GetInputSetter();
            outputGetter = emulator.GetOutputGetter();
        }

        public void StartPlaying()
        {
            if (IsPlaying)
            {
                throw new InvalidOperationException("Already playing");
            }
            if (blackBox == null)
            {
                throw new InvalidOperationException("A genome wasn't loaded before playing");
            }
            if (state == null)
            {
                throw new InvalidOperationException("A save state wasn't loaded before playing");
            }
            IsPlaying = true;
            shouldStop = false;

            new Thread(Play).Start();
        }

        public void StopPlaying()
        {
            shouldStop = true;
            syncSemaphore.WaitOne();
            syncSemaphore.Release();
        }

        public void LoadGenome(string path)
        {
            bool shouldRestart = IsPlaying;
            StopPlaying();

            var loader = NeatGenomeLoaderFactory.CreateLoaderDouble(metaGenome);

            NeatGenomeDecoderAcyclic decoder = new();
            blackBox = decoder.Decode(loader.Load(path));

            if (shouldRestart) StartPlaying();
        }

        public void LoadState(string path)
        {
            bool shouldRestart = IsPlaying;
            StopPlaying();

            state = Path.GetFullPath(path);
            if (shouldRestart) StartPlaying();
        }

        /// <summary>
        /// Play loop
        /// </summary>
        private void Play()
        {
            try
            {

                IsPlaying = true;
                syncSemaphore.WaitOne();
                UpdateNetwork();
                emulator.LoadState(state!);
                emulator.NextFrame();
                dataFetcher.NextLevel();

                while (!shouldStop)
                {
                    DoFrame();
                }

                syncSemaphore.Release();
                IsPlaying = false;
            }
            catch (Exception ex)
            {
                Exceptions.QueueException(new Exception($"An exception occured during play. Was the emulator window closed?\n{ex.Message}\n{ex.StackTrace}"));
            }
        }

        /// <summary>
        /// Executes one frame of play mode
        /// </summary>
        private void DoFrame()
        {
            blackBox!.ResetState();
            inputSetter.SetInputs(blackBox.InputVector);
            blackBox.Activate();

            emulator.SendInput(outputGetter.GetControllerInput(blackBox.OutputVector));

            emulator.NextFrame();
            dataFetcher.NextFrame();
            emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(blackBox.InputVector), SharpNeatUtils.VectorToArray(blackBox.OutputVector));
        }

        public void Dispose()
        {
            emulatorManager.FreeOne(emulator);

            emulatorManager.Clean();
        }

        /// <summary>
        /// Sends the network changed event.
        /// </summary>
        private void UpdateNetwork()
        {
            int[] outputMap = new int[blackBox!.OutputCount];
            Array.Copy(blackBox.OutputVector.GetField<int[]>("_map"), outputMap, blackBox.OutputCount);
            emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(blackBox), outputMap);
        }
    }
}
