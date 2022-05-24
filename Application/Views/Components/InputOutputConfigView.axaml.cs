using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Application.Views.Components
{
    public partial class InputOutputConfigView : UserControl
    {
        public InputOutputConfigView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
