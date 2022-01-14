using ReactiveUI;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeat;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SMW_ML.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private readonly INeuralTrainer trainer;
        private bool isTrainingRunning = false;

        public MainWindowViewModel()
        {
            trainer = new SharpNeatTrainer();
        }

        public string Greeting => "Super Mario World - Machine Learning";

        public string Start => "Start Training";
        public void StartTraining()
        {
            IsTrainingRunning = true;
            trainer.StartTraining("config/config.json");
        }

        public string Stop => "Stop Training";
        public void StopTraining()
        {
            trainer.StopTraining();
            IsTrainingRunning = false;
        }

        [DataMember]
        public bool IsTrainingRunning
        {
            get => isTrainingRunning;
            set => this.RaiseAndSetIfChanged(ref isTrainingRunning, value);
        }
    }
}
