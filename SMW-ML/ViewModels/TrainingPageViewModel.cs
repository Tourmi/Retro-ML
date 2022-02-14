using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeat;
using SMW_ML.Neural.Training.SharpNeatImpl;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels
{
    internal class TrainingPageViewModel : ViewModelBase
    {
        public event Action? OnStopTraining;

        private readonly INeuralTrainer trainer;
        private readonly EmulatorManager emulatorManager;

        private bool canStop = true;

        public TrainingPageViewModel()
        {
            //TODO : use config to setup training
            emulatorManager = new(2);
            TrainingStatistics = new ObservableCollection<TrainingStatistics.Stat>();
            trainer = new SharpNeatTrainer(emulatorManager);
            trainer.OnStatisticsUpdated += new Action<TrainingStatistics>(HandleGetStats);
        }

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

        public async void Init()
        {
            CanStop = true;

            await Task.Run(() =>
            {
                emulatorManager.Init();
                trainer.StartTraining(DefaultPaths.SHARPNEAT_CONFIG);
            });
        }

        public void LoadPopulation(string path)
        {
            trainer.LoadPopulation(path);
        }

        public void SavePopulation(string path)
        {
            trainer.SavePopulation(path);
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

        private ObservableCollection<TrainingStatistics.Stat> trainingStatistics;
        public ObservableCollection<TrainingStatistics.Stat> TrainingStatistics
        {
            get => trainingStatistics;
            set => this.RaiseAndSetIfChanged(ref trainingStatistics, value);
        }

    }
}
