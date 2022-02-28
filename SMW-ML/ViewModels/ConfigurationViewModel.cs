using Avalonia.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;
using SMW_ML.Models;
using SMW_ML.Models.Config;
using SMW_ML.Neural.Scoring;
using SMW_ML.Utils;
using SMW_ML.ViewModels.Components;
using SMW_ML.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace SMW_ML.ViewModels
{
    internal class ConfigurationViewModel : ViewModelBase
    {
        private static readonly JsonSerializerSettings JSON_CONFIG = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        private enum DispMethodEnum : ushort
        {
            OpenGL = 0,
            GDI = 1,
            Direct3D9 = 2
        }

        private enum DispSpeedupFeaturesEnum : ushort
        {
            Deactivated = 0,
            Activated = 2
        }

        #region Strings
        public static string NumberOfAIString => "Number of AI";
        public static string GeneralTrainingSettingsString => "Training Settings";
        public static string EvolutionSettingsString => "Evolution Algorithm Settings";
        public static string SpeciesCountString => "Species Count";
        public static string ButtonSaveString => "Save";
        public static string ButtonCloseString => "Close";
        public static string ElitismProportionString => "Elitism Proportion";
        public static string SelectionProportionString => "Selection Proportion";
        public static string TabItemSharpNeat => "Neural Network";
        public static string TabItemBizhawk => "Emulator";
        public static string TabItemApp => "Application";
        public static string TabItemObjectives => "Objectives";

        #endregion

        #region Properties
        private SharpNeatModel? SharpNeatModel;
        private ApplicationConfig? ApplicationConfig;

        public ObservableCollection<Error> ErrorList { get; set; }
        public ObservableCollection<string> DispMethodList { get; set; }

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
        public List<IScoreFactor> ScoreFactors;

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

        private bool _isDataGridErrorVisible;
        public bool IsDataGridErrorVisible
        {
            get { return _isDataGridErrorVisible; }
            set
            {
                this.RaiseAndSetIfChanged(ref _isDataGridErrorVisible, value);
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

        private string? _arduinoPort;
        [DataMember]
        public string? ArduinoPort
        {
            get => _arduinoPort;
            set
            {
                this.RaiseAndSetIfChanged(ref _arduinoPort, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArduinoPort)));
            }
        }

        public bool _soundEnabled;
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _soundEnabled, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SoundEnabled)));
            }
        }

        private float _soundVolume;
        public float SoundVolume
        {
            get => _soundVolume;
            set
            {
                this.RaiseAndSetIfChanged(ref _soundVolume, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SoundVolume)));
            }
        }

        private bool _unthrottled;
        public bool Unthrottled
        {
            get => _unthrottled;
            set
            {
                this.RaiseAndSetIfChanged(ref _unthrottled, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unthrottled)));
            }
        }

        private int _zoomFactor;
        public int ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                this.RaiseAndSetIfChanged(ref _zoomFactor, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomFactor)));
            }
        }

        private int _dispMethod;
        public int DispMethod
        {
            get => _dispMethod;
            set
            {
                this.RaiseAndSetIfChanged(ref _dispMethod, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DispMethod)));
            }
        }

        private bool _dispSpeedupFeatures;
        public bool DispSpeedupFeatures
        {
            get => _dispSpeedupFeatures;
            set
            {
                this.RaiseAndSetIfChanged(ref _dispSpeedupFeatures, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DispSpeedupFeatures)));
            }
        }

        public bool _soundEnabledPlayMode;
        public bool SoundEnabledPlayMode
        {
            get => _soundEnabledPlayMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _soundEnabledPlayMode, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SoundEnabledPlayMode)));
            }
        }

        private float _soundVolumePlayMode;
        public float SoundVolumePlayMode
        {
            get => _soundVolumePlayMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _soundVolumePlayMode, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SoundVolumePlayMode)));
            }
        }

        private bool _unthrottledPlayMode;
        public bool UnthrottledPlayMode
        {
            get => _unthrottledPlayMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _unthrottledPlayMode, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnthrottledPlayMode)));
            }
        }

        private int _zoomFactorPlayMode;
        public int ZoomFactorPlayMode
        {
            get => _zoomFactorPlayMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _zoomFactorPlayMode, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomFactorPlayMode)));
            }
        }

        private int _dispMethodPlayMode;
        public int DispMethodPlayMode
        {
            get => _dispMethodPlayMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _dispMethodPlayMode, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DispMethodPlayMode)));
            }
        }

        private bool _dispSpeedupFeaturesPlayMode;
        public bool DispSpeedupFeaturesPlayMode
        {
            get => _dispSpeedupFeaturesPlayMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _dispSpeedupFeaturesPlayMode, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DispSpeedupFeaturesPlayMode)));
            }
        }

        private List<string> saveStates;
        private string saveStatePreview;
        public string SaveStates
        {
            get => saveStatePreview;
            set
            {
                this.RaiseAndSetIfChanged(ref saveStatePreview, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveStates)));
            }
        }

        private void SetSaveStates(List<string> saveStates)
        {
            this.saveStates = saveStates;
            SaveStates = string.Join("\n", saveStates);
        }

        public List<ScoreFactorViewModel>? Objectives { get; set; }


        public new event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

            DispMethodList = new ObservableCollection<string>()
            {
                "OpenGL",
                "GDI+",
                "Direct3D9"
            };

            ErrorList = new ObservableCollection<Error>();
            PropertyChanged += HandlePropertyChanged;
            ScoreFactors = new List<IScoreFactor>();
            saveStates = new List<string>();

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
        public void ShowWindow(Window mainWindow)
        {
            var window = new Configuration
            {
                DataContext = this,
                Width = 700,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.ShowDialog(mainWindow);
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

            string sharpNeatOutput = JsonConvert.SerializeObject(SharpNeatModel, Formatting.Indented, JSON_CONFIG);
            File.WriteAllText(DefaultPaths.SHARPNEAT_CONFIG, sharpNeatOutput);

            //Tab Emulator
            var bizHawkConfig = new BizhawkConfig(DefaultPaths.EMULATOR_CONFIG);
            bizHawkConfig.SoundEnabled = SoundEnabled;
            bizHawkConfig.Volume = SoundVolume;
            bizHawkConfig.Unthrottled = Unthrottled;
            bizHawkConfig.ZoomFactor = ZoomFactor;
            bizHawkConfig.DispMethod = DispMethod;
            bizHawkConfig.DispSpeedupFeatures = (int)(DispSpeedupFeatures ? DispSpeedupFeaturesEnum.Activated : DispSpeedupFeaturesEnum.Deactivated);
            bizHawkConfig.Serialize(DefaultPaths.EMULATOR_CONFIG);

            //Tab Emulator Play Mode
            var bizHawkConfigPlayMode = new BizhawkConfig(DefaultPaths.EMULATOR_PLAY_CONFIG);
            bizHawkConfigPlayMode.SoundEnabled = SoundEnabledPlayMode;
            bizHawkConfigPlayMode.Volume = SoundVolumePlayMode;
            bizHawkConfigPlayMode.Unthrottled = UnthrottledPlayMode;
            bizHawkConfigPlayMode.ZoomFactor = ZoomFactorPlayMode;
            bizHawkConfigPlayMode.DispMethod = DispMethodPlayMode;
            bizHawkConfigPlayMode.DispSpeedupFeatures = (int)(DispSpeedupFeaturesPlayMode ? DispSpeedupFeaturesEnum.Activated : DispSpeedupFeaturesEnum.Deactivated);
            bizHawkConfigPlayMode.Serialize(DefaultPaths.EMULATOR_PLAY_CONFIG);

            //Tab Application
            if (ApplicationConfig == null) { return; }

            ApplicationConfig.Multithread = Multithread;
            ApplicationConfig.ArduinoCommunicationPort = ArduinoPort!;
            ApplicationConfig.StopTrainingCondition = StopTrainingSelectedItem!;
            ApplicationConfig.StopTrainingConditionValue = StopTrainingConditionValue;
            ApplicationConfig.SaveStates = saveStates;

            //Tab Objectives
            for (int i = 0; i < Objectives!.Count; i++)
            {
                ApplicationConfig.ScoreFactors[i].ScoreMultiplier = Objectives[i].Multiplier;
                if (ApplicationConfig.ScoreFactors[i].CanBeDisabled)
                {
                    ApplicationConfig.ScoreFactors[i].IsDisabled = !Objectives[i].IsEnabled;
                }
            }

            string appOutput = JsonConvert.SerializeObject(ApplicationConfig, Formatting.Indented, JSON_CONFIG);
            File.WriteAllText(DefaultPaths.APP_CONFIG, appOutput);
        }

        /// <summary>
        /// Deserializes the configuration files into objects
        /// </summary>
        private void DeserializeConfig()
        {
            //Tab NeuralNetwork
            string configJSon = File.ReadAllText(DefaultPaths.SHARPNEAT_CONFIG);
            SharpNeatModel = JsonConvert.DeserializeObject<SharpNeatModel>(configJSon);

            if (SharpNeatModel == null) { return; }

            SpeciesCount = SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount;
            ElitismProportion = SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion;
            SelectionProportion = SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion;
            NumberAI = SharpNeatModel.PopulationSize;

            //Tab Emulator
            var bizhawkConfig = new BizhawkConfig(DefaultPaths.EMULATOR_CONFIG);
            SoundEnabled = bizhawkConfig.SoundEnabled;
            SoundVolume = bizhawkConfig.Volume;
            Unthrottled = bizhawkConfig.Unthrottled;
            ZoomFactor = bizhawkConfig.ZoomFactor;
            DispMethod = bizhawkConfig.DispMethod;
            DispSpeedupFeatures = bizhawkConfig.DispSpeedupFeatures == (int)DispSpeedupFeaturesEnum.Activated;

            //Tab Emulator Play Mode
            var bizhawkConfigPlayMode = new BizhawkConfig(DefaultPaths.EMULATOR_PLAY_CONFIG);
            SoundEnabledPlayMode = bizhawkConfigPlayMode.SoundEnabled;
            SoundVolumePlayMode = bizhawkConfigPlayMode.Volume;
            UnthrottledPlayMode = bizhawkConfigPlayMode.Unthrottled;
            ZoomFactorPlayMode = bizhawkConfigPlayMode.ZoomFactor;
            DispMethodPlayMode = bizhawkConfigPlayMode.DispMethod;
            DispSpeedupFeaturesPlayMode = bizhawkConfigPlayMode.DispSpeedupFeatures == (int)DispSpeedupFeaturesEnum.Activated;

            //Tab Application
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig = JsonConvert.DeserializeObject<ApplicationConfig>(appConfigJson, JSON_CONFIG);

            if (ApplicationConfig == null) { return; }

            Multithread = ApplicationConfig.Multithread;
            ArduinoPort = ApplicationConfig.ArduinoCommunicationPort;
            StopTrainingSelectedItem = ApplicationConfig.StopTrainingCondition;
            StopTrainingConditionValue = ApplicationConfig.StopTrainingConditionValue;

            SetSaveStates(ApplicationConfig.SaveStates);

            //Tab Objectives
            Objectives = new List<ScoreFactorViewModel>();
            foreach (var obj in ApplicationConfig.ScoreFactors)
            {
                Objectives.Add(new(obj));
            }
        }

        public async void SelectSaveStates()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Save states", Extensions = { "State" } });
            fileDialog.AllowMultiple = true;
            fileDialog.Directory = Path.GetFullPath("./config/SaveStates");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());

            List<string> saveStates = new List<string>();
            if (paths == null)
            {
                return;
            }
            foreach (string path in paths)
            {
                saveStates.Add(path);
            }
            SetSaveStates(saveStates);
        }

        #region Validation
        private void ValidateFields()
        {
            ValidateSpeciesCount();
            ValidateElitismProportion();
            ValidateSelectionProportion();
            ValidateMultithread();
            ValidateArduinoPort();
            ValidateStopTrainingCondition();
            ValidateStopTrainingValue();
            ValidateSaveStates();
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

        private void ValidateSaveStates()
        {
            if (!saveStates.Any())
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Save States",
                    Description = "You must select at least one save state for training"
                });
            }
        }
        #endregion

        #region Events
        private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ErrorList))
                return;
            if (e.PropertyName == nameof(IsButtonSaveEnabled))
                return;

            ErrorList.Clear();

            ValidateFields();

            if (ErrorList.Count > 0)
            {
                IsButtonSaveEnabled = false;
                IsDataGridErrorVisible = true;
            }
            else
            {
                IsButtonSaveEnabled = true;
                IsDataGridErrorVisible = false;
            }
        }

        #endregion


    }
}
