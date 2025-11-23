using System.Collections.ObjectModel;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Communicator.Core.UX;

namespace Communicator.UX.Analytics.ViewModels;

/// <summary>
/// ViewModel responsible for managing the real-time sentiment line graph.
/// Supports dynamic scrolling and time-window visualization.
/// </summary>
public class GraphViewModel : ObservableObject
{
    /// <summary>
    /// Collection of time-series points used by the sentiment line chart.
    /// </summary>
    public ObservableCollection<ObservablePoint> Points { get; } = new();

    /// <summary>Chart series rendered on the line graph.</summary>
    public ISeries[] Series { get; }

    /// <summary>X-axis configuration (time axis).</summary>
    public Axis[] XAxes { get; }

    /// <summary>Y-axis configuration (sentiment axis).</summary>
    public Axis[] YAxes { get; }

    /// <summary>
    /// Number of seconds visible in the scrolling chart window.
    /// </summary>
    public double WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Initializes the sentiment graph, setting up series and axes.
    /// </summary>
    public GraphViewModel()
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
                Name = "Time (s)",
                NamePaint = new SolidColorPaint(SKColors.White),
                NameTextSize = 14,
                TextSize = 12,

                // Initial time window
                MinLimit = 0,
                MaxLimit = WindowSeconds,

                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                TicksPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(80, 80, 80)),

                // Format numeric labels
                Labeler = v => v.ToString("0"),

                MinStep = 5
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "Sentiment",
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
        var textPrimary = GetThemeColor("TextPrimaryColor");
        var textSecondary = GetThemeColor("TextSecondaryColor");
        var borderColor = GetThemeColor("BorderColor");

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
    /// Adds a new data point using time value <paramref name="t"/> and sentiment <paramref name="val"/>.
    /// Automatically adjusts the X-axis window when the data exceeds the visible duration.
    /// </summary>
    public void AddPoint(double t, double val)
    {
        Points.Add(new ObservablePoint(t, val));

        if (t > WindowSeconds)
        {
            XAxes[0].MinLimit = t - WindowSeconds;
            XAxes[0].MaxLimit = t;
        }
    }

    /// <summary>
    /// Adds a new data point (alternate method).
    /// Automatically scrolls when <paramref name="x"/> exceeds 60 seconds.
    /// </summary>
    public void Add(double x, double y)
    {
        Points.Add(new ObservablePoint(x, y));

        if (x > WindowSeconds)
        {
            XAxes[0].MinLimit = x - WindowSeconds;
            XAxes[0].MaxLimit = x;
        }
    }
}
