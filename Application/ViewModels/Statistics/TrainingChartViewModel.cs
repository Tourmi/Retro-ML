using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using Retro_ML.Neural.Train;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Retro_ML.Neural.Train.TrainingStatistics;

namespace Retro_ML.Application.ViewModels.Statistics;

internal class TrainingChartViewModel : ViewModelBase
{
    public class Filter : ViewModelBase
    {
        public event Action? OnFilterUpdated;

        public string Name { get; private set; }

        private string displayName;
        public string DisplayName
        {
            get { return displayName; }
            private set
            {
                this.RaiseAndSetIfChanged(ref displayName, value);
            }
        }

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                this.RaiseAndSetIfChanged(ref enabled, value);
            }
        }

        public Filter(string name, bool enabled)
        {
            Name = name;
            this.enabled = enabled;
            displayName = $"{(Enabled ? "Hide" : "Show")} {Name}";
        }

        public void Clicked()
        {
            Enabled = !Enabled;
            DisplayName = $"{(Enabled ? "Hide" : "Show")} {Name}";
            OnFilterUpdated?.Invoke();
        }
    }

    private Dictionary<string, ObservableCollection<double>> values;

    private int maxLabelLength = 1;
    private int maxValueLength = 1;

    public ObservableCollection<ISeries<double>> ShownSeries { get; set; }
    public List<Filter> Filters { get; set; }

    public TrainingChartViewModel()
    {
        values = new()
        {
            [BEST_GENOME_FITNESS] = new ObservableCollection<double>(),
            [MEAN_FITNESS] = new ObservableCollection<double>(),
            [MAX_COMPLEXITY] = new ObservableCollection<double>(),
            [BEST_GENOME_COMPLEXITY] = new ObservableCollection<double>(),
            [MEAN_COMPLEXITY] = new ObservableCollection<double>(),
            [EVALS_PER_MINUTE] = new ObservableCollection<double>(),
            [SPECIES_COUNT] = new ObservableCollection<double>(),
            [BEST_SPECIES_POPULATION] = new ObservableCollection<double>(),
            [AVERAGE_SPECIES_POPULATION] = new ObservableCollection<double>(),
        };

        maxLabelLength = values.Max(v => v.Key.Length);
        maxValueLength = 4;

        ShownSeries = new ObservableCollection<ISeries<double>>();

        Filters = new List<Filter>();
        foreach (var value in values)
        {
            var filter = new Filter(value.Key, value.Key == BEST_GENOME_FITNESS || value.Key == MEAN_FITNESS);
            Filters.Add(filter);
            filter.OnFilterUpdated += FiltersUpdated;

            ISeries<double> series = new LineSeries<double>()
            {
                Values = values[filter.Name],
                Name = filter.Name,
                LineSmoothness = 0,
                TooltipLabelFormatter = FormatTooltip,
                IsVisible = filter.Enabled,
                GeometrySize = 10
            };

            ShownSeries.Add(series);
        }
    }

    public void AddGeneration(TrainingStatistics stats)
    {
        foreach (var stat in stats.GetStats())
        {
            if (values.ContainsKey(stat.Name))
            {
                values[stat.Name].Add(stat.Value);
            }
        }

        maxValueLength = Math.Max(stats.GetStats().Append(new Stat()).Max(s => s.Value.ToString("N2").Length), maxValueLength);
    }

    public void ClearData()
    {
        foreach (var val in values.Values)
        {
            val.Clear();
        }
    }

    public void FiltersUpdated()
    {
        foreach (var filter in Filters)
        {
            ShownSeries.Single(ss => ss.Name == filter.Name).IsVisible = filter.Enabled;
        }
    }

    private string FormatTooltip(ChartPoint p)
    {
        return $"{p.Context.Series.Name!.PadRight(maxLabelLength, ' ')} : {p.PrimaryValue.ToString("N2").PadLeft(maxValueLength, ' ')}";
    }
}
