using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeatImpl;
using SMW_ML.Utils;
using SMW_ML.ViewModels.Neural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels
{
    internal class TrainingPageViewModel : ViewModelBase
    {
        public event Action? OnStopTraining;

        private INeuralTrainer trainer;
        private EmulatorManager emulatorManager;
        private NetworkViewModel neuralNetwork;

        private bool canStop = true;

        public TrainingPageViewModel()
        {
            //TODO : use config to setup training




        }

        public void Init()
        {
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = JsonConvert.DeserializeObject<ApplicationConfig>(appConfigJson)!;

            NeuralConfig neuralConfig = new();
            NeuralNetwork = new NetworkViewModel(neuralConfig);
            emulatorManager = new(appConfig, neuralConfig);
            trainer = new SharpNeatTrainer(emulatorManager, appConfig);

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

        public NetworkViewModel NeuralNetwork
        {
            get => neuralNetwork;
            set => this.RaiseAndSetIfChanged(ref neuralNetwork, value);
        }

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
