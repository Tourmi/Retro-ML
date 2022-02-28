using Avalonia.Threading;
using Newtonsoft.Json;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeat;
using SMW_ML.Neural.Training.SharpNeatImpl;
using SMW_ML.Utils;
using SMW_ML.ViewModels.Neural;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels
{
    internal class TrainingPageViewModel : ViewModelBase
    {
        public event Action? OnStopTraining;

        private INeuralTrainer trainer;
        private EmulatorManager emulatorManager;
        private string? populationToLoad;

        #region Strings
        public static string Status => "Currently training AIs";
        public static string Stop => "Stop Training";
        #endregion

        #region Properties

        private bool canStop = true;
        public bool CanStop
        {
            get => canStop;
            set => this.RaiseAndSetIfChanged(ref canStop, value);
        }

        private NetworkViewModel neuralNetwork;
        public NetworkViewModel NeuralNetwork
        {
            get => neuralNetwork;
            set => this.RaiseAndSetIfChanged(ref neuralNetwork, value);
        }

        public ObservableCollection<TrainingStatistics.Stat> TrainingStatistics { get; set; }

        #endregion

        #region Constructor
        public TrainingPageViewModel()
        {
            TrainingStatistics = new ObservableCollection<TrainingStatistics.Stat>();
        }
        #endregion

        #region  Methods

        public void Init()
        {
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig appConfig = JsonConvert.DeserializeObject<ApplicationConfig>(appConfigJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace })!;

            NeuralConfig neuralConfig = new();
            NeuralNetwork = new NetworkViewModel(neuralConfig);
            emulatorManager = new(appConfig, neuralConfig);
            trainer = new SharpNeatTrainer(emulatorManager, appConfig);
            trainer.OnStatisticsUpdated += HandleGetStats;
            if (populationToLoad != null)
            {
                trainer.LoadPopulation(populationToLoad);
                populationToLoad = null;
            }

            CanStop = true;

            new Thread(() =>
            {
                emulatorManager.Init(false);
                emulatorManager.GetFirstEmulator().LinkedNetworkActivated += NeuralNetwork.UpdateNodes;
                emulatorManager.GetFirstEmulator().ChangedLinkedNetwork += NeuralNetwork.UpdateTopology;
                trainer.StartTraining(DefaultPaths.SHARPNEAT_CONFIG);
            }).Start();
        }

        public void LoadPopulation(string path)
        {
            populationToLoad = path;
        }

        public void SavePopulation(string path)
        {
            trainer.SavePopulation(path);
        }

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
        #endregion

        #region Events

        private void HandleGetStats(TrainingStatistics stats)
        {
            Dispatcher.UIThread.Post(() =>
            {
                TrainingStatistics.Clear();
                foreach (var stat in stats.GetStats())
                {
                    TrainingStatistics.Add(stat);
                }
            });
        }
        #endregion

    }
}
