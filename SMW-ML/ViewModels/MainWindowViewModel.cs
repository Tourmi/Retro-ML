using Avalonia.Threading;
using ReactiveUI;
using SMW_ML.Utils;
using SMW_ML.Views.Components;
using System;
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
            mainPageViewModel.OnStartTrainingCalled += HandleTrainingEnter;
            mainPageViewModel.OnOpenPlayMode += HandleOpenPlayMode;

            trainingPageViewModel = new TrainingPageViewModel();
            trainingPageViewModel.OnExit += HandleTrainingExit;

            playingPageViewModel = new PlayingPageViewModel();
            playingPageViewModel.OnExit += HandlePlayingExit;

            new Thread(ErrorManagementThread).Start();

            Content = mainPageViewModel;
        }

        public void HandleTrainingEnter()
        {
            trainingPageViewModel.IsEnabled = false;
            Content = trainingPageViewModel;
            trainingPageViewModel.IsEnabled = true;
        }

        public void HandleTrainingExit()
        {
            mainPageViewModel.IsEnabled = false;
            Content = mainPageViewModel;
            mainPageViewModel.IsEnabled = true;
        }

        public void HandleOpenPlayMode()
        {
            playingPageViewModel.IsEnabled = false;
            Content = playingPageViewModel;
            new Thread(() =>
            {
                playingPageViewModel.Init();
                playingPageViewModel.IsEnabled = true;
            }).Start();
        }

        public void HandlePlayingExit()
        {
            mainPageViewModel.IsEnabled = false;
            Content = mainPageViewModel;
            playingPageViewModel.OnExit -= HandlePlayingExit;
            playingPageViewModel = new PlayingPageViewModel();
            playingPageViewModel.OnExit += HandlePlayingExit;
            mainPageViewModel.IsEnabled = true;
        }

        public void ErrorManagementThread()
        {
            while (true)
            {
                Exception ex = Exceptions.ConsumeException();

                Dispatcher.UIThread.Post(async () =>
                {
                    bool currStatus = Content!.IsEnabled;
                    Content.IsEnabled = false;
                    await MessageBox.Show(null, ex.Message, "Error", MessageBox.MessageBoxButtons.Ok);
                    Content.IsEnabled = currStatus;
                });
            }
        }
    }
}
