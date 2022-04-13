using ReactiveUI;
using SMW_ML.Game.SuperMarioWorld;

namespace SMW_ML.ViewModels.Components
{
    internal class InputOutputConfigViewModel : ViewModelBase
    {
        public InputOutputConfigViewModel(InputNode inputNode)
        {
            Name = inputNode.Name;
            IsInput = true;
            IsChecked = inputNode.ShouldUse;
        }

        public InputOutputConfigViewModel(OutputNode outputNode)
        {
            Name = outputNode.Name;
            IsInput = false;
            IsChecked = outputNode.ShouldUse;
        }

        public string Name { get; }
        public bool IsInput { get; }
        public bool IsOutput => !IsInput;
        private bool isEnabled;
        public bool IsChecked
        {
            get => isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }
    }
}
