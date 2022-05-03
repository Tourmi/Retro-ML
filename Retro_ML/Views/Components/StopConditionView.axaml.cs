using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SMW_ML.Views.Components
{
    public partial class StopConditionView : UserControl
    {
        public StopConditionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
