using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Views
{
    public partial class MainPageView : UserControl
    {
        public MainPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
