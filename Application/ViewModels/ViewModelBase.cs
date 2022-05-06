using ReactiveUI;

namespace Retro_ML.Application.ViewModels
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
