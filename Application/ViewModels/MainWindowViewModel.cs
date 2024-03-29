using Avalonia.Threading;
using ReactiveUI;
using Retro_ML.Application.Views.Components;
using Retro_ML.Configuration;
using Retro_ML.Utils;
using System;
using System.IO;
using System.Threading;

namespace Retro_ML.Application.ViewModels
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
            PluginUtils.LoadPlugins();

            ApplicationConfig applicationConfig = ApplicationConfig.Deserialize(File.ReadAllText(DefaultPaths.APP_CONFIG));
            if (string.IsNullOrWhiteSpace(applicationConfig.RomPath))
            {
                Exceptions.QueueException(new Exception($"No ROMs are currently specified. Please go into the Configuration to select a ROM before anything else."));
            }
            else if (!File.Exists(applicationConfig.RomPath))
            {
                Exceptions.QueueException(new Exception($"Could not find ROM at path {applicationConfig.RomPath}. Please go into the Configuration to select a ROM before anything else."));
            }

            Content = mainPageViewModel;
        }

        public void HandleTrainingEnter()
        {
            ViewLocator.GetMainWindow().SetCloseButtonEnabled(false);
            trainingPageViewModel.IsEnabled = false;
            Content = trainingPageViewModel;
            trainingPageViewModel.IsEnabled = true;
        }

        public void HandleTrainingExit()
        {
            ViewLocator.GetMainWindow().SetCloseButtonEnabled(true);
            mainPageViewModel.IsEnabled = false;
            Content = mainPageViewModel;
            mainPageViewModel.IsEnabled = true;
        }

        public void HandleOpenPlayMode()
        {
            ViewLocator.GetMainWindow().SetCloseButtonEnabled(false);
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
            ViewLocator.GetMainWindow().SetCloseButtonEnabled(true);
            mainPageViewModel.IsEnabled = false;
            Content = mainPageViewModel;
            playingPageViewModel.OnExit -= HandlePlayingExit;
            playingPageViewModel = new PlayingPageViewModel();
            playingPageViewModel.OnExit += HandlePlayingExit;
            mainPageViewModel.IsEnabled = true;
        }

        public static void OpenDebug()
        {
            if (DebugInfo.IsDebug)
            {
                var debugViewModel = new DebugWindowViewModel();
                debugViewModel.ShowWindow(ViewLocator.GetMainWindow());
            }
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
                    await MessageBox.Show(ViewLocator.GetMainWindow(), ex.Message, "Error", MessageBox.MessageBoxButtons.Ok);
                    Content.IsEnabled = currStatus;
                });
            }
        }
    }
}
