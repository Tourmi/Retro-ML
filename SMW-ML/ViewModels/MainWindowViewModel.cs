using Avalonia.Controls;
using ReactiveUI;
using SMW_ML.Models.Config;
using SMW_ML.Views.Components;

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

        public async void HandleLoadPopulation(string path)
        {
            try
            {
                trainingPageViewModel.LoadPopulation(path);
            }
            catch
            {
                await MessageBox.Show(null, "Could not load the population. The current configuration might be invalid for the population.", "Error", MessageBox.MessageBoxButtons.Ok);
            }
        }

        public void HandleStartTraining()
        {
            Content = trainingPageViewModel;
            trainingPageViewModel.Init(new NeuralConfig());
        }

        public void HandleStopTraining()
        {
            Content = mainPageViewModel;
            mainPageViewModel.CanSaveTraining = true;
        }


    }
}
