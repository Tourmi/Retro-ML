using Avalonia.Controls;
using ReactiveUI;
using SMW_ML.Emulator;
using SMW_ML.Neural.Training;
using SMW_ML.Neural.Training.SharpNeat;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace SMW_ML.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private INeuralTrainer? trainer;
        private EmulatorManager? emulatorManager;
        private bool isTrainingRunning = false;
 
        public MainWindowViewModel()
        {

        }

        public string Greeting => "Super Mario World - Machine Learning";

        public string Start => "Start Training";
        public void StartTraining()
        {
            IsTrainingRunning = true;
            emulatorManager = new(12);
            trainer = new SharpNeatTrainer(emulatorManager);
            Thread t = new Thread(() => trainer.StartTraining("config/config.json"));
            t.Start();
        }

        public string Stop => "Stop Training";
        public void StopTraining()
        {
            trainer?.StopTraining();
            emulatorManager?.Dispose();
            emulatorManager = null;
            trainer = null;
            IsTrainingRunning = false;
        }

        public string OpenSettingsString => "Training Configuration";
        public void OpenConfigSettings(Window mainWindow)
        {
            var configViewModel = new ConfigurationViewModel();
            configViewModel.ShowWindow(mainWindow);
        }

        [DataMember]
        public bool IsTrainingRunning
        {
            get => isTrainingRunning;
            set => this.RaiseAndSetIfChanged(ref isTrainingRunning, value);
        }
    }
}
