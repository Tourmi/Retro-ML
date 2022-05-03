using Avalonia.Controls;
using ReactiveUI;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models;
using SMW_ML.Models.Config;
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

        #endregion

        #region Properties
        private SharpNeatModel? SharpNeatModel;
        private ApplicationConfig? ApplicationConfig;

        public int[] RayCounts => Raycast.POSSIBLE_RAY_COUNT;
        public int[] PossibleClockLengths => new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 16 };

        public ObservableCollection<Error> ErrorList { get; set; }
        public ObservableCollection<string> DispMethodList { get; set; }

        private string _romPath;
        [DataMember]
        public string RomPath
        {
            get => _romPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _romPath, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RomPath)));
            }
        }

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

        private double _initialInterconnectionsProportion;
        [DataMember]
        public double InitialInterconnectionsProportion
        {
            get => _initialInterconnectionsProportion;
            set
            {
                this.RaiseAndSetIfChanged(ref _initialInterconnectionsProportion, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialInterconnectionsProportion)));
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
        public ObservableCollection<StopConditionViewModel> StopConditions { get; set; }

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

        private int _soundVolume;
        public int SoundVolume
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

        private int _soundVolumePlayMode;
        public int SoundVolumePlayMode
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

        private List<string> _saveStates;
        private string _saveStatePreview;
        public string SaveStates
        {
            get => _saveStatePreview;
            set
            {
                this.RaiseAndSetIfChanged(ref _saveStatePreview, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveStates)));
            }
        }

        private void SetSaveStates(List<string> saveStates)
        {
            this._saveStates = saveStates;
            SaveStates = string.Join("\n", saveStates);
        }

        public ObservableCollection<ScoreFactorViewModel> Objectives { get; set; }

        private bool _useVisionGrid = false;
        public bool UseVisionGrid
        {
            get => _useVisionGrid;
            set => this.RaiseAndSetIfChanged(ref _useVisionGrid, value);
        }
        private int _viewDistanceHorizontal = 4;
        public int ViewDistanceHorizontal
        {
            get => _viewDistanceHorizontal;
            set => this.RaiseAndSetIfChanged(ref _viewDistanceHorizontal, value);
        }
        private int _viewDistanceVertical = 4;
        public int ViewDistanceVertical
        {
            get => _viewDistanceVertical;
            set => this.RaiseAndSetIfChanged(ref _viewDistanceVertical, value);
        }

        private int _rayLength = 6;
        public int RayLength
        {
            get => _rayLength;
            set => this.RaiseAndSetIfChanged(ref _rayLength, value);
        }
        private int _rayCountIndex = 2;
        public int RayCount
        {
            get => _rayCountIndex;
            set => this.RaiseAndSetIfChanged(ref _rayCountIndex, value);
        }

        private int _clockLength = 8;
        public int ClockLength
        {
            get => _clockLength;
            set => this.RaiseAndSetIfChanged(ref _clockLength, value);
        }
        private int _clockTickLength = 1;
        public int ClockTickLength
        {
            get => _clockTickLength;
            set => this.RaiseAndSetIfChanged(ref _clockTickLength, value);
        }


        public ObservableCollection<InputOutputConfigViewModel> NeuralConfigs { get; }


        public new event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #endregion

        #region Constructor
        public ConfigurationViewModel()
        {
            DispMethodList = new ObservableCollection<string>()
            {
                "OpenGL",
                "GDI+",
                "Direct3D9"
            };

            ErrorList = new ObservableCollection<Error>();
            PropertyChanged += HandlePropertyChanged;
            StopConditions = new();

            Objectives = new();

            _romPath = "smw.sfc";

            _saveStates = new List<string>();
            _saveStatePreview = "";
            NeuralConfigs = new ObservableCollection<InputOutputConfigViewModel>();

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
            window.Icon = mainWindow.Icon;
            window.ShowDialog(mainWindow);
        }


        /// <summary>
        /// Serializes the settings and saves it in the json configuration files
        /// </summary>
        private void SerializeConfig()
        {
            //Tab SharpNeat
            if (SharpNeatModel == null) { return; }
            SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount = SpeciesCount;
            SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion = ElitismProportion;
            SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion = SelectionProportion;
            SharpNeatModel.PopulationSize = NumberAI;
            SharpNeatModel.DegreeOfParallelism = Multithread;
            SharpNeatModel.InitialInterconnectionsProportion = InitialInterconnectionsProportion;

            string sharpNeatOutput = SharpNeatModel.Serialize();
            File.WriteAllText(DefaultPaths.SHARPNEAT_CONFIG, sharpNeatOutput);

            //Tab Emulator
            var bizHawkConfig = BizhawkConfig.Deserialize(File.ReadAllText(DefaultPaths.EMULATOR_CONFIG));
            bizHawkConfig.SoundEnabled = SoundEnabled;
            bizHawkConfig.Volume = SoundVolume;
            bizHawkConfig.Unthrottled = Unthrottled;
            bizHawkConfig.ZoomFactor = ZoomFactor;
            bizHawkConfig.DispMethod = DispMethod;
            bizHawkConfig.DispSpeedupFeatures = (int)(DispSpeedupFeatures ? DispSpeedupFeaturesEnum.Activated : DispSpeedupFeaturesEnum.Deactivated);
            File.WriteAllText(DefaultPaths.EMULATOR_CONFIG, bizHawkConfig.Serialize());

            //Tab Emulator Play Mode
            var bizHawkConfigPlayMode = BizhawkConfig.Deserialize(File.ReadAllText(DefaultPaths.EMULATOR_PLAY_CONFIG));
            bizHawkConfigPlayMode.SoundEnabled = SoundEnabledPlayMode;
            bizHawkConfigPlayMode.Volume = SoundVolumePlayMode;
            bizHawkConfigPlayMode.Unthrottled = UnthrottledPlayMode;
            bizHawkConfigPlayMode.ZoomFactor = ZoomFactorPlayMode;
            bizHawkConfigPlayMode.DispMethod = DispMethodPlayMode;
            bizHawkConfigPlayMode.DispSpeedupFeatures = (int)(DispSpeedupFeaturesPlayMode ? DispSpeedupFeaturesEnum.Activated : DispSpeedupFeaturesEnum.Deactivated);
            File.WriteAllText(DefaultPaths.EMULATOR_PLAY_CONFIG, bizHawkConfigPlayMode.Serialize());

            //Tab Application
            if (ApplicationConfig == null) { return; }

            ApplicationConfig.RomPath = RomPath;
            ApplicationConfig.Multithread = Multithread;
            ApplicationConfig.ArduinoCommunicationPort = ArduinoPort!;
            for (int i = 0; i < StopConditions.Count; i++)
            {
                ApplicationConfig.StopConditions[i].ShouldUse = StopConditions[i].IsChecked;
                ApplicationConfig.StopConditions[i].ParamValue = StopConditions[i].ParamValue;
            }
            ApplicationConfig.SaveStates = _saveStates;

            //Tab Objectives
            for (int i = 0; i < Objectives.Count; i++)
            {
                ApplicationConfig.ScoreFactors[i].ScoreMultiplier = Objectives[i].Multiplier;
                if (ApplicationConfig.ScoreFactors[i].CanBeDisabled)
                {
                    ApplicationConfig.ScoreFactors[i].IsDisabled = !Objectives[i].IsChecked;
                }
                for (int j = 0; j < ApplicationConfig.ScoreFactors[i].ExtraFields.Count(); j++)
                {
                    ApplicationConfig.ScoreFactors[i].ExtraFields[j].Value = Objectives[i].ExtraFields[j].Value;
                }
            }

            //Tab Neural
            ApplicationConfig.NeuralConfig.UseGrid = UseVisionGrid;
            ApplicationConfig.NeuralConfig.GridDistanceX = ViewDistanceHorizontal;
            ApplicationConfig.NeuralConfig.GridDistanceY = ViewDistanceVertical;

            ApplicationConfig.NeuralConfig.RayLength = RayLength;
            ApplicationConfig.NeuralConfig.RayCount = RayCount;

            ApplicationConfig.NeuralConfig.InternalClockLength = ClockLength;
            ApplicationConfig.NeuralConfig.InternalClockTickLength = ClockTickLength;

            int inputCount = ApplicationConfig.NeuralConfig.InputNodes.Count;
            int outputCount = ApplicationConfig.NeuralConfig.OutputNodes.Count;
            for (int i = 0; i < inputCount; i++)
            {
                ApplicationConfig.NeuralConfig.EnabledStates[i] = NeuralConfigs![i].IsChecked;
            }
            for (int i = inputCount; i < inputCount + outputCount; i++)
            {
                ApplicationConfig.NeuralConfig.EnabledStates[i] = NeuralConfigs![i].IsChecked;
            }

            string appOutput = ApplicationConfig.Serialize();
            File.WriteAllText(DefaultPaths.APP_CONFIG, appOutput);
        }

        /// <summary>
        /// Deserializes the configuration files into objects
        /// </summary>
        private void DeserializeConfig()
        {
            //Tab NeuralNetwork
            string configJSon = File.ReadAllText(DefaultPaths.SHARPNEAT_CONFIG);
            SharpNeatModel = SharpNeatModel.Deserialize(configJSon);

            if (SharpNeatModel == null) { return; }

            SpeciesCount = SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount;
            ElitismProportion = SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion;
            SelectionProportion = SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion;
            NumberAI = SharpNeatModel.PopulationSize;
            InitialInterconnectionsProportion = SharpNeatModel.InitialInterconnectionsProportion;

            //Tab Emulator
            var bizhawkConfig = BizhawkConfig.Deserialize(File.ReadAllText(DefaultPaths.EMULATOR_CONFIG));
            SoundEnabled = bizhawkConfig.SoundEnabled;
            SoundVolume = bizhawkConfig.Volume;
            Unthrottled = bizhawkConfig.Unthrottled;
            ZoomFactor = bizhawkConfig.ZoomFactor;
            DispMethod = bizhawkConfig.DispMethod;
            DispSpeedupFeatures = bizhawkConfig.DispSpeedupFeatures == (int)DispSpeedupFeaturesEnum.Activated;

            //Tab Emulator Play Mode
            var bizhawkConfigPlayMode = BizhawkConfig.Deserialize(File.ReadAllText(DefaultPaths.EMULATOR_PLAY_CONFIG));
            SoundEnabledPlayMode = bizhawkConfigPlayMode.SoundEnabled;
            SoundVolumePlayMode = bizhawkConfigPlayMode.Volume;
            UnthrottledPlayMode = bizhawkConfigPlayMode.Unthrottled;
            ZoomFactorPlayMode = bizhawkConfigPlayMode.ZoomFactor;
            DispMethodPlayMode = bizhawkConfigPlayMode.DispMethod;
            DispSpeedupFeaturesPlayMode = bizhawkConfigPlayMode.DispSpeedupFeatures == (int)DispSpeedupFeaturesEnum.Activated;

            //Tab Application
            string appConfigJson = File.ReadAllText(DefaultPaths.APP_CONFIG);
            ApplicationConfig = ApplicationConfig.Deserialize(appConfigJson);

            if (ApplicationConfig == null) { return; }

            RomPath = ApplicationConfig.RomPath;
            Multithread = ApplicationConfig.Multithread;
            ArduinoPort = ApplicationConfig.ArduinoCommunicationPort;

            StopConditions.Clear();
            for (int i = 0; i < ApplicationConfig.StopConditions.Count; i++)
            {
                StopConditions.Add(new(ApplicationConfig.StopConditions[i]));
            }

            SetSaveStates(ApplicationConfig.SaveStates);

            //Tab Objectives
            Objectives.Clear();
            foreach (var obj in ApplicationConfig.ScoreFactors)
            {
                Objectives.Add(new(obj));
            }

            //Tab Neural
            PopulateNeuralConfig();
        }

        public async void SelectRom()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Rom", Extensions = { "sfc" } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());

            if (paths == null) return;

            string localPath = Path.GetFullPath(".");
            RomPath = Path.GetFullPath(paths[0]).Replace(localPath, "").Trim('/', '\\');
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
            string localPath = Path.GetFullPath(".");
            foreach (string path in paths)
            {
                saveStates.Add(Path.GetFullPath(path).Replace(localPath, "").Trim('/', '\\'));
            }
            SetSaveStates(saveStates);
        }

        public async void LoadNeuralConfig()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Neural Config", Extensions = { DefaultPaths.NEURAL_CONFIG_EXTENSION } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());
            if (paths == null) return;

            string neuralConfigJson = await File.ReadAllTextAsync(paths.First());
            ApplicationConfig!.NeuralConfig = NeuralConfig.Deserialize(neuralConfigJson);
            PopulateNeuralConfig();
        }

        private void PopulateNeuralConfig()
        {
            using var delay = DelayChangeNotifications();
            NeuralConfigs.Clear();
            foreach (var input in ApplicationConfig!.NeuralConfig.InputNodes)
            {
                NeuralConfigs.Add(new(input));
            }
            foreach (var output in ApplicationConfig.NeuralConfig.OutputNodes)
            {
                NeuralConfigs.Add(new(output));
            }

            UseVisionGrid = ApplicationConfig.NeuralConfig.UseGrid;
            ViewDistanceHorizontal = ApplicationConfig.NeuralConfig.GridDistanceX;
            ViewDistanceVertical = ApplicationConfig.NeuralConfig.GridDistanceY;

            RayLength = ApplicationConfig.NeuralConfig.RayLength;
            RayCount = ApplicationConfig.NeuralConfig.RayCount;

            ClockLength = ApplicationConfig.NeuralConfig.InternalClockLength;
            ClockTickLength = ApplicationConfig.NeuralConfig.InternalClockTickLength;
        }

        #region Validation
        private void ValidateFields()
        {
            ValidateSpeciesCount();
            ValidateElitismProportion();
            ValidateSelectionProportion();
            ValidateMultithread();
            ValidateArduinoPort();
            ValidateROM();
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

        private void ValidateROM()
        {
            if (!File.Exists(RomPath))
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "ROM Path",
                    Description = "The current ROM path is invalid."
                });
            }
        }

        private void ValidateSaveStates()
        {
            if (!_saveStates.Any())
            {
                ErrorList.Add(new Error()
                {
                    FieldError = "Save States",
                    Description = "You must select at least one save state for training"
                });
            }
            foreach (var ss in _saveStates)
            {
                if (!File.Exists(ss))
                {
                    ErrorList.Add(new Error()
                    {
                        FieldError = "Save States",
                        Description = "One of the save states does not exist."
                    });
                }
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
