using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Retro_ML.Application.ViewModels;
using Retro_ML.Application.Views;

namespace Retro_ML.Application
{
    public class App : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
