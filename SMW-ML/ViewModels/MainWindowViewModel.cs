using Avalonia.Controls;
using ReactiveUI;
using SMW_ML.Models.Config;
using SMW_ML.Views.Components;
using System.Threading;

namespace SMW_ML.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private MainPageViewModel mainPageViewModel;
        private TrainingPageViewModel trainingPageViewModel;
        private PlayingPageViewModel playingPageViewModel;

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
            mainPageViewModel.OnOpenPlayMode += HandleOpenPlayMode;
            mainPageViewModel.OnLoadPopulation += HandleLoadPopulation;
            mainPageViewModel.OnSavePopulation += HandleSavePopulation;

            trainingPageViewModel = new TrainingPageViewModel();
            trainingPageViewModel.OnStopTraining += HandleStopTraining;

            playingPageViewModel = new PlayingPageViewModel();
            playingPageViewModel.OnExit += HandlePlayingExit;

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
            trainingPageViewModel.Init();
        }

        public void HandleStopTraining()
        {
            Content = mainPageViewModel;
            mainPageViewModel.CanSaveTraining = true;
        }

        public void HandleOpenPlayMode()
        {
            Content = playingPageViewModel;
            new Thread(() => { playingPageViewModel.Init(); }).Start();
        }

        public void HandlePlayingExit()
        {
            Content = mainPageViewModel;
            playingPageViewModel.OnExit -= HandlePlayingExit;
            playingPageViewModel = new PlayingPageViewModel();
            playingPageViewModel.OnExit += HandlePlayingExit;
        }

    }
}
