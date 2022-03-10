using Avalonia.Controls;
using Newtonsoft.Json;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Play;
using SMW_ML.Neural.Play.SharpNeat;
using SMW_ML.Utils;
using SMW_ML.ViewModels.Neural;
using System;
using System.IO;
using System.Threading;

namespace SMW_ML.ViewModels
{
    internal class PlayingPageViewModel : ViewModelBase
    {
        public event Action? OnExit;

        #region Strings
        public static string ExitString => "Exit";
        public static string StartString => "Start";
        public static string StopString => "Stop";
        public static string LoadGenomeString => "Load genome";
        public static string LoadSaveStateString => "Load save state";
        #endregion

        private INeuralPlayer? neuralPlayer;
        private EmulatorManager? emulatorManager;
        private NetworkViewModel? neuralNetwork;

        private bool loadedState = false;
        private bool loadedGenome = false;

        public NetworkViewModel NeuralNetwork
        {
            get => neuralNetwork!;
            set => this.RaiseAndSetIfChanged(ref neuralNetwork, value);
        }

        private bool canStart;
        public bool CanStart
        {
            get => canStart;
            set => this.RaiseAndSetIfChanged(ref canStart, value);
        }
        private bool canStop;
        public bool CanStop
        {
            get => canStop;
            set => this.RaiseAndSetIfChanged(ref canStop, value);
        }

        public PlayingPageViewModel()
        {
            CanStart = false;
            CanStop = false;
        }

        public void Init()
        {
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = JsonConvert.DeserializeObject<ApplicationConfig>(appConfigJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace })!;
            appConfig.NeuralConfig.InitNodes();

            NeuralNetwork = new NetworkViewModel(appConfig.NeuralConfig);
            emulatorManager = new(1, appConfig);
            neuralPlayer = new SharpNeatPlayer(emulatorManager);
            emulatorManager.GetFirstEmulator().LinkedNetworkActivated += NeuralNetwork.UpdateNodes;
            emulatorManager.GetFirstEmulator().ChangedLinkedNetwork += NeuralNetwork.UpdateTopology;
        }

        public async void LoadGenome()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Genome", Extensions = { "genome" } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());
            string path = paths?[0] ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            neuralPlayer!.LoadGenome(path);
            loadedGenome = true;
            UpdateCanStart();
        }

        public async void LoadState()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Save State", Extensions = { "State" } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath("./config/SaveStates");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());
            string path = paths?[0] ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            neuralPlayer!.LoadState(path);
            loadedState = true;
            UpdateCanStart();
        }

        public void Start()
        {
            if (!CanStart) return;
            CanStart = false;

            new Thread(() =>
            {
                neuralPlayer!.StartPlaying();
                CanStop = true;
            }).Start();
        }

        public void Stop()
        {
            if (!CanStop) return;
            CanStop = false;
            neuralPlayer!.StopPlaying();
            CanStart = true;
        }

        public void Exit()
        {
            neuralPlayer!.Dispose();
            OnExit?.Invoke();
        }

        private void UpdateCanStart() => CanStart = loadedGenome && loadedState && !neuralPlayer.IsPlaying;
    }
}
