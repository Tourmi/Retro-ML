using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Application.Views.Components.FieldInfo
{
    public partial class IntegerView : UserControl
    {
        public IntegerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
