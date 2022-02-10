using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SMW_ML.Views.Neural
{
    public partial class NetworkView : UserControl
    {
        public NetworkView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
