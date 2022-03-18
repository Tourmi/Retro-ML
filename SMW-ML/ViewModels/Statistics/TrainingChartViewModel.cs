using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using SMW_ML.Neural.Training;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static SMW_ML.Neural.Training.TrainingStatistics;

namespace SMW_ML.ViewModels.Statistics
{
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

        public ObservableCollection<ISeries<double>> ShownSeries { get; set; }
        public List<Filter> Filters { get; set; }

        public TrainingChartViewModel()
        {
            values = new()
            {
                [BEST_GENOME_FITNESS] = new ObservableCollection<double>(),
                [MEAN_FITNESS] = new ObservableCollection<double>(),
                [BEST_GENOME_COMPLEXITY] = new ObservableCollection<double>(),
                [MEAN_COMPLEXITY] = new ObservableCollection<double>(),
                [MAX_COMPLEXITY] = new ObservableCollection<double>(),
                [EVALS_PER_MINUTE] = new ObservableCollection<double>()
            };

            ShownSeries = new ObservableCollection<ISeries<double>>();

            Filters = new List<Filter>();
            foreach (var value in values)
            {
                var filter = new Filter(value.Key, value.Key == BEST_GENOME_FITNESS || value.Key == MEAN_FITNESS);
                Filters.Add(filter);
                filter.OnFilterUpdated += FiltersUpdated;
            }

            FiltersUpdated();
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
                ISeries<double>? series = ShownSeries.SingleOrDefault(ss => ss.Name == filter.Name);

                if (filter.Enabled && series == null)
                {
                    ShownSeries.Add(new LineSeries<double>()
                    {
                        Values = values[filter.Name],
                        Name = filter.Name,
                        LineSmoothness = 0
                    });
                }
                else if (!filter.Enabled && series != null)
                {
                    ShownSeries.Remove(series);
                }
            }
        }
    }
}
