using Avalonia.Controls;
using ReactiveUI;
using Retro_ML.Utils;
using System;
using System.Threading;

namespace Retro_ML.Application.ViewModels;
internal class DebugWindowViewModel : ViewModelBase, IDisposable
{
    private const double sleepTime = 1.0 / 60.0;
    private bool shouldStop = false;
    private readonly Semaphore sem = new(1, 1);
    private double timeSinceLastUpdate = 0;

    private int refreshRate = 10;
    public int RefreshRate { get => refreshRate; set => this.RaiseAndSetIfChanged(ref refreshRate, value); }

    private string debugText = "";
    public string DebugText { get => debugText; set => this.RaiseAndSetIfChanged(ref debugText, value); }

    public DebugWindowViewModel()
    {
        _ = sem.WaitOne();
        new Thread(RefreshLoop).Start();
    }

    public void ShowWindow(Window mainWindow)
    {
        var window = new Views.DebugWindow
        {
            DataContext = this,
            Width = 700,
            Height = 700,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Icon = mainWindow.Icon
        };
        window.Show();
    }

    public void Clear()
    {
        DebugInfo.ClearInfo();
        DebugText = string.Empty;
    }

    public void Dispose()
    {
        shouldStop = true;
        _ = sem.WaitOne();
        _ = sem.Release();
    }


    private void RefreshLoop()
    {
        while (!shouldStop)
        {
            Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
            timeSinceLastUpdate += sleepTime;
            if (timeSinceLastUpdate >= 1.0 / RefreshRate)
            {
                DebugText = DebugInfo.GetFormattedInfo();
                timeSinceLastUpdate = 0;
            }
        }
        _ = sem.Release();
    }
}
