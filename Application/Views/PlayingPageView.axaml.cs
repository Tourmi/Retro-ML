using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Application.Views
{
    public partial class PlayingPageView : UserControl
    {
        public PlayingPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
