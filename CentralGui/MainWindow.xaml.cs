using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using CanvasDataModel;
using ViewModel;
using static ViewModel.CanvasViewModel;
using Drawing = System.Drawing; // Alias to avoid conflict with System.Windows.Shapes
using System.Linq; // --- ADDED ---
namespace CentralGui;

public partial class MainWindow : Window
{
    private readonly CanvasViewModel _vm = new();
    // --- FIELDS FOR PERFORMANCE OPTIMIZATION ---
    private UIElement? _currentPreviewElement = null;
    private Rectangle? _selectionBox = null;
    // --- END FIELDS ---
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        // --- ADDED ---
        // Listen for when the ViewModel changes the selected shape
        _vm.PropertyChanged += Vm_PropertyChanged;
        // --- END ADDED ---
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

            // --- MODIFIED ---
            // Only capture mouse if the VM started tracking (i.e., not in Select mode)
            if (_vm._isTracking)
            {
                DrawArea.CaptureMouse();
            }
            // --- END MODIFIED ---
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

            // --- MODIFIED ---
            // StopTracking creates the shape, adds it to the dictionary,
            // and saves it in the _vm.LastCreatedShape property.
            _vm.StopTracking();

            DrawArea.ReleaseMouseCapture();

            // Now we render *only* the new shape for performance.
            if (_vm.LastCreatedShape != null)
            {
                ShapeRenderer.Render(DrawArea, _vm.LastCreatedShape);
            }
            // --- END MODIFIED ---
        };

        this.KeyDown += (s, e) =>
        {
            // --- MODIFIED ---
            // We now render from the dictionary's values, filtering for visibility.
            IEnumerable<IShape> visibleShapes = _vm._shapes.Values
                                    .Where(item => item.IsVisible)
                                    .Select(item => item.Shape);
            // --- END MODIFIED ---
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                _vm.Undo();
                ShapeRenderer.RenderAll(DrawArea, visibleShapes); // RenderAll IS required here
                UpdateSelectionBox(); // --- ADDED ---

            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                _vm.Redo();
                ShapeRenderer.RenderAll(DrawArea, visibleShapes); // RenderAll IS required here
                UpdateSelectionBox(); // --- ADDED ---

            }
            // --- NEW ---
            else if (e.Key == Key.Delete && _vm.SelectedShape != null)
            {
                _vm.DeleteSelectedShape();
                // Get the *new* list of visible shapes (without the deleted one) and render all
                visibleShapes = _vm._shapes.Values.Where(item => item.IsVisible).Select(item => item.Shape);
                ShapeRenderer.RenderAll(DrawArea, visibleShapes);
                // Selection is cleared by the ViewModel, which triggers UpdateSelectionBox()
            }
            // --- END NEW ---
            else if (e.Key == Key.T)
            {
                _vm.AddTestShape();
                // --- MODIFIED ---
                IEnumerable<IShape> newVisibleShapes = _vm._shapes.Values
                                        .Where(item => item.IsVisible)
                                        .Select(item => item.Shape);
                ShapeRenderer.RenderAll(DrawArea, newVisibleShapes);
                // --- END MODIFIED ---
            }
        };
        UpdateToolButtons();
        UpdateCurrentColorUI();
    }
    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When the selected shape changes, update the selection box
        if (e.PropertyName == nameof(CanvasViewModel.SelectedShape))
        {
            UpdateSelectionBox();
        }
        // When the tool mode changes, update the buttons
        if (e.PropertyName == nameof(CanvasViewModel.CurrentMode))
        {
            UpdateToolButtons();
        }
    }

    /// <summary>
    /// Handles drawing the blue selection box
    /// </summary>
    private void UpdateSelectionBox()
    {
        // Remove old box
        if (_selectionBox != null)
        {
            DrawArea.Children.Remove(_selectionBox);
            _selectionBox = null;
        }

        // --- MODIFIED ---
        // Only draw a selection box if the shape is also visible
        if (_vm.SelectedShape != null && _vm._shapes.ContainsKey(_vm.SelectedShape.ShapeId) && _vm._shapes[_vm.SelectedShape.ShapeId].IsVisible)
        {
            _selectionBox = ShapeRenderer.CreateSelectionBox(_vm.SelectedShape.GetBoundingBox());
            DrawArea.Children.Add(_selectionBox);
        }
        // --- END MODIFIED ---
    }
    // --- END ADDED ---
    private void RedrawCanvas()
    {
        // --- MODIFIED ---
        // Get only the visible shapes from the dictionary to render.
        IEnumerable<IShape> visibleShapes = _vm._shapes.Values
                                .Where(item => item.IsVisible)
                                .Select(item => item.Shape);
        ShapeRenderer.RenderAll(DrawArea, visibleShapes);
        // --- END MODIFIED ---        
        // --- ADDED ---
        // Re-draw the selection box on top after a full redraw
        UpdateSelectionBox();
        // --- END ADDED ---
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
    // --- ADDED ---
    private void BtnSelect_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentMode = DrawingMode.Select;
    }
    // --- END ADDED ---
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
        RedrawCanvas(); // Use RedrawCanvas to correctly handle selection
    }

    private void BtnRedo_Click(object sender, RoutedEventArgs e)
    {
        _vm.Redo();
        RedrawCanvas(); // Use RedrawCanvas to correctly handle selection
    }
    private void UpdateToolButtons()
    {
        // Reset all
        BtnSelect.ClearValue(Button.BackgroundProperty); // ADDED
        BtnSelect.ClearValue(Button.BorderBrushProperty); // ADDED
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

        // --- MODIFIED ---
        switch (_vm.CurrentMode)
        {
            case DrawingMode.Select:
                BtnSelect.Background = selectedBrush;
                BtnSelect.BorderBrush = selectedBorder;
                break;
            case DrawingMode.FreeHand:
                BtnFreehand.Background = selectedBrush;
                BtnFreehand.BorderBrush = selectedBorder;
                break;
            case DrawingMode.StraightLine:
                BtnLine.Background = selectedBrush;
                BtnLine.BorderBrush = selectedBorder;
                break;
            case DrawingMode.Rectangle:
                BtnRectangle.Background = selectedBrush;
                BtnRectangle.BorderBrush = selectedBorder;
                break;
            case DrawingMode.EllipseShape:
                BtnEllipse.Background = selectedBrush;
                BtnEllipse.BorderBrush = selectedBorder;
                break;
            case DrawingMode.TriangleShape:
                BtnTriangle.Background = selectedBrush;
                BtnTriangle.BorderBrush = selectedBorder;
                break;
        }

    }
}
