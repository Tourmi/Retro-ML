using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Views.Components
{
    public partial class ScoreFactorView : UserControl
    {
        public ScoreFactorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

}