using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SMW_ML.Views
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
