using ReactiveUI;
using SMW_ML.Neural.Scoring;

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
    }
}
