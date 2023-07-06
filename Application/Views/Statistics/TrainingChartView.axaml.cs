using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Retro_ML.Application.Views.Statistics;

public partial class TrainingChartView : UserControl
{
    public TrainingChartView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var chart = this.FindControl<CartesianChart>("TrainingChart");
        chart.TooltipTextPaint = new SolidColorPaint(new SKColor(30, 30, 30)) { FontFamily = "Courier New", SKFontStyle = SKFontStyle.Bold };
        chart.TooltipTextSize = 12;
    }
}
