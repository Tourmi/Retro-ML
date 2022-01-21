using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SMW_ML.Views
{
    public partial class Configuration : Window
    {
        public Configuration()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
