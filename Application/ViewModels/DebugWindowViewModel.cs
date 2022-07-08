using Avalonia.Controls;
using ReactiveUI;
using Retro_ML.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Retro_ML.Application.ViewModels;
internal class DebugWindowViewModel : ViewModelBase, IDisposable
{
    private const double sleepTime = 1.0 / 60.0;
    private bool shouldStop = false;
    private readonly Semaphore sem = new(1, 1);
    private double timeSinceLastUpdate = 0;

    private int refreshRate = 10;
    public int RefreshRate
    {
        get => refreshRate;
        set
        {
            this.RaiseAndSetIfChanged(ref refreshRate, value);
            IsRefreshVisible = value == 0;
        }
    }

    private string debugText = "";
    public string DebugText { get => debugText; set => this.RaiseAndSetIfChanged(ref debugText, value); }

    private string[] categories;
    public string[] Categories { get => categories; set => this.RaiseAndSetIfChanged(ref categories, value); }

    private List<string> selectedCategories;
    public List<string> SelectedCategories
    {
        get => selectedCategories;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedCategories, value);
            Refresh();
        }
    }

    private bool isRefreshVisible;
    public bool IsRefreshVisible { get => isRefreshVisible; set => this.RaiseAndSetIfChanged(ref isRefreshVisible, value); }

    public DebugWindowViewModel()
    {
        _ = sem.WaitOne();
        categories = Array.Empty<string>();
        selectedCategories = new();
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

    public void Refresh()
    {
        Categories = DebugInfo.GetCategories();
        DebugText = DebugInfo.GetFormattedInfo(selectedCategories.ToArray());
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
                Refresh();
                timeSinceLastUpdate = 0;
            }
        }
        _ = sem.Release();
    }
}
