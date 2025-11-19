//using LiveChartsCore;
//using LiveChartsCore.Defaults;
//using LiveChartsCore.SkiaSharpView;
//using System.Collections.ObjectModel;

//namespace AnalyticsApp.ViewModels;

///// <summary>
///// A single graph: holds points + chart series.
///// </summary>
//public class GraphViewModel
//{
//    public ObservableCollection<DateTimePoint> Points { get; } = new();

//    public ISeries[] Series { get; }
//    public Axis[] XAxes { get; }
//    public Axis[] YAxes { get; }

//    public GraphViewModel()
//    {
//        Series = new ISeries[]
//        {
//            new LineSeries<DateTimePoint>
//            {
//                Values = Points,
//                GeometrySize = 6,
//                LineSmoothness = 0,
//                Fill = null
//            }
//        };

//    }
//}
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AnalyticsApp.ViewModels;

/// <summary>
/// ViewModel responsible for managing the real-time sentiment line graph.
/// Supports dynamic scrolling and time-window visualization.
/// </summary>
public class GraphViewModel
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
                NamePaint = new SolidColorPaint(SKColors.Black),
                NameTextSize = 18,
                TextSize = 14,

                // Initial time window
                MinLimit = 0,
                MaxLimit = WindowSeconds,

                LabelsPaint = new SolidColorPaint(SKColors.DimGray),
                TicksPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),

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
                NamePaint = new SolidColorPaint(SKColors.Black),
                NameTextSize = 18,
                TextSize = 14
            }
        };
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




//public void AddPoint(double timeSeconds, double value)
//{
//    Points.Add(new ObservablePoint(timeSeconds, value));

//    // Sliding window
//    double min = Math.Max(0, timeSeconds - WindowSeconds);
//    double max = timeSeconds;

//    XAxes[0].MinLimit = min;
//    XAxes[0].MaxLimit = max;
//}