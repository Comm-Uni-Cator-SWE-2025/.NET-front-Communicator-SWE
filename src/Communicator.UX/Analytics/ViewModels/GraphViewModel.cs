using System.Collections.ObjectModel;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Communicator.UX.Core;

namespace Communicator.UX.Analytics.ViewModels;

/// <summary>
/// ViewModel responsible for managing the real-time sentiment line graph.
/// Supports dynamic scrolling and time-window visualization with time labels.
/// </summary>
public class GraphViewModel : ObservableObject
{
    /// <summary>
    /// Collection of time-series points used by the sentiment line chart.
    /// </summary>
    public ObservableCollection<ObservablePoint> Points { get; } = new();

    /// <summary>
    /// Collection of time labels corresponding to each point index.
    /// </summary>
    private readonly List<string> _timeLabels = new();

    /// <summary>Chart series rendered on the line graph.</summary>
    public ISeries[] Series { get; }

    /// <summary>X-axis configuration (time axis).</summary>
    public Axis[] XAxes { get; }

    /// <summary>Y-axis configuration (sentiment axis).</summary>
    public Axis[] YAxes { get; }

    /// <summary>
    /// Number of points visible in the scrolling chart window.
    /// </summary>
    public int WindowSize { get; set; } = 10;

    /// <summary>
    /// Initializes the graph with default "Sentiment" Y-axis label.
    /// </summary>
    public GraphViewModel() : this("Sentiment")
    {
    }

    /// <summary>
    /// Initializes the graph with a custom Y-axis label.
    /// </summary>
    /// <param name="yAxisName">The name to display on the Y-axis</param>
    public GraphViewModel(string yAxisName)
    {
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = Points,
                GeometrySize = 6,
                LineSmoothness = 0,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 3)
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Name = "Time",
                NamePaint = new SolidColorPaint(SKColors.White),
                NameTextSize = 14,
                TextSize = 12,

                // Initial time window
                MinLimit = 0,
                MaxLimit = WindowSize,

                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                TicksPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(80, 80, 80)),

                // Format labels using time strings
                Labeler = v =>
                {
                    int index = (int)Math.Round(v);
                    if (index >= 0 && index < _timeLabels.Count)
                    {
                        return _timeLabels[index];
                    }
                    return string.Empty;
                },

                MinStep = 1
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = yAxisName,
                NamePaint = new SolidColorPaint(SKColors.White),
                NameTextSize = 14,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(80, 80, 80))
            }
        };

        ApplyTheme();
    }

    public void ApplyTheme()
    {
        SKColor textPrimary = GetThemeColor("TextPrimaryColor");
        SKColor textSecondary = GetThemeColor("TextSecondaryColor");
        SKColor borderColor = GetThemeColor("BorderColor");

        if (XAxes != null && XAxes.Length > 0)
        {
            XAxes[0].NamePaint = new SolidColorPaint(textPrimary);
            XAxes[0].LabelsPaint = new SolidColorPaint(textSecondary);
            XAxes[0].SeparatorsPaint = new SolidColorPaint(borderColor);
        }

        if (YAxes != null && YAxes.Length > 0)
        {
            YAxes[0].NamePaint = new SolidColorPaint(textPrimary);
            YAxes[0].LabelsPaint = new SolidColorPaint(textSecondary);
            YAxes[0].SeparatorsPaint = new SolidColorPaint(borderColor);
        }
    }

    private static SKColor GetThemeColor(string key)
    {
        if (Application.Current != null && Application.Current.Resources.Contains(key) && Application.Current.Resources[key] is System.Windows.Media.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }
        return SKColors.Gray;
    }

    /// <summary>
    /// Adds a new data point with a time label.
    /// </summary>
    /// <param name="timeLabel">The time label to display (e.g., "10:01")</param>
    /// <param name="sentiment">The sentiment value</param>
    public void AddPointWithLabel(string timeLabel, double sentiment)
    {
        int index = Points.Count;
        _timeLabels.Add(timeLabel);
        Points.Add(new ObservablePoint(index, sentiment));

        // Adjust window to show latest points
        if (index >= WindowSize)
        {
            XAxes[0].MinLimit = index - WindowSize + 1;
            XAxes[0].MaxLimit = index + 1;
        }
        else
        {
            XAxes[0].MinLimit = 0;
            XAxes[0].MaxLimit = Math.Max(WindowSize, index + 1);
        }
    }

    /// <summary>
    /// Adds a new data point using time value and sentiment (legacy numeric method).
    /// </summary>
    public void AddPoint(double t, double val)
    {
        Points.Add(new ObservablePoint(t, val));

        if (t > WindowSize)
        {
            XAxes[0].MinLimit = t - WindowSize;
            XAxes[0].MaxLimit = t;
        }
    }

    /// <summary>
    /// Adds a new data point (alternate method).
    /// </summary>
    public void Add(double x, double y)
    {
        Points.Add(new ObservablePoint(x, y));

        if (x > WindowSize)
        {
            XAxes[0].MinLimit = x - WindowSize;
            XAxes[0].MaxLimit = x;
        }
    }

    /// <summary>
    /// Clears all data points and time labels.
    /// </summary>
    public void Clear()
    {
        Points.Clear();
        _timeLabels.Clear();
        XAxes[0].MinLimit = 0;
        XAxes[0].MaxLimit = WindowSize;
    }
}
