using Avalonia.Controls;
using Newtonsoft.Json;
using ReactiveUI;
using SMW_ML.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SMW_ML.Models.Config;
using System.ComponentModel;
using SMW_ML.Models;
using Avalonia;
using System.Runtime.CompilerServices;
using Avalonia.Data;

namespace SMW_ML.ViewModels
{
    internal class ConfigurationViewModel : ViewModelBase
    {

        #region Strings
        public string NumberOfAIString => "Number of AI";
        public string GeneralTrainingSettingsString => "Training Settings";
        public string EvolutionSettingsString => "Evolution Algorithm Settings";
        public string SpeciesCountString => "Species Count";
        public string ButtonSaveString => "Save";
        public string ButtonCloseString => "Close";
        public string ElitismProportionString => "Elitism Proportion";
        public string SelectionProportionString => "Selection Proportion";
        public string TabItemSharpNeat => "Neural Network";
        public string TabItemBizhawk => "Emulator";
        public string TabItemApp => "Application";

        #endregion

        #region Properties
        private SharpNeatModel? SharpNeatModel;
        private ApplicationConfig? ApplicationConfig;

        public ObservableCollection<Error> ErrorList { get; set; }

        private bool isStopManually = true;
        [DataMember]
        public bool IsStopManually
        {
            get => isStopManually;
            set => this.RaiseAndSetIfChanged(ref isStopManually, value);
        }

        public ObservableCollection<string> StopTrainingItems { get; set; }
        public ObservableCollection<string> ListOfObjectives { get; set; }

