using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeatImpl;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace SMW_ML.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private INeuralTrainer trainer;
        private EmulatorManager emulatorManager;
        private bool isTrainingRunning = false;
        private bool canSaveTraining = false;

        public MainWindowViewModel()
        {
            emulatorManager = new(6);
            trainer = new SharpNeatTrainer(emulatorManager);
        }

        public string Greeting => "Super Mario World - Machine Learning";

        public string Start => "Start Training";
        public void StartTraining()
        {
            IsTrainingRunning = true;
            emulatorManager.Init();
            trainer.StartTraining(DefaultPaths.SHARPNEAT_CONFIG);
        }

        public string Stop => "Stop Training";
        public void StopTraining()
        {
            trainer.StopTraining();
            emulatorManager.Clean();
            IsTrainingRunning = false;
            CanSaveTraining = true;
        }

        public string LoadPopulationString => "Load population";
        public async void LoadPopulation()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Population", Extensions = { "pop" } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());
            string path = paths?[0] ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            trainer.LoadPopulation(path);
        }

        public string SavePopulationString => "Save population";
        public async void SavePopulation()
        {
            SaveFileDialog fileDialog = new();
            fileDialog.Filters.Add(new() { Name = "Population", Extensions = { "pop" } });
            fileDialog.Directory = Path.GetFullPath(".");
            fileDialog.InitialFileName = "population";

            string? path = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            trainer.SavePopulation(path);
        }

        public string OpenSettingsString => "Training Configuration";
        public void OpenConfigSettings()
        {
            var configViewModel = new ConfigurationViewModel();
            configViewModel.ShowWindow();
        }

        [DataMember]
        public bool IsTrainingRunning
        {
            get => isTrainingRunning;
            set
            {
                this.RaiseAndSetIfChanged(ref isTrainingRunning, value);
                this.RaisePropertyChanged(nameof(CanSaveTraining));
            }
        }
        [DataMember]
        public bool CanSaveTraining
        {
            get => canSaveTraining && !isTrainingRunning;
            set => this.RaiseAndSetIfChanged(ref canSaveTraining, value);
        }
    }
}
