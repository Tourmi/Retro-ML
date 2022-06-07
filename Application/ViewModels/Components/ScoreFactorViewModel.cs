using ReactiveUI;
using Retro_ML.Neural.Scoring;
using System.Collections.ObjectModel;
using Retro_ML.Configuration.FieldInformation;
using Retro_ML.Configuration;
using Retro_ML.Application.ViewModels.Components.FieldInfo;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Retro_ML.Application.ViewModels.Components
{
    internal class ScoreFactorViewModel : ViewModelBase
    {
        public static string EnabledString => "Enabled: ";
        public static string MultiplierString => "Multiplier: ";

        public ScoreFactorViewModel(IScoreFactor scoreFactor)
        {
            Name = scoreFactor.Name;
            CanBeDisabled = scoreFactor.CanBeDisabled;
            isEnabled = !scoreFactor.IsDisabled;
            multiplier = scoreFactor.ScoreMultiplier;
            ExtraFields = new ObservableCollection<ExtraField>(scoreFactor.ExtraFields);

            FieldInfos = new ObservableCollection<ViewModelBase>();

            foreach (var fieldInfo in scoreFactor.Fields)
            {
                FieldInfos.Add(FieldInfoViewModel.GetFieldInfoViewModel(fieldInfo, scoreFactor[fieldInfo.Name]));
            }
        }

        public string Name { get; }
        public bool CanBeDisabled { get; }
        private bool isEnabled;
        public bool IsChecked
        {
            get => isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }
        private double multiplier;
        public double Multiplier
        {
            get => multiplier;
            set => this.RaiseAndSetIfChanged(ref multiplier, value);
        }

        public ObservableCollection<ViewModelBase> FieldInfos { get; set; }

        public ObservableCollection<ExtraField> ExtraFields { get; set; }
    }
}
