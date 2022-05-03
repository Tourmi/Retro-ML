using ReactiveUI;
using Retro_ML.Neural.Training.StopCondition;

namespace Retro_ML.ViewModels.Components
{
    internal class StopConditionViewModel : ViewModelBase
    {
        private IStopCondition stopCondition;

        public string Name => stopCondition.Name;
        private bool _isChecked;
        public bool IsChecked
        {
            get => stopCondition.ShouldUse;
            set
            {
                stopCondition.ShouldUse = value;
                this.RaiseAndSetIfChanged(ref _isChecked, value);
            }
        }
        public bool HasParam => stopCondition.HasParam;
        public string ParamName => stopCondition.ParamName;
        private int _paramValue;
        public int ParamValue
        {
            get => stopCondition.ParamValue;
            set
            {
                stopCondition.ParamValue = value;
                this.RaiseAndSetIfChanged(ref _paramValue, value);
            }
        }

        public StopConditionViewModel(IStopCondition stopCondition)
        {
            this.stopCondition = stopCondition;
        }

        public IStopCondition GetStopCondition() => stopCondition;
    }
}
