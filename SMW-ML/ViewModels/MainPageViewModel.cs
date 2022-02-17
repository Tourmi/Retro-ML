using Avalonia.Controls;
using ReactiveUI;
using System;
using System.IO;

namespace SMW_ML.ViewModels
{
    internal class MainPageViewModel : ViewModelBase
    {
        public event Action? OnStartTrainingCalled;
        public event Action? OnOpenPlayMode;
        public event Action<string>? OnSavePopulation;
        public event Action<string>? OnLoadPopulation;

        private bool canSaveTraining = false;
        public static string Greeting => "Super Mario World - Machine Learning";

        public static string Start => "Start Training";
        public void StartTraining()
        {
            OnStartTrainingCalled?.Invoke();
        }

        public static string PlayString => "Play mode";
        public void OpenPlay()
        {
            OnOpenPlayMode?.Invoke();
        }

        public static string LoadPopulationString => "Load population";
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

            OnLoadPopulation?.Invoke(path);
        }

        public static string SavePopulationString => "Save population";
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

            OnSavePopulation?.Invoke(path);
        }

        public static string OpenSettingsString => "Training Configuration";
        public void OpenConfigSettings(Window mainPage)
        {
            var configViewModel = new ConfigurationViewModel();
            configViewModel.ShowWindow(mainPage);
        }

        public bool CanSaveTraining
        {
            get => canSaveTraining;
            set => this.RaiseAndSetIfChanged(ref canSaveTraining, value);
        }
    }
}
