using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Application.Views.Neural
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
