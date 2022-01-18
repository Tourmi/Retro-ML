using ReactiveUI;
using SMW_ML.Emulator;
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
        private INeuralTrainer? trainer;
        private IEmulatorAdapter? emulator;
        private bool isTrainingRunning = false;

        public MainWindowViewModel()
        {
            
        }

        public string Greeting => "Super Mario World - Machine Learning";

        public string Start => "Start Training";
        public void StartTraining()
        {
            IsTrainingRunning = true;
            emulator = new BizhawkAdapter(pathToEmulator: null, pathToLuaScript: null, pathToROM: null, pathToBizhawkConfig: null, savestatesPath: null);
            trainer = new SharpNeatTrainer(emulator);
            trainer.StartTraining("config/config.json");
        }

        public string Stop => "Stop Training";
        public void StopTraining()
        {
            trainer?.StopTraining();
            emulator?.Dispose();
            emulator = null;
            trainer = null;
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
