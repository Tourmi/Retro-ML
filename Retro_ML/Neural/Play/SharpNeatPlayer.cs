using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Scoring;
using Retro_ML.Utils;
using Retro_ML.Utils.SharpNeat;
using SharpNeat.BlackBox;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace Retro_ML.Neural.Play
{
    public class SharpNeatPlayer : INeuralPlayer
    {
        public event Action? FinishedPlaying;

        protected readonly Semaphore syncSemaphore;
        protected readonly EmulatorManager emulatorManager;
        protected readonly IEmulatorAdapter emulator;

        protected MetaNeatGenome<double> metaGenome;
        protected readonly List<IBlackBox<double>> blackBoxes;
        protected readonly List<string> states;

        protected IDataFetcher dataFetcher;
        protected InputSetter inputSetter;
        protected OutputGetter outputGetter;
        protected readonly IScoreFactor[] scoreFactors;

        protected bool shouldStop;

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
            scoreFactors = appConfig.GetScoreFactorClones().ToArray();

            blackBoxes = new();
            states = new();

            dataFetcher = emulator.GetDataFetcher();
            inputSetter = emulator.GetInputSetter();
            outputGetter = emulator.GetOutputGetter();
        }

        public virtual bool LoadGenomes(string[] paths)
        {
            bool shouldRestart = IsPlaying;
            StopPlaying();

            blackBoxes.Clear();
            foreach (var path in paths.OrderBy(p => p))
            {
                if (!LoadGenome(path))
                {
                    return false;
                }
            }

            if (shouldRestart) StartPlaying();
            return true;
        }

        public virtual void LoadStates(string[] paths)
        {
            bool shouldRestart = IsPlaying;
            StopPlaying();

            states.Clear();

            foreach (var path in paths)
            {
                LoadState(path);
            }

            if (shouldRestart) StartPlaying();
        }

        public virtual void StartPlaying()
        {
            if (IsPlaying)
            {
                throw new InvalidOperationException("Already playing");
            }
            if (blackBoxes.Count == 0)
            {
                throw new InvalidOperationException("A genome wasn't loaded before playing");
            }
            if (states.Count == 0)
            {
                throw new InvalidOperationException("A save state wasn't loaded before playing");
            }
            IsPlaying = true;
            shouldStop = false;

            new Thread(Play).Start();
        }

        public virtual void StopPlaying()
        {
            shouldStop = true;
            syncSemaphore.WaitOne();
            syncSemaphore.Release();
        }

        protected virtual bool LoadGenome(string path)
        {
            if (!VerifyGenome(path))
            {
                Exceptions.QueueException(new Exception($"Could not load genome {path}. It uses a different Neural Configuration."));
                return false;
            }
            var loader = NeatGenomeLoaderFactory.CreateLoaderDouble(metaGenome);

            NeatGenomeDecoderAcyclic decoder = new();
            blackBoxes.Add(decoder.Decode(loader.Load(path)));
            return true;
        }

        protected virtual bool VerifyGenome(string path)
        {
            string[] inputOutput = File.ReadLines(path).Where(l => !l.Trim().StartsWith("#") && !string.IsNullOrEmpty(l.Trim())).First().Trim().Split(null);
            int input = int.Parse(inputOutput[0]);
            int output = int.Parse(inputOutput[1]);

            return input == metaGenome.InputNodeCount && output == metaGenome.OutputNodeCount;
        }

        protected virtual void LoadState(string path)
        {
            states.Add(Path.GetFullPath(path));
        }

        /// <summary>
        /// Play loop
        /// </summary>
        protected virtual void Play()
        {
            try
            {
                IsPlaying = true;
                syncSemaphore.WaitOne();

                DoPlayLoop();

                syncSemaphore.Release();
                IsPlaying = false;
                FinishedPlaying?.Invoke();
            }
            catch (Exception ex)
            {
                Exceptions.QueueException(new Exception($"An exception occured during play. Was the emulator window closed?\n{ex.Message}\n{ex.StackTrace}"));
            }
        }

        /// <summary>
        /// Goes through every savestates for every loaded black boxes
        /// </summary>
        protected virtual void DoPlayLoop()
        {
            var blackBoxesEnum = blackBoxes.GetEnumerator();
            blackBoxesEnum.MoveNext();
            UpdateNetwork(blackBoxesEnum.Current);

            var statesEnum = states.GetEnumerator();
            while (!shouldStop)
            {
                if (!statesEnum.MoveNext())
                {
                    if (!blackBoxesEnum.MoveNext())
                    {
                        break;
                    }
                    UpdateNetwork(blackBoxesEnum.Current);
                    statesEnum = states.GetEnumerator();
                    statesEnum.MoveNext();

                }
                DoSaveState(statesEnum.Current, blackBoxesEnum.Current);
            }
        }

        /// <summary>
        /// Plays the game starting from the given savestate.
        /// </summary>
        /// <param name="saveState"></param>
        /// <param name="blackBox"></param>
        protected virtual void DoSaveState(string saveState, IBlackBox<double> blackBox)
        {
            emulator.LoadState(saveState);
            emulator.NextFrame();
            dataFetcher.NextState();

            Score score = new(scoreFactors.Select(s => s.Clone()));

            while (!shouldStop && !score.ShouldStop)
            {
                DoFrame(blackBox);
                score.Update(dataFetcher);
            }
        }

        /// <summary>
        /// Executes one frame of play mode
        /// </summary>
        protected virtual void DoFrame(IBlackBox<double> blackBox)
        {
            blackBox!.ResetState();
            inputSetter.SetInputs(blackBox.InputVector);
            blackBox.Activate();

            emulator.SendInput(outputGetter.GetControllerInput(blackBox.OutputVector));

            emulator.NextFrame();
            dataFetcher.NextFrame();
            emulator.NetworkUpdated(SharpNeatUtils.VectorToArray(blackBox.InputVector), SharpNeatUtils.VectorToArray(blackBox.OutputVector));
        }

        /// <summary>
        /// Sends the network changed event.
        /// </summary>
        protected void UpdateNetwork(IBlackBox<double> blackBox)
        {
            int[] outputMap = new int[blackBox!.OutputCount];
            Array.Copy(blackBox.OutputVector.GetField<int[]>("_map"), outputMap, blackBox.OutputCount);
            emulator.NetworkChanged(SharpNeatUtils.GetConnectionLayers(blackBox), outputMap);
        }

        public virtual void Dispose()
        {
            emulatorManager.FreeOne(emulator);

            emulatorManager.Clean();
        }
    }
}
