using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeat;
using SMW_ML.Neural.Training.SharpNeatImpl;
using SMW_ML.Utils;
using SMW_ML.ViewModels.Neural;
using SMW_ML.ViewModels.Statistics;
using SMW_ML.Views.Components;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels
{
    internal class TrainingPageViewModel : ViewModelBase
    {
        public event Action? OnExit;

        private INeuralTrainer? trainer;
        private EmulatorManager? emulatorManager;
        private string? populationToLoad;

        #region Strings
        public static string StartString => "Start Training";
        public static string StopString => "Stop Training";
        public static string SavePopulationString => "Save population";
        public static string LoadPopulationString => "Load population";
        public static string ExitString => "Return to main menu";
        #endregion

        #region Properties

        private bool canStart = true;
        public bool CanStart
        {
            get => canStart;
            set => this.RaiseAndSetIfChanged(ref canStart, value);
        }

        private bool canStop = false;
        public bool CanStop
        {
            get => canStop;
            set => this.RaiseAndSetIfChanged(ref canStop, value);
        }
        private bool canForceStop = false;
        public bool CanForceStop
        {
            get => canForceStop;
            set => this.RaiseAndSetIfChanged(ref canForceStop, value);
        }
        private bool canLoadTraining = true;
        public bool CanLoadTraining
        {
            get => canLoadTraining;
            set => this.RaiseAndSetIfChanged(ref canLoadTraining, value);
        }

        private bool canSaveTraining = false;
        public bool CanSaveTraining
        {
            get => canSaveTraining;
            set => this.RaiseAndSetIfChanged(ref canSaveTraining, value);
        }

        private NetworkViewModel? neuralNetwork;
        public NetworkViewModel? NeuralNetwork
        {
            get => neuralNetwork;
            set => this.RaiseAndSetIfChanged(ref neuralNetwork, value);
        }

        public ObservableCollection<TrainingStatistics.Stat> TrainingStatistics { get; set; }

        public TrainingChartViewModel TrainingChart { get; set; }

        #endregion

        #region Constructor
        public TrainingPageViewModel()
        {
            TrainingStatistics = new ObservableCollection<TrainingStatistics.Stat>();
            TrainingChart = new TrainingChartViewModel();
        }
        #endregion

        #region Methods

        public void StartTraining()
        {
            if (!CanStart) return;
            CanStart = false;
            CanLoadTraining = false;
            CanSaveTraining = false;
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = ApplicationConfig.Deserialize(appConfigJson)!;

            NeuralNetwork = new NetworkViewModel(appConfig.NeuralConfig);
            emulatorManager = new(appConfig);
            trainer = new SharpNeatTrainer(emulatorManager, appConfig);
            trainer.OnStatisticsUpdated += HandleGetStats;
            if (populationToLoad != null)
            {
                trainer.LoadPopulation(populationToLoad);
                populationToLoad = null;
            }

            new Thread(() =>
            {
                emulatorManager.Init(true);
                emulatorManager.GetFirstEmulator().LinkedNetworkActivated += NeuralNetwork.UpdateNodes;
                emulatorManager.GetFirstEmulator().ChangedLinkedNetwork += NeuralNetwork.UpdateTopology;
                TrainingChart.ClearData();
                trainer.StartTraining(DefaultPaths.SHARPNEAT_CONFIG);
                CanStop = true;
                CanForceStop = true;
            }).Start();

        }

        public async void LoadPopulation()
        {
            IsEnabled = false;
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Population", Extensions = { "pop" } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());
            string path = paths?[0] ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                IsEnabled = true;
                return;
            }

            populationToLoad = path;
            IsEnabled = true;
        }

        public async void SavePopulation()
        {
            IsEnabled = false;
            SaveFileDialog fileDialog = new();
            fileDialog.Filters.Add(new() { Name = "Population", Extensions = { "pop" } });
            fileDialog.Directory = Path.GetFullPath(".");
            fileDialog.InitialFileName = "population";

            string? path = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());

            if (string.IsNullOrWhiteSpace(path))
            {
                IsEnabled = true;
                return;
            }

            trainer!.SavePopulation(path);
            IsEnabled = true;
        }

        public async void StopTraining(bool forceStop)
        {
            if (!CanStop && !forceStop) return;
            if (!CanForceStop && forceStop) return;

            if (forceStop)
            {
                if (await MessageBox.Show(null, "Do you really want to stop the training?", "Force Stop", MessageBox.MessageBoxButtons.YesNo) != MessageBox.MessageBoxResult.Yes)
                {
                    return;
                }
            }

            CanStop = false;
            CanForceStop = !forceStop;

            if (trainer != null) trainer.ForceStop = forceStop;

            await Task.Run(() =>
            {
                trainer?.StopTraining();
                emulatorManager?.Clean();
                CanStart = true;
                CanSaveTraining = true;
                CanLoadTraining = true;
                CanForceStop = false;
            });
        }

        public void Exit()
        {
            if (!CanStart) return;

            Dispatcher.UIThread.Post(() =>
            {
                OnExit?.Invoke();
            });
        }
        #endregion

        #region Events

        private void HandleGetStats(TrainingStatistics stats)
        {
            Dispatcher.UIThread.Post(() =>
            {
                using var delay = DelayChangeNotifications();

                TrainingStatistics.Clear();
                foreach (var stat in stats.GetStats())
                {
                    TrainingStatistics.Add(stat);
                }
                TrainingChart.AddGeneration(stats);
            });
        }
        #endregion

    }
}
