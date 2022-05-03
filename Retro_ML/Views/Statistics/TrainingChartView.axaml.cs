using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Views.Statistics
{
    public partial class TrainingChartView : UserControl
    {
        public TrainingChartView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
