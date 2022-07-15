using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Retro_ML.Application.Views;
public partial class DebugWindow : Window
{
    public DebugWindow()
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
