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

        #endregion

        #region Properties
        private SharpNeatModel? SharpNeatModel;
        private bool isStopManually = true;
        [DataMember]
        public bool IsStopManually
        {
            get => isStopManually;
            set => this.RaiseAndSetIfChanged(ref isStopManually, value);
        }

        public ObservableCollection<string> StopTrainingItems { get; set; }

        private int _speciesCount;
        [DataMember]
        public int SpeciesCount
        {
            get => _speciesCount;
            set => this.RaiseAndSetIfChanged(ref _speciesCount, value);
        }

        private int _numberAI;
        [DataMember]
        public int NumberAI
        {
            get => _numberAI;
            set => this.RaiseAndSetIfChanged(ref _numberAI, value);
        }

        private double _elitismProportion;
        [DataMember]
        public double ElitismProportion
        {
            get => _elitismProportion;
            set => this.RaiseAndSetIfChanged(ref _elitismProportion, value);
        }

        private double _selectionProportion;
        [DataMember]
        public double SelectionProportion
        {
            get => _selectionProportion;
            set => this.RaiseAndSetIfChanged(ref _selectionProportion, value);
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

            //Initialize the properties with the current config
            if (!Design.IsDesignMode)
            {
                DeserializeConfig();
            }
            
            _stopTrainingSelectedItem = "Manually";
            IsStopManually = true;

        }
        #endregion

        private string _stopTrainingSelectedItem;
        public string StopTrainingSelectedItem
        {
            get { return _stopTrainingSelectedItem; }
            set
            {
                _stopTrainingSelectedItem = value;
                if (_stopTrainingSelectedItem.Equals("Manually"))
                {
                    IsStopManually = true;
                }
                else
                {
                    IsStopManually = false;
                }
            }
        }

        /// <summary>
        /// Saves the configuration
        /// </summary>
        public void SaveSettings(Window window)
        {
            //string configJSon = File.ReadAllText("config/sharpNeatConfig.json");
            //var sharpNeatConfig = JsonConvert.DeserializeObject<SharpNeatModel>(configJSon);

            //if (sharpNeatConfig == null) { return; }

            //sharpNeatConfig.EvolutionAlgorithmSettings.SpeciesCount = this.SpeciesCount;

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
            window.Width = 500;
            window.Height = 480;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }

        /// <summary>
        /// Serializes the settings and saves it in the json configuration files
        /// </summary>
        private void SerializeConfig()
        {
            if (SharpNeatModel == null) { return; }
            SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount = SpeciesCount;
            SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion = ElitismProportion;
            SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion = SelectionProportion;
            SharpNeatModel.PopulationSize = NumberAI;

            string output = JsonConvert.SerializeObject(SharpNeatModel, Formatting.Indented);
            File.WriteAllText("config/sharpNeatConfig.json", output);
        }

        /// <summary>
        /// Deserializes the configuration files into objects
        /// </summary>
        private void DeserializeConfig()
        {
            string configJSon = File.ReadAllText("config/sharpNeatConfig.json");
            SharpNeatModel = JsonConvert.DeserializeObject<SharpNeatModel>(configJSon);
            if (SharpNeatModel == null) { return; }

            SpeciesCount = SharpNeatModel.EvolutionAlgorithmSettings.SpeciesCount;
            ElitismProportion = SharpNeatModel.EvolutionAlgorithmSettings.ElitismProportion;
            SelectionProportion = SharpNeatModel.EvolutionAlgorithmSettings.SelectionProportion;
            NumberAI = SharpNeatModel.PopulationSize;
        }

    }
}
