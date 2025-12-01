using System.Collections.ObjectModel;
using Communicator.UX.Analytics.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Communicator.UX.Core;
using System.Windows;

namespace Communicator.UX.Analytics.ViewModels;

/// <summary>
/// ViewModel responsible for managing and updating the bar chart
/// representing canvas shape counts (Graph 2).
/// </summary>
public class CanvasGraphViewModel : ObservableObject
{
    /// <summary>Free-hand shape counts across snapshots.</summary>
    public ObservableCollection<int> FreeHand { get; } = new();

    /// <summary>Straight-line shape counts across snapshots.</summary>
    public ObservableCollection<int> Line { get; } = new();

    /// <summary>Rectangle shape counts across snapshots.</summary>
    public ObservableCollection<int> Rectangle { get; } = new();

    /// <summary>Ellipse shape counts across snapshots.</summary>
    public ObservableCollection<int> Ellipse { get; } = new();

    /// <summary>Triangle shape counts across snapshots.</summary>
    public ObservableCollection<int> Triangle { get; } = new();

    /// <summary>Labels for each snapshot (T1, T2, T3, ...).</summary>
    public ObservableCollection<string> Labels { get; } = new();

    /// <summary>Chart series configuration for LiveCharts.</summary>
    public ISeries[] Series { get; }

    /// <summary>X-axis configuration for the bar chart.</summary>
    public Axis[] XAxes { get; }

    /// <summary>Y-axis configuration for the bar chart.</summary>
    public Axis[] YAxes { get; }

    /// <summary>
    /// Initializes chart series and axes for the canvas bar graph.
    /// </summary>
    public CanvasGraphViewModel()
    {
        Series = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = FreeHand,
                Name = "Free Hand",
                Fill = new SolidColorPaint(SKColors.SkyBlue)
            },
            new ColumnSeries<int>
            {
                Values = Line,
                Name = "Straight Line",
                Fill = new SolidColorPaint(SKColors.IndianRed)
            },
            new ColumnSeries<int>
            {
                Values = Rectangle,
                Name = "Rectangle",
                Fill = new SolidColorPaint(SKColors.MediumSeaGreen)
            },
            new ColumnSeries<int>
            {
                Values = Ellipse,
                Name = "Ellipse",
                Fill = new SolidColorPaint(SKColors.CornflowerBlue)
            },
            new ColumnSeries<int>
            {
                Values = Triangle,
                Name = "Triangle",
                Fill = new SolidColorPaint(SKColors.MediumPurple)
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = Labels,
                Name = "Data Snapshot",
                TextSize = 12,
                NameTextSize = 14,
                NamePaint = new SolidColorPaint(SKColors.White),
                LabelsPaint = new SolidColorPaint(SKColors.LightGray)
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "Count",
                TextSize = 12,
                NameTextSize = 14,
                NamePaint = new SolidColorPaint(SKColors.White),
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
    /// Adds a new snapshot of canvas data and updates the bar chart.
    /// Automatically keeps only the latest 10 snapshots.
    /// </summary>
    /// <param name="data">Canvas shape count data.</param>
    /// <param name="label">Label for the snapshot (e.g., T1, T2, T3).</param>
    public void AddSnapshot(CanvasData data, string label)
    {
        Labels.Add(label);
        FreeHand.Add(data.FreeHand);
        Line.Add(data.StraightLine);
        Rectangle.Add(data.Rectangle);
        Ellipse.Add(data.Ellipse);
        Triangle.Add(data.Triangle);

        // Restrict to last 5 snapshots for clarity
        if (Labels.Count > 5)
        {
            Labels.RemoveAt(0);
            FreeHand.RemoveAt(0);
            Line.RemoveAt(0);
            Rectangle.RemoveAt(0);
            Ellipse.RemoveAt(0);
            Triangle.RemoveAt(0);
        }
    }
}
