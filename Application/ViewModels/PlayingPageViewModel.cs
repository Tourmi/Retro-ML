﻿using Avalonia.Controls;
using ReactiveUI;
using Retro_ML.Application.ViewModels.Neural;
using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Play;
using Retro_ML.Plugin;
using Retro_ML.Utils;
using System;
using System.IO;
using System.Threading;

namespace Retro_ML.Application.ViewModels
{
    internal class PlayingPageViewModel : ViewModelBase
    {
        public event Action? OnExit;

        #region Strings
        public static string ExitString => "Return to main menu";
        public static string StartString => "Start";
        public static string StopString => "Stop";
        public static string LoadGenomeString => "Load genomes";
        public static string LoadSaveStateString => "Load save states";
        #endregion

        private INeuralPlayer? neuralPlayer;
        private EmulatorManager? emulatorManager;
        private NetworkViewModel? neuralNetwork;
        private ApplicationConfig? appConfig;

        private bool loadedState = false;
        private bool loadedGenome = false;

        public NetworkViewModel NeuralNetwork
        {
            get => neuralNetwork!;
            set => this.RaiseAndSetIfChanged(ref neuralNetwork, value);
        }

        private bool canStart;
        public bool CanStart
        {
            get => canStart;
            set => this.RaiseAndSetIfChanged(ref canStart, value);
        }
        private bool canStop;
        public bool CanStop
        {
            get => canStop;
            set => this.RaiseAndSetIfChanged(ref canStop, value);
        }

        public PlayingPageViewModel()
        {
            CanStart = false;
            CanStop = false;
        }

        public void Init()
        {
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            appConfig = ApplicationConfig.Deserialize(appConfigJson)!;
            IGamePlugin gamePlugin = appConfig.GetGamePlugin();

            NeuralNetwork = new NetworkViewModel(appConfig.NeuralConfig);
            emulatorManager = new(1, appConfig, gamePlugin.GetDataFetcherFactory());
            neuralPlayer = gamePlugin.GetNeuralPlayer(emulatorManager, appConfig);
            neuralPlayer.FinishedPlaying += HandlePlayStopped;
            emulatorManager.GetFirstEmulator().LinkedNetworkActivated += NeuralNetwork.UpdateNodes;
            emulatorManager.GetFirstEmulator().ChangedLinkedNetwork += NeuralNetwork.UpdateTopology;
        }

        public async void LoadGenome()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Genome", Extensions = { "genome" } });
            fileDialog.AllowMultiple = true;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());

            if ((paths?.Length ?? 0) == 0)
            {
                return;
            }

            if (neuralPlayer!.LoadGenomes(paths!))
            {
                loadedGenome = true;
                UpdateCanStart();
            }
        }

        public async void LoadState()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Save State", Extensions = { "State" } });
            fileDialog.AllowMultiple = true;
            fileDialog.Directory = Path.GetFullPath($"./config/SaveStates/{appConfig!.GamePluginName}");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());

            if ((paths?.Length ?? 0) == 0)
            {
                return;
            }

            neuralPlayer!.LoadStates(paths!);
            loadedState = true;
            UpdateCanStart();
        }

        public void Start()
        {
            if (!CanStart) return;
            CanStart = false;

            new Thread(() =>
            {
                neuralPlayer!.StartPlaying();
                CanStop = true;
            }).Start();
        }

        public void Stop()
        {
            if (!CanStop) return;
            CanStop = false;
            if (neuralPlayer?.IsPlaying ?? false)
            {
                neuralPlayer!.StopPlaying();
            }
            CanStart = true;
        }

        public void Exit()
        {
            neuralPlayer!.Dispose();
            OnExit?.Invoke();
        }

        private void UpdateCanStart() => CanStart = loadedGenome && loadedState && !neuralPlayer!.IsPlaying;

        private void HandlePlayStopped()
        {
            UpdateCanStart();
            CanStop = false;
        }
    }
}
