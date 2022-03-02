﻿using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
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

        private INeuralTrainer trainer;
        private EmulatorManager emulatorManager;
        private string? populationToLoad;

        #region Strings
        public static string StartString => "Start Training";
        public static string StopString => "Stop Training";
        public static string SavePopulationString => "Save population";
        public static string LoadPopulationString => "Load population";
        public static string ExitString => "Exit";
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

        private NetworkViewModel neuralNetwork;
        public NetworkViewModel NeuralNetwork
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

        public async void StartTraining()
        {
            if (!CanStart) return;
            CanStart = false;
            CanLoadTraining = false;
            CanSaveTraining = false;
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = JsonConvert.DeserializeObject<ApplicationConfig>(appConfigJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace })!;
            appConfig.NeuralConfig.InitNodes();

            NeuralNetwork = new NetworkViewModel(appConfig.NeuralConfig);
            emulatorManager = new(appConfig);
            trainer = new SharpNeatTrainer(emulatorManager, appConfig);
            trainer.OnStatisticsUpdated += HandleGetStats;
            if (populationToLoad != null)
            {
                try
                {
                    trainer.LoadPopulation(populationToLoad);
                    populationToLoad = null;
                }
                catch
                {
                    await MessageBox.Show(null, "Failed to load specified population. Is it compatible with the current Input/Output neuron settings?", "Error", MessageBox.MessageBoxButtons.Ok);
                }
            }

            new Thread(() =>
            {
                emulatorManager.Init();
                emulatorManager.GetFirstEmulator().LinkedNetworkActivated += NeuralNetwork.UpdateNodes;
                emulatorManager.GetFirstEmulator().ChangedLinkedNetwork += NeuralNetwork.UpdateTopology;
                TrainingChart.ClearData();
                trainer.StartTraining(DefaultPaths.SHARPNEAT_CONFIG);
            }).Start();

            CanStop = true;
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

            trainer.SavePopulation(path);
            IsEnabled = true;
        }

        public async void StopTraining()
        {
            if (!CanStop) return;

            CanStop = false;
            await Task.Run(() =>
            {
                trainer?.StopTraining();
                emulatorManager?.Clean();
                CanStart = true;
                CanSaveTraining = true;
                CanLoadTraining = true;
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