        private int _multithread;
        [DataMember]
        public int Multithread
        {
            get => _multithread;
            set
            {
                this.RaiseAndSetIfChanged(ref _multithread, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Multithread)));
            }
        }

        private int _speciesCount;
        [DataMember]
        public int SpeciesCount
        {
            get => _speciesCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _speciesCount, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeciesCount)));
            }
        }

        private int _numberAI;
        [DataMember]
        public int NumberAI
        {
            get => _numberAI;
            set
            {
                this.RaiseAndSetIfChanged(ref _numberAI, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberAI)));
            }
        }

        private double _elitismProportion;
        [DataMember]
        public double ElitismProportion
        {
            get => _elitismProportion;
            set
            {
                this.RaiseAndSetIfChanged(ref _elitismProportion, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElitismProportion)));
            }
        }

        private double _selectionProportion;
        [DataMember]
        public double SelectionProportion
        {
            get => _selectionProportion;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectionProportion, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionProportion)));
            }
        }

        private string? _stopTrainingSelectedItem;
        public string? StopTrainingSelectedItem
        {
            get { return _stopTrainingSelectedItem; }
            set
            {
                this.RaiseAndSetIfChanged(ref _stopTrainingSelectedItem, value);
                if (_stopTrainingSelectedItem != null)
                {
                    if (_stopTrainingSelectedItem.Equals("Manually"))
                    {
                        IsStopManually = true;
                    }
                    else
                    {
                        IsStopManually = false;
                    }
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopTrainingSelectedItem)));
            }
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set
            {
                this.RaiseAndSetIfChanged(ref _isButtonSaveEnabled, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsButtonSaveEnabled)));
            }
        }
        private string? _aiObjectiveSelectedItem;
        public string? SelectedObjective
        {
            get => _aiObjectiveSelectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _aiObjectiveSelectedItem, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedObjective)));
            }
        }

        private int? _stopTrainingConditionValue;
        [DataMember(IsRequired = true)]
        public int? StopTrainingConditionValue
        {
            get => _stopTrainingConditionValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _stopTrainingConditionValue, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopTrainingConditionValue)));
            }
        }

        private string _arduinoPort;
        [DataMember]
        public string ArduinoPort
        {
            get => _arduinoPort;
            set
            {
                this.RaiseAndSetIfChanged(ref _arduinoPort, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArduinoPort)));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        #endregion

        #region Constructor
        public ConfigurationViewModel()
        {
            StopTrainingItems = new ObservableCollection<string>() {
                "Number of Generations",
                "Amount of Time",
                "Manually"
            };

            ListOfObjectives = new ObservableCollection<string>(){
                "Best Score",
                "Maximize Lives",
                "Maximize Coins",
                "All Dragon Pieces"
            };

            ErrorList = new ObservableCollection<Error>();
            ErrorList.Add(new Error()
            {
                Description = "asd",
                FieldError = "lfd"
            });
            this.PropertyChanged += HandlePropertyChanged;

            //Initialize the properties with the current config
            if (!Design.IsDesignMode)
            {
                DeserializeConfig();
            }
        }
        #endregion

        /// <summary>
        /// Saves the configuration
        /// </summary>
        public void SaveSettings(Window window)
        {
            ValidateFields();
            if (ErrorList.Count > 0)
            {
                return;
            }

            //Saves the config
            SerializeConfig();

            if (window != null)
            {
                window.Close();
            }
        }

        /// <summary>
        /// Closes the configuration window
        /// </summary>
        public void CloseSettings(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        /// <summary>
        /// Shows the configuration window
        /// </summary>
        public void ShowWindow()
        {
            var window = new Configuration();
            window.DataContext = this;
            window.Width = 700;
            window.Height = 480;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }


        /// <summary>
        /// Serializes the settings and saves it in the json configuration files
        /// </summary>
        private void SerializeConfig()
        {
            //Tab NeuralNetwork
            if (SharpNeatModel == null) { return; }
            SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount = SpeciesCount;
            SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion = ElitismProportion;
            SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion = SelectionProportion;
            SharpNeatModel.PopulationSize = NumberAI;

            string sharpNeatOutput = JsonConvert.SerializeObject(SharpNeatModel, Formatting.Indented);
            File.WriteAllText("config/sharpNeatConfig.json", sharpNeatOutput);

            //Tab Emulator


            //Tab Application
            if (ApplicationConfig == null) { return; }

            ApplicationConfig.AIObjective = SelectedObjective;
            ApplicationConfig.Multithread = Multithread;
            ApplicationConfig.ArduinoCommunicationPort = ArduinoPort;
            ApplicationConfig.StopTrainingCondition = StopTrainingSelectedItem;
            ApplicationConfig.StopTrainingConditionValue = StopTrainingConditionValue;

            string appOutput = JsonConvert.SerializeObject(ApplicationConfig, Formatting.Indented);
            File.WriteAllText("config/appConfig.json", appOutput);
        }

        /// <summary>
        /// Deserializes the configuration files into objects
        /// </summary>
        private void DeserializeConfig()
        {
            //Tab NeuralNetwork
            string configJSon = File.ReadAllText("config/sharpNeatConfig.json");
            SharpNeatModel = JsonConvert.DeserializeObject<SharpNeatModel>(configJSon);

            if (SharpNeatModel == null) { return; }

            SpeciesCount = SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount;
            ElitismProportion = SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion;
            SelectionProportion = SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion;
            NumberAI = SharpNeatModel.PopulationSize;

            //Tab Emulator
            string emulatorConfig = File.ReadAllText("config/bizhawkConfig.ini");
            // EmulatorConfig = JsonConvert.DeserializeObject<BizhawkConfig>(emulatorConfig);

            //Tab Application
            string appConfigJson = File.ReadAllText("config/appConfig.json");
            ApplicationConfig = JsonConvert.DeserializeObject<ApplicationConfig>(appConfigJson);

            if (ApplicationConfig == null) { return; }

            Multithread = ApplicationConfig.Multithread;
            ArduinoPort = ApplicationConfig.ArduinoCommunicationPort;
            SelectedObjective = ApplicationConfig.AIObjective;
            StopTrainingSelectedItem = ApplicationConfig.StopTrainingCondition;
            StopTrainingConditionValue = ApplicationConfig.StopTrainingConditionValue;
        }

        #region Validation
        private void ValidateFields()
        {
            ValidateSpeciesCount();
            ValidateElitismProportion();
            ValidateSelectionProportion();
            ValidateAIObjective();
            ValidateMultithread();
            ValidateArduinoPort();
            ValidateStopTrainingCondition();
            ValidateStopTrainingValue();
        }

        private void ValidateSpeciesCount()
        {
            if (SpeciesCount < 1)
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Species Count",
                    Description = "Species count must be greater or equal to 1."
                });
            }
        }

        private void ValidateElitismProportion()
        {
            double elitism;
            bool isDouble = double.TryParse(ElitismProportion.ToString(), out elitism);
            if (!isDouble)
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Elitism Proportion",
                    Description = "Elitism proportion must be a number."
                });
            }
            if (ElitismProportion <= 0)
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Elitism Proportion",
                    Description = "Elitism proportion must be greater than 0."
                });
            }
        }

        private void ValidateSelectionProportion()
        {
            double elitism;
            bool isDouble = double.TryParse(SelectionProportion.ToString(), out elitism);
            if (!isDouble)
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Selection Proportion",
                    Description = "Selection proportion must be a number."
                });
            }
            if (SelectionProportion <= 0)
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Selection Proportion",
                    Description = "Selection proportion must be greater than 0."
                });
            }
        }

        private void ValidateMultithread()
        {
            if (Multithread <= 0)
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Multithread",
                    Description = "Multithread must be greater than 0."
                });
            }
        }

        private void ValidateArduinoPort()
        {
            if (string.IsNullOrEmpty(ArduinoPort))
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Arduino Port",
                    Description = "Arduino port cannot be empty."
                });
            }
        }

        private void ValidateAIObjective()
        {
            if (string.IsNullOrEmpty(SelectedObjective))
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "AI Objective",
                    Description = "You must select an objective."
                });
            }
        }

        private void ValidateStopTrainingValue()
        {
            if (StopTrainingSelectedItem != null && !StopTrainingSelectedItem.Equals("Manually"))
            {
                if (StopTrainingConditionValue <= 0 || StopTrainingConditionValue == null)
                {
                    ErrorList.Add(new Error()
                    {
                        FieldError = "Stop Training Value",
                        Description = "Stop training value must be greater than 0."
                    });
                }
            }
        }

        private void ValidateStopTrainingCondition()
        {
            if (string.IsNullOrEmpty(StopTrainingSelectedItem))
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Stop Training Condition",
                    Description = "You must select a stop training condition."
                });
            }
        }
        #endregion

        #region Events
        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ErrorList))
                return;
            if (e.PropertyName == nameof(IsButtonSaveEnabled))
                return;

            ErrorList.Clear();

            ValidateFields();

            if (ErrorList != null && ErrorList.Count > 0)
            {
                IsButtonSaveEnabled = false;
            }
            else
            {
                IsButtonSaveEnabled = true;
            }
        }

        #endregion


    }
}
