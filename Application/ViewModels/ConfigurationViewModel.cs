﻿using Avalonia.Controls;
using ReactiveUI;
using Retro_ML.Application.Models;
using Retro_ML.Application.ViewModels.Components;
using Retro_ML.Application.ViewModels.Components.FieldInfo;
using Retro_ML.Configuration;
using Retro_ML.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Retro_ML.Application.ViewModels
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

        public ObservableCollection<Error> ErrorList { get; set; }
        public ObservableCollection<string> GamePlugins { get; set; }
        public ObservableCollection<string> DispMethodList { get; set; }

        public ObservableCollection<FieldInfoViewModel> GamePluginConfigFields { get; }

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

        private string _gamePlugin;
        [DataMember]
        public string GamePluginName
        {
            get => _gamePlugin;
            set
            {
                this.RaiseAndSetIfChanged(ref _gamePlugin, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GamePluginName)));
                ReloadGamePlugin();
            }
        }

        private string _pluginGameName;
        [DataMember]
        public string PluginGameName
        {
            get => _pluginGameName;
            set
            {
                this.RaiseAndSetIfChanged(ref _pluginGameName, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PluginGameName)));
            }
        }

        private string _pluginConsoleName;
        [DataMember]
        public string PluginConsoleName
        {
            get => _pluginConsoleName;
            set
            {
                this.RaiseAndSetIfChanged(ref _pluginConsoleName, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PluginConsoleName)));
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
            _saveStates = saveStates;
            SaveStates = string.Join("\n", saveStates);
        }

        public ObservableCollection<ScoreFactorViewModel> Objectives { get; set; }

        private int _shortTermMemoryCount;
        public int ShortTermMemoryCount
        {
            get => _shortTermMemoryCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _shortTermMemoryCount, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShortTermMemoryCount)));
            }
        }
        private int _longTermMemoryCount;
        public int LongTermMemoryCount
        {
            get => _longTermMemoryCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _longTermMemoryCount, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LongTermMemoryCount)));
            }
        }
        private int _permanentMemoryCount;
        public int PermanentMemoryCount
        {
            get => _permanentMemoryCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _permanentMemoryCount, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PermanentMemoryCount)));
            }
        }
        private double _memoryMaximumValue;
        public double MaximumMemoryValue
        {
            get => _memoryMaximumValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _memoryMaximumValue, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaximumMemoryValue)));
            }
        }
        public ObservableCollection<InputOutputConfigViewModel> NeuralConfigs { get; }

        public new event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor
        public ConfigurationViewModel()
        {
            GamePluginConfigFields = new ObservableCollection<FieldInfoViewModel>();
            GamePlugins = new();

            _pluginConsoleName = "";
            _gamePlugin = "";
            _pluginGameName = "";

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
            var window = new Views.Configuration
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
            ApplicationConfig.GamePluginName = GamePluginName;
            ApplicationConfig.Multithread = Multithread;
            ApplicationConfig.ArduinoCommunicationPort = ArduinoPort!;
            for (int i = 0; i < StopConditions.Count; i++)
            {
                ApplicationConfig.StopConditions[i].ShouldUse = StopConditions[i].IsChecked;
                ApplicationConfig.StopConditions[i].ParamValue = StopConditions[i].ParamValue;
            }
            ApplicationConfig.SaveStates = _saveStates;

            //Tab Game && Objectives
            SaveGamePluginConfig();

            //Tab Neural
            ApplicationConfig.NeuralConfig.ShortTermMemoryNodeCount = ShortTermMemoryCount;
            ApplicationConfig.NeuralConfig.LongTermMemoryNodeCount = LongTermMemoryCount;
            ApplicationConfig.NeuralConfig.PermanentMemoryNodeCount = PermanentMemoryCount;
            ApplicationConfig.NeuralConfig.MaximumMemoryNodeValue = MaximumMemoryValue;

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
            //Plugins
            PluginUtils.LoadPlugins();
            foreach (var plugin in PluginUtils.GamePlugins)
            {
                GamePlugins.Add(plugin.PluginName);
            }

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

            LoadApplicationConfig();

            SetSaveStates(ApplicationConfig.SaveStates);

            //Tab Game
            LoadGamePluginConfig();

            //Tab Objectives
            LoadObjectives();

            //Tab Neural
            PopulateNeuralConfig();
        }

        public void LoadApplicationConfig()
        {
            if (ApplicationConfig == null) { return; }

            RomPath = ApplicationConfig.RomPath;
            GamePluginName = ApplicationConfig.GamePluginName;
            Multithread = ApplicationConfig.Multithread;
            ArduinoPort = ApplicationConfig.ArduinoCommunicationPort;

            StopConditions.Clear();
            for (int i = 0; i < ApplicationConfig.StopConditions.Count; i++)
            {
                StopConditions.Add(new(ApplicationConfig.StopConditions[i]));
            }
        }

        public void LoadGamePluginConfig()
        {
            GamePluginConfigFields.Clear();

            if (ApplicationConfig?.GamePluginConfig == null) return;

            var pluginConfig = ApplicationConfig.GamePluginConfig;

            foreach (var fieldInfo in pluginConfig.Fields)
            {
                GamePluginConfigFields.Add(FieldInfoViewModel.GetFieldInfoViewModel(fieldInfo, pluginConfig[fieldInfo.Name]));
            }
        }

        private void LoadObjectives()
        {
            Objectives.Clear();
            foreach (var obj in ApplicationConfig!.GamePluginConfig!.ScoreFactors)
            {
                Objectives.Add(new(obj));
            }
        }

        public void SaveGamePluginConfig()
        {
            if (ApplicationConfig?.GamePluginConfig == null) return;

            var pluginConfig = ApplicationConfig.GamePluginConfig;

            foreach (var field in GamePluginConfigFields)
            {
                pluginConfig[field.FieldName] = field.GetValue();
            }

            var scoreFactors = ApplicationConfig.GamePluginConfig.ScoreFactors;
            for (int i = 0; i < scoreFactors.Count; i++)
            {
                if (scoreFactors[i].CanBeDisabled) scoreFactors[i].IsDisabled = !Objectives[i].IsChecked;
                scoreFactors[i].ScoreMultiplier = Objectives[i].Multiplier;

                foreach (var field in Objectives[i].FieldInfos)
                {
                    scoreFactors[i][field.FieldName] = field.GetValue();
                }
            }

            string pluginConfigPath = ApplicationConfig.GetGamePlugin().PluginConfigPath;
            Directory.CreateDirectory(Path.GetDirectoryName(pluginConfigPath)!);
            File.WriteAllText(pluginConfigPath, ApplicationConfig!.GamePluginConfig!.Serialize());
        }

        public async void SelectRom()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Rom", Extensions = ApplicationConfig!.GetConsolePlugin().ROMExtensions.ToList() });
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
            ApplicationConfig.GamePluginConfig!.InitNeuralConfig(ApplicationConfig.NeuralConfig);

            PopulateNeuralConfig();
        }

        public async void LoadGameConfig()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filters.Add(new FileDialogFilter() { Name = "Game Plugin Config", Extensions = { DefaultPaths.GAME_PLUGIN_CONFIG_EXTENSION } });
            fileDialog.AllowMultiple = false;
            fileDialog.Directory = Path.GetFullPath(".");

            string[]? paths = await fileDialog.ShowAsync(ViewLocator.GetMainWindow());
            if (paths == null) return;

            string gamePluginConfig = await File.ReadAllTextAsync(paths.First());

            ApplicationConfig!.GamePluginConfig!.Deserialize(gamePluginConfig);

            //Tab Game
            LoadGamePluginConfig();

            //Tab Objectives
            LoadObjectives();

        }

        private void PopulateNeuralConfig()
        {
            using var delay = DelayChangeNotifications();
            ShortTermMemoryCount = ApplicationConfig!.NeuralConfig.ShortTermMemoryNodeCount;
            LongTermMemoryCount = ApplicationConfig.NeuralConfig.LongTermMemoryNodeCount;
            PermanentMemoryCount = ApplicationConfig.NeuralConfig.PermanentMemoryNodeCount;
            MaximumMemoryValue = ApplicationConfig.NeuralConfig.MaximumMemoryNodeValue;

            NeuralConfigs.Clear();
            foreach (var input in ApplicationConfig.NeuralConfig.InputNodes)
            {
                NeuralConfigs.Add(new(input));
            }
            foreach (var output in ApplicationConfig.NeuralConfig.OutputNodes)
            {
                NeuralConfigs.Add(new(output));
            }
        }

        private void ReloadGamePlugin()
        {
            ApplicationConfig!.GamePluginName = GamePluginName;
            var gamePlugin = ApplicationConfig!.GetGamePlugin();
            PluginGameName = gamePlugin.PluginGameName;
            PluginConsoleName = ApplicationConfig.GetConsolePlugin().ConsoleName;

            PopulateNeuralConfig();
            LoadGamePluginConfig();
            LoadObjectives();
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
