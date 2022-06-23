using ReactiveUI;
using Retro_ML.Application.ViewModels.Components.FieldInfo;
using Retro_ML.Neural.Scoring;
using System.Collections.ObjectModel;

namespace Retro_ML.Application.ViewModels.Components
{
    internal class ScoreFactorViewModel : ViewModelBase
    {
        public ScoreFactorViewModel(IScoreFactor scoreFactor)
        {
            Name = scoreFactor.Name;
            Tooltip = scoreFactor.Tooltip;
            CanBeDisabled = scoreFactor.CanBeDisabled;
            isEnabled = !scoreFactor.IsDisabled;
            multiplier = scoreFactor.ScoreMultiplier;

            FieldInfos = new ObservableCollection<FieldInfoViewModel>();

            foreach (var fieldInfo in scoreFactor.Fields)
            {
                FieldInfos.Add(FieldInfoViewModel.GetFieldInfoViewModel(fieldInfo, scoreFactor[fieldInfo.Name]));
            }
        }

        public string Name { get; }
        public string Tooltip { get; }
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

        public ObservableCollection<FieldInfoViewModel> FieldInfos { get; set; }
    }
}
