using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Views
{
    public partial class TrainingPageView : UserControl
    {
        public TrainingPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
