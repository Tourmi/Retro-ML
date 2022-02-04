using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeatImpl;
using SMW_ML.Utils;
using SMW_ML.ViewModels.Neural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels
{
    internal class TrainingPageViewModel : ViewModelBase
    {
        public event Action? OnStopTraining;

        private readonly INeuralTrainer trainer;
        private readonly EmulatorManager emulatorManager;

        private bool canStop = true;

        public TrainingPageViewModel(NeuralConfig neuralConfig)
        {
            //TODO : use config to setup training
            NeuralNetwork = new NetworkViewModel(neuralConfig);
            emulatorManager = new(6, neuralConfig);
            trainer = new SharpNeatTrainer(emulatorManager);
        }

        public void Init()
        {
            CanStop = true;

            new Thread(() =>
            {
                emulatorManager.Init();
                emulatorManager.GetFirstEmulator().LinkedNetworkActivated += NeuralNetwork.UpdateNodes;
                emulatorManager.GetFirstEmulator().ChangedLinkedNetwork += NeuralNetwork.UpdateTopology;
                trainer.StartTraining(DefaultPaths.SHARPNEAT_CONFIG);
            }).Start();
        }

        public void LoadPopulation(string path)
        {
            trainer.LoadPopulation(path);
        }

        public void SavePopulation(string path)
        {
            trainer.SavePopulation(path);
        }

        public NetworkViewModel NeuralNetwork { get; set; }

        public static string Status => "Currently training AIs";

        public static string Stop => "Stop Training";
        public async void StopTraining()
        {
            if (!CanStop) return;

            CanStop = false;
            await Task.Run(() =>
            {
                trainer?.StopTraining();
                emulatorManager?.Clean();
            });

            Dispatcher.UIThread.Post(() =>
            {
                OnStopTraining?.Invoke();
            });
        }
        public bool CanStop
        {
            get => canStop;
            set => this.RaiseAndSetIfChanged(ref canStop, value);
        }
    }
}
