using System.Collections.Generic;
using System.ComponentModel;
using System.Linq; // --- ADDED ---
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

        // --- NEW: Give the ViewModel the canvas bounds ---
        _vm.CanvasBounds = new Drawing.Rectangle(0, 0, (int)DrawArea.Width, (int)DrawArea.Height);
        // --- END NEW ---

        // --- ADDED ---
        // Listen for when the ViewModel changes the selected shape
        _vm.PropertyChanged += Vm_PropertyChanged;
        // --- END ADDED ---

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
            // Capture mouse if we are drawing OR moving a shape
            if (_vm._isTracking || _vm.IsMovingShape)
            {
                DrawArea.CaptureMouse();
            }
            // --- END MODIFIED ---
        };

        DrawArea.MouseMove += (s, e) =>
        {
            Point pos = e.GetPosition(DrawArea);
            bool isInside = pos.X >= 0 && pos.X <= DrawArea.ActualWidth &&
                            pos.Y >= 0 && pos.Y <= DrawArea.ActualHeight;

            if (!isInside) { return; }// Don't track outside the canvas

            // --- ENTIRELY NEW LOGIC ---
            if (_vm._isTracking) // Handle drawing preview
            {
                _vm.TrackPoint(new Drawing.Point((int)pos.X, (int)pos.Y));

                // A. Remove the PREVIOUS preview element
                if (_currentPreviewElement != null)
                {
                    DrawArea.Children.Remove(_currentPreviewElement);
                }

                // B. Get the NEW preview shape data
                IShape? previewData = _vm.CurrentPreviewShape;
                if (previewData != null)
                {
                    // C. Render the new preview and STORE a reference to it
                    _currentPreviewElement = ShapeRenderer.Render(DrawArea, previewData);
                }
                else
                {
                    _currentPreviewElement = null;
                }
            }
            else if (_vm.IsMovingShape) // Handle shape move
            {
                _vm.TrackPoint(new Drawing.Point((int)pos.X, (int)pos.Y));

                // A move requires a full redraw of the canvas
                SyncCanvasState();
            }
            // --- END NEW LOGIC ---
        };

        DrawArea.MouseLeftButtonUp += (s, e) =>
        {
            if (_currentPreviewElement != null)
            {
                DrawArea.Children.Remove(_currentPreviewElement);
                _currentPreviewElement = null;
            }

            // --- MODIFIED ---
            // Check state *before* stopping
            bool wasMoving = _vm.IsMovingShape;
            bool wasTracking = _vm._isTracking;

            _vm.StopTracking();
            DrawArea.ReleaseMouseCapture();

            if (wasMoving)
            {
                // Final redraw after move is committed to state
                SyncCanvasState();
            }
            else if (wasTracking && _vm.LastCreatedShape != null)
            {
                // Render *only* the new shape for performance.
                ShapeRenderer.Render(DrawArea, _vm.LastCreatedShape);
            }
            // --- END MODIFIED ---
        };

        this.KeyDown += (s, e) =>
        {
            // --- FIX IS HERE ---
            // DO NOT get visibleShapes here. Get it *after* the action.

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                _vm.Undo();
                // Get the *new* state AFTER undoing
                SyncCanvasState(); // Use full sync
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                _vm.Redo();
                // Get the *new* state AFTER redoing
                SyncCanvasState(); // Use full sync
            }
            else if ((e.Key == Key.Delete || e.Key == Key.D) && _vm.SelectedShape != null) // --- ADDED BACKSPACE ---
            {
                _vm.DeleteSelectedShape();
                // Get the *new* state AFTER deleting
                SyncCanvasState(); // Use full sync
            }
            // --- END FIX ---
            else if (e.Key == Key.T)
            {
                _vm.AddTestShape();
                // --- MODIFIED ---
                SyncCanvasState(); // Use full sync
                // --- END MODIFIED ---
            }
        };
        UpdateToolButtons();
        UpdateCurrentColorUI();
        // --- NEW: EVENT HANDLERS FOR MODIFICATION ---
        // We assume your slider in XAML has the name x:Name="ThicknessSlider"
        // This handler provides the live-preview redraw while dragging
        ThicknessSlider.ValueChanged += (s, e) =>
        {
            // --- MODIFIED ---
            // Only update if we are not currently dragging the slider
            if (ThicknessSlider.IsMouseCaptureWithin)
            {
                if (_vm.SelectedShape != null)
                {
                    SyncCanvasState();
                }
            }
            // --- END MODIFIED ---
        };

        // This handler commits the change to the undo stack
        // when the user releases the mouse from the slider.
        ThicknessSlider.PreviewMouseLeftButtonUp += (s, e) =>
        {
            _vm.CommitModification();
        };
        // --- END NEW ---

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
        // --- MODIFIED ---
        // When the VM's property changes (e.g., from Undo/Redo),
        // we must update the slider to match.
        if (e.PropertyName == nameof(CanvasViewModel.CurrentThickness))
        {
            // This syncs the slider if Undo/Redo changes the thickness
            ThicknessSlider.Value = _vm.CurrentThickness;
        }
        if (e.PropertyName == nameof(CanvasViewModel.CurrentColor))
        {
            // This syncs the color box if Undo/Redo changes the color
            UpdateCurrentColorUI();
        }
        // --- END MODIFIED ---
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
    private void SyncCanvasState()
    {
        IEnumerable<IShape> visibleShapes = _vm._shapes.Values
                                .Where(item => item.IsVisible)
                                .Select(item => item.Shape);
        ShapeRenderer.RenderAll(DrawArea, visibleShapes);
        // Re-draw the selection box on top after a full redraw
        UpdateSelectionBox();
    }
    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button colorButton && colorButton.Background is SolidColorBrush brush)
        {
            System.Windows.Media.Color wpfColor = brush.Color;
            Drawing.Color modelColor = Drawing.Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

            // Set the color in the VM, which triggers the modification logic
            _vm.CurrentColor = modelColor;

            // Commit the modification to the undo stack immediately
            _vm.CommitModification();

            // Redraw if a shape was modified
            if (_vm.SelectedShape != null)
            {
                SyncCanvasState(); // Redraw the whole canvas
            }

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
        SyncCanvasState(); // Use RedrawCanvas to correctly handle selection
    }

    private void BtnRedo_Click(object sender, RoutedEventArgs e)
    {
        _vm.Redo();
        SyncCanvasState(); // Use RedrawCanvas to correctly handle selection
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
