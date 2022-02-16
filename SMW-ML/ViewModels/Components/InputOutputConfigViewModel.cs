using ReactiveUI;
using SMW_ML.Game.SuperMarioWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels.Components
{
    internal class InputOutputConfigViewModel : ViewModelBase
    {
        public InputOutputConfigViewModel(InputNode inputNode)
        {
            Name = inputNode.Name;
            IsInput = true;
            IsEnabled = inputNode.ShouldUse;
        }

        public InputOutputConfigViewModel(OutputNode outputNode)
        {
            Name = outputNode.Name;
            IsInput = false;
            IsEnabled = outputNode.ShouldUse;
        }

        public string Name { get; }
        public bool IsInput { get; }
        public bool IsOutput => !IsInput;
        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }
    }
}
