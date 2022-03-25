using ReactiveUI;
using SMW_ML.Neural.Scoring;
using System.Collections.ObjectModel;

namespace SMW_ML.ViewModels.Components
{
    internal class ScoreFactorViewModel : ViewModelBase
    {
        public static string EnabledString => "Enabled: ";
        public static string MultiplierString => "Multiplier: ";

        public ScoreFactorViewModel(IScoreFactor scoreFactor)
        {
            this.Name = scoreFactor.Name;
            this.CanBeDisabled = scoreFactor.CanBeDisabled;
            this.isEnabled = !scoreFactor.IsDisabled;
            this.multiplier = scoreFactor.ScoreMultiplier;
            this.ExtraFields = new ObservableCollection<ExtraField>(scoreFactor.ExtraFields);
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

        public ObservableCollection<ExtraField> ExtraFields { get; set; }
    }
}
