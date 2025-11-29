using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Communicator.UX.Core;

namespace Communicator.UX.Analytics.ViewModels;

/// <summary>
/// ViewModel dedicated for ScreenShare sentiment graph.
/// Works like GraphViewModel but with different color styling.
/// </summary>
public class ScreenShareGraphViewModel : ObservableObject
{
    /// <summary>
    /// Time-series points for ScreenShare sentiment.
    /// </summary>
    public ObservableCollection<ObservablePoint> Points { get; } = new();

    /// <summary>Series displayed on the graph.</summary>
    public ISeries[] Series { get; }

    /// <summary>X-axis time settings.</summary>
    public Axis[] XAxes { get; }

    /// <summary>Y-axis settings.</summary>
    public Axis[] YAxes { get; }

    /// <summary>
    /// Amount of time (seconds) visible in the scrolling window.
    /// </summary>
    public double WindowSeconds { get; set; } = 80;

    public ScreenShareGraphViewModel()
    {
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = Points,
                GeometrySize = 6,
                LineSmoothness = 0.2,
                Fill = null,

                // ⭐ Different color for ScreenShare
                Stroke = new SolidColorPaint(SKColors.MediumPurple, 3)
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Name = "Time (s)",
                NamePaint = new SolidColorPaint(SKColors.Black),
                NameTextSize = 18,
                TextSize = 14,

                MinLimit = 0,
                MaxLimit = WindowSeconds,

                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TicksPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),

                Labeler = v => v.ToString("0"),
                MinStep = 5
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "ScreenShare Sentiment",
                NamePaint = new SolidColorPaint(SKColors.Black),
                NameTextSize = 18,
                TextSize = 14
            }
        };
    }

    /// <summary>
    /// Adds a new ScreenShare sentiment point.
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
    /// Alternate add method (same behavior).
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
