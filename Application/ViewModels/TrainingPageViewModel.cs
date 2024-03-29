﻿using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using Retro_ML.Application.ViewModels.Neural;
using Retro_ML.Application.ViewModels.Statistics;
using Retro_ML.Application.Views.Components;
using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.Utils;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Retro_ML.Application.ViewModels
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
        public static string OpenTrainingFolderString => "Open Training Folder";
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
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = ApplicationConfig.Deserialize(appConfigJson)!;
            IGamePlugin gamePlugin = appConfig.GetGamePlugin();

            NeuralNetwork = new NetworkViewModel(appConfig.NeuralConfig);
            emulatorManager = new(appConfig, gamePlugin.GetDataFetcherFactory());
            trainer = gamePlugin.GetNeuralTrainer(emulatorManager, appConfig);
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
                trainer.OnStopConditionReached += HandleStopConditionReached;
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
            if (VerifyPopulation(path))
            {
                populationToLoad = path;
            }
            else
            {
                populationToLoad = null;
                Exceptions.QueueException(new Exception("The population is incompatible with the current Neural configuration."));
            }
            IsEnabled = true;
        }

        private bool VerifyPopulation(string path)
        {
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = ApplicationConfig.Deserialize(appConfigJson)!;

            var zip = ZipFile.Open(path, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.Contains(".genome"))
                {
                    using StreamReader sw = new StreamReader(entry.Open());
                    for (string? line = sw.ReadLine(); line != null; line = sw.ReadLine())
                    {
                        line = line.Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.StartsWith("#")) continue;
                        string[] inputOutput = line.Split(null);
                        int input = int.Parse(inputOutput[0]);
                        int output = int.Parse(inputOutput[1]);
                        return appConfig.NeuralConfig.GetInputCount() == input && appConfig.NeuralConfig.GetOutputCount() == output;
                    }
                }
            }

            return false;
        }

        public void OpenTrainingFolder() => Process.Start("explorer.exe", Path.GetFullPath("."));

        public async void StopTraining(bool forceStop)
        {
            if (!CanStop && !forceStop) return;
            if (!CanForceStop && forceStop) return;

            if (forceStop)
            {
                if (await MessageBox.Show(ViewLocator.GetMainWindow(), "Do you really want to stop the training?", "Force Stop", MessageBox.MessageBoxButtons.YesNo) != MessageBox.MessageBoxResult.Yes)
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

        private void HandleStopConditionReached()
        {
            Dispatcher.UIThread.Post(() =>
            {
                StopTraining(false);
            });
        }
        #endregion

    }
}
