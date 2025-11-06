using CanvasDataModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using ViewModel;
using Drawing = System.Drawing; // Alias to avoid conflict with System.Windows.Shapes
using static ViewModel.CanvasViewModel;

namespace CentralGui;

public partial class MainWindow : Window
{
    private readonly CanvasViewModel _vm = new();
    private UIElement? _currentPreviewElement = null;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        
        //_vm.RequestRedraw += (s, e) => RedrawCanvas();
        //Point? lastPoint = null;
        DrawArea.MouseLeftButtonDown += (s, e) =>
        {
            // --- ADD THIS ---
            // Safety check: remove any lingering preview
            if (_currentPreviewElement != null)
            {
                DrawArea.Children.Remove(_currentPreviewElement);
                _currentPreviewElement = null;
            }
            // --- END ADD ---
            Point pos = e.GetPosition(DrawArea);
            _vm.StartTracking(new Drawing.Point((int)pos.X, (int)pos.Y)); // Use Drawing.Point
            DrawArea.CaptureMouse();
        };

        DrawArea.MouseMove += (s, e) =>
        {
            if (_vm._isTracking) // Use _isTracking
            {
                Point pos = e.GetPosition(DrawArea);

                bool isInside = pos.X >= 0 && pos.X <= DrawArea.ActualWidth &&
                                pos.Y >= 0 && pos.Y <= DrawArea.ActualHeight;

                if (isInside)
                {
                    _vm.TrackPoint(new Drawing.Point((int)pos.X, (int)pos.Y)); // Use Drawing.Point

                    // --- ENTIRELY NEW LOGIC ---

                    // A. Remove the PREVIOUS preview element
                    if (_currentPreviewElement != null)
                    {
                        DrawArea.Children.Remove(_currentPreviewElement);
                    }

                    // B. Get the NEW preview shape data
                    IShape previewData = _vm.CurrentPreviewShape;
                    if (previewData != null)
                    {
                        // C. Render the new preview and STORE a reference to it
                        _currentPreviewElement = ShapeRenderer.Render(DrawArea, previewData);
                    }
                    else
                    {
                        _currentPreviewElement = null;
                    }
                    // --- END NEW LOGIC ---
                }
            }
        };

        DrawArea.MouseLeftButtonUp += (s, e) =>
        {
            if (_currentPreviewElement != null)
            {
                DrawArea.Children.Remove(_currentPreviewElement);
                _currentPreviewElement = null;
            }

            _vm.StopTracking(); 

            DrawArea.ReleaseMouseCapture();

            if (_vm._shapes.Count > 0)
            {
                IShape newShape = _vm._shapes.Last();
                ShapeRenderer.Render(DrawArea, newShape);
            }
        };

        this.KeyDown += (s, e) =>
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                _vm.Undo();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                _vm.Redo();
            }
            else if (e.Key == Key.T) 
            {
                _vm.AddTestShape();
            }
            ShapeRenderer.RenderAll(DrawArea, _vm._shapes);
        };
        UpdateToolButtons();
        UpdateCurrentColorUI();
    }

    private void RedrawCanvas()
    {
        ShapeRenderer.RenderAll(DrawArea, _vm._shapes);
    }
    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button colorButton && colorButton.Background is SolidColorBrush brush)
        {
            System.Windows.Media.Color wpfColor = brush.Color;

            Drawing.Color modelColor = Drawing.Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

            _vm.CurrentColor = modelColor;

            UpdateCurrentColorUI();
        }
    }

    private void UpdateCurrentColorUI()
    {
        Drawing.Color modelColor = _vm.CurrentColor;
        var wpfColor = System.Windows.Media.Color.FromArgb(modelColor.A, modelColor.R, modelColor.G, modelColor.B);
        CurrentColorBorder.Background = new SolidColorBrush(wpfColor);
    }
    private void BtnFreehand_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentMode = DrawingMode.FreeHand;
        UpdateToolButtons();
    }

    private void BtnLine_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentMode = DrawingMode.StraightLine;
        UpdateToolButtons();
    }

    private void BtnRectangle_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentMode = DrawingMode.Rectangle;
        UpdateToolButtons();
    }

    private void BtnTriangle_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentMode = DrawingMode.TriangleShape;
        UpdateToolButtons();
    }

    private void BtnEllipse_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentMode = DrawingMode.EllipseShape;
        UpdateToolButtons();
    }

    private void BtnUndo_Click(object sender, RoutedEventArgs e)
    {
        _vm.Undo();
        ShapeRenderer.RenderAll(DrawArea, _vm._shapes);
    }

    private void BtnRedo_Click(object sender, RoutedEventArgs e)
    {
        _vm.Redo();
        ShapeRenderer.RenderAll(DrawArea, _vm._shapes);
    }
    private void UpdateToolButtons()
    {
        // Reset all
        BtnFreehand.ClearValue(Button.BackgroundProperty);
        BtnFreehand.ClearValue(Button.BorderBrushProperty);
        BtnLine.ClearValue(Button.BackgroundProperty);
        BtnLine.ClearValue(Button.BorderBrushProperty);
        BtnRectangle.ClearValue(Button.BackgroundProperty);
        BtnRectangle.ClearValue(Button.BorderBrushProperty);
        BtnEllipse.ClearValue(Button.BackgroundProperty);
        BtnEllipse.ClearValue(Button.BorderBrushProperty);
        BtnTriangle.ClearValue(Button.BackgroundProperty);
        BtnTriangle.ClearValue(Button.BorderBrushProperty);
        BtnUndo.ClearValue(Button.BackgroundProperty);
        BtnUndo.ClearValue(Button.BorderBrushProperty);
        BtnRedo.ClearValue(Button.BackgroundProperty);
        BtnRedo.ClearValue(Button.BorderBrushProperty);

        Brush? selectedBrush = null;
        Brush? selectedBorder = null;
        try
        {
            selectedBrush = (Brush)FindResource("GlassyPressedBrush");
        }
        catch
        {
            selectedBrush = Brushes.LightSteelBlue;
        }

        selectedBorder = new SolidColorBrush(System.Windows.Media.Color.FromRgb(111, 166, 214)); // #6FA6D6

        if (_vm.CurrentMode == DrawingMode.FreeHand)
        {
            BtnFreehand.Background = selectedBrush;
            BtnFreehand.BorderBrush = selectedBorder;
        }
        else if (_vm.CurrentMode == DrawingMode.StraightLine)
        {
            BtnLine.Background = selectedBrush;
            BtnLine.BorderBrush = selectedBorder;
        }
        else if (_vm.CurrentMode == DrawingMode.Rectangle)
        {
            BtnRectangle.Background = selectedBrush;
            BtnRectangle.BorderBrush = selectedBorder;
        }
        else if (_vm.CurrentMode == DrawingMode.EllipseShape)
        {
            BtnEllipse.Background = selectedBrush;
            BtnEllipse.BorderBrush = selectedBorder;
        }
        else if (_vm.CurrentMode == DrawingMode.TriangleShape)
        {
            BtnTriangle.Background = selectedBrush;
            BtnTriangle.BorderBrush = selectedBorder;
        }

    }
}
