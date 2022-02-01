using Avalonia.Controls;
using ReactiveUI;

namespace SMW_ML.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private MainPageViewModel mainPageViewModel;
        private TrainingPageViewModel trainingPageViewModel;

        private ViewModelBase? content;

        public ViewModelBase? Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public MainWindowViewModel()
        {
            mainPageViewModel = new MainPageViewModel();
            mainPageViewModel.OnStartTrainingCalled += HandleStartTraining;
            mainPageViewModel.OnLoadPopulation += HandleLoadPopulation;
            mainPageViewModel.OnSavePopulation += HandleSavePopulation;
            trainingPageViewModel = new TrainingPageViewModel();
            trainingPageViewModel.OnStopTraining += HandleStopTraining;

            Content = mainPageViewModel;
        }

        public void HandleSavePopulation(string path)
        {
            trainingPageViewModel.SavePopulation(path);
        }

        public void HandleLoadPopulation(string path)
        {
            trainingPageViewModel.LoadPopulation(path);
        }

        public void HandleStartTraining()
        {
            Content = trainingPageViewModel;
            trainingPageViewModel.Init();
        }

        public void HandleStopTraining()
        {
            Content = mainPageViewModel;
            mainPageViewModel.CanSaveTraining = true;
        }

        public string OpenSettingsString => "Training Configuration";
        public void OpenConfigSettings(Window mainWindow)
        {
            var configViewModel = new ConfigurationViewModel();
            configViewModel.ShowWindow(mainWindow);
        }
    }
}
