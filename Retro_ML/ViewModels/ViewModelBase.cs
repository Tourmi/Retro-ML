using ReactiveUI;

namespace Retro_ML.ViewModels
{
    internal class ViewModelBase : ReactiveObject
    {
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { this.RaiseAndSetIfChanged(ref isEnabled, value); }
        }
    }
}
