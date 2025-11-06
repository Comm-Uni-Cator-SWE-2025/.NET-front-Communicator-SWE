using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;
using CanvasDataModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace ViewModel;

public class CanvasViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum DrawingMode { Select, FreeHand, StraightLine, Rectangle, EllipseShape, TriangleShape }
    private DrawingMode _currentMode = DrawingMode.FreeHand;
    public DrawingMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode != value)
            {
                _currentMode = value;
                OnPropertyChanged();

                // When switching away from Select, deselect any shape
                if (_currentMode != DrawingMode.Select)
                {
                    SelectedShape = null;
                }
            }
        }
    }
    private List<Point> _trackedPoints = new(); // this is for tracking mouse movements
    public bool _isTracking = false;

    // --- MAJOR CHANGE ---
    // Changed from ObservableCollection<IShape> to a Dictionary.
    // The dictionary stores the shape and its visibility, as you requested.
    public Dictionary<string, (IShape Shape, bool IsVisible)> _shapes = new();

    // --- MAJOR CHANGE ---
    // The StateManager now works with CanvasActions.
    private readonly StateManager _stateManager = new();
    public Color CurrentColor { get; set; } = Color.Red;
    public string CurrentUserId { get; set; } = "user_default"; // <-- ADD THIS (mock ID for now)
    private double _currentThickness = 2.0; // Default value
    public double CurrentThickness
    {
        get => _currentThickness;
        set
        {
            if (_currentThickness != value)
            {
                _currentThickness = value;
                OnPropertyChanged(); // <-- This notifies the UI (the slider)
            }
        }
    }
    // --- ADDED ---
    private IShape? _selectedShape;
    public IShape? SelectedShape
    {
        get => _selectedShape;
        set
        {
            if (_selectedShape != value)
            {
                _selectedShape = value;
                OnPropertyChanged(); // Notify the View to update the selection box
            }
        }
    }

    /// <summary>
    /// Finds and selects the top-most shape at a given point.
    /// </summary>
    /// 
    /// /// <summary>
    /// Holds a reference to the very last shape created, so the
    /// View can render it once without a full RenderAll().
    /// </summary>
    public IShape? LastCreatedShape { get; private set; }
    public void SelectShapeAt(Point point)
    {
        SelectedShape = null;

        // --- MODIFIED ---
        // Iterate over the dictionary values in reverse (top-most shapes drawn last)
        // Only check shapes that are currently visible.
        foreach ((IShape shape, bool isVisible) item in _shapes.Values.Reverse())
        {
            if (item.isVisible && item.shape.IsHit(point))
            {
                SelectedShape = item.shape;
                return;
            }
        }
        // --- END MODIFIED ---
    }
    public void StartTracking(Point point)
    {
        LastCreatedShape = null;
        // --- MODIFIED ---
        if (CurrentMode == DrawingMode.Select)
        {
            // Handle selection, don't start tracking
            _isTracking = false;
            SelectShapeAt(point);
            return;
        }

        // --- If not Select, proceed with drawing ---
        _isTracking = true;
        SelectedShape = null; // Deselect any shape when drawing
        // --- END MODIFIED ---
        if (CurrentMode == DrawingMode.FreeHand)
        {
            _trackedPoints.Clear();
            _trackedPoints.Add(point);
        }
        else if (CurrentMode == DrawingMode.StraightLine || CurrentMode == DrawingMode.Rectangle || CurrentMode == DrawingMode.EllipseShape || CurrentMode == DrawingMode.TriangleShape)
        {
            _trackedPoints.Clear();
            _trackedPoints.Add(point); // start point
            _trackedPoints.Add(point); // end point (will be updated on mouse move)
        }
    }
    public void TrackPoint(Point point)
    {
        if (_isTracking && _trackedPoints.Count > 0)
        {
            if (CurrentMode == DrawingMode.FreeHand)
            {
                _trackedPoints.Add(point);
            }
            else if (CurrentMode == DrawingMode.StraightLine || CurrentMode == DrawingMode.Rectangle || CurrentMode == DrawingMode.EllipseShape || CurrentMode == DrawingMode.TriangleShape)
            {
                _trackedPoints[1] = point; // update end point
            }
        }
    }

    public void StopTracking()
    {
        // --- MODIFIED ---
        if (CurrentMode == DrawingMode.Select || !_isTracking)
        {
            _isTracking = false;
            return; // Don't create a shape if we weren't tracking
        }
        // --- END MODIFIED ---
        _isTracking = false;
        if (_trackedPoints.Count == 0)
        {
            return;
        }
        IShape? newShape = null; // --- ADDED ---

        if (CurrentMode == DrawingMode.FreeHand)
        {
            if (_trackedPoints.Count == 0)
            {
                return;
            }
            var freehand = new FreeHand(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            newShape = freehand; // --- ADDED ---

        }
        else if (CurrentMode == DrawingMode.StraightLine)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var line = new StraightLine(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            newShape = line; // --- ADDED ---

        }
        else if (CurrentMode == DrawingMode.Rectangle)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var rectangle = new RectangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            newShape = rectangle; // --- ADDED ---
        }
        else if (CurrentMode == DrawingMode.EllipseShape)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var ellipse = new EllipseShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            newShape = ellipse; // --- ADDED ---
        }
        else if (CurrentMode == DrawingMode.TriangleShape)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var triangle = new TriangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            newShape = triangle; // --- ADDED ---
        }
        if (newShape != null)
        {
            // Add to the dictionary with visibility set to true
            _shapes.Add(newShape.ShapeId, (newShape, true));

            // Add the 'Create' action to the state manager, with prevShape as null
            _stateManager.AddAction(new CanvasAction(CanvasActionType.Create, null, newShape));

            // Set properties for the View to consume
            SelectedShape = newShape;
            LastCreatedShape = newShape;
        }
        // --- END ADDED ---
    }
    public IShape? CurrentPreviewShape
    {
        get
        {
            // --- MODIFIED ---
            if (!_isTracking || _trackedPoints.Count < 2 || CurrentMode == DrawingMode.Select)
            {
                return null;
            }
            // --- END MODIFIED ---
            switch (CurrentMode)
            {
                case DrawingMode.FreeHand:
                    return new FreeHand(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.StraightLine:
                    return new StraightLine(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.Rectangle:
                    return new RectangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.EllipseShape:
                    return new EllipseShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.TriangleShape:
                    return new TriangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                default:
                    return null;
            }
        }
    }

    // --- NEW METHOD ---
    /// <summary>
    /// Deletes the currently selected shape.
    /// </summary>
    public void DeleteSelectedShape()
    {
        if (SelectedShape == null) { return; }

        string shapeId = SelectedShape.ShapeId;
        if (_shapes.ContainsKey(shapeId))
        {
            // 1. Set visibility to false
            _shapes[shapeId] = (_shapes[shapeId].Shape, false);

            // 2. Create the Delete action. PrevShape is the shape we just "deleted". NewShape is null.
            var deleteAction = new CanvasAction(CanvasActionType.Delete, SelectedShape, null);

            // 3. Add to the state manager
            _stateManager.AddAction(deleteAction);

            // 4. Deselect the shape
            SelectedShape = null;
        }
    }
    // --- END NEW METHOD ---
    public void Undo()
    {
        SelectedShape = null;
        // --- MAJOR CHANGE ---
        // Get the action that was just undone
        CanvasAction? undoneAction = _stateManager.Undo();

        if (undoneAction != null)
        {
            // If we are undoing a 'Create', hide the shape (using NewShape)
            if (undoneAction.ActionType == CanvasActionType.Create && undoneAction.NewShape != null)
            {
                if (_shapes.ContainsKey(undoneAction.NewShape.ShapeId))
                {
                    // Set visibility flag to false
                    _shapes[undoneAction.NewShape.ShapeId] = (_shapes[undoneAction.NewShape.ShapeId].Shape, false);
                }
            }
            // --- MODIFIED ---
            // If we are undoing a 'Delete', "resurrect" the shape (set visibility to true)
            else if (undoneAction.ActionType == CanvasActionType.Delete && undoneAction.PrevShape != null)
            {
                if (_shapes.ContainsKey(undoneAction.PrevShape.ShapeId))
                {
                    _shapes[undoneAction.PrevShape.ShapeId] = (_shapes[undoneAction.PrevShape.ShapeId].Shape, true);
                }
            }
        }
        // --- END MAJOR CHANGE ---
    }

    public void Redo()
    {
        SelectedShape = null;

        // --- MAJOR CHANGE ---
        // Get the action that was just redone
        CanvasAction? redoneAction = _stateManager.Redo();

        if (redoneAction != null)
        {
            // If we are redoing a 'Create', show the shape (using NewShape)
            if (redoneAction.ActionType == CanvasActionType.Create && redoneAction.NewShape != null)
            {
                if (_shapes.ContainsKey(redoneAction.NewShape.ShapeId))
                {
                    // Set visibility flag to true
                    _shapes[redoneAction.NewShape.ShapeId] = (_shapes[redoneAction.NewShape.ShapeId].Shape, true);
                }
            }
            // --- MODIFIED ---
            // If we are redoing a 'Delete', hide the shape again
            else if (redoneAction.ActionType == CanvasActionType.Delete && redoneAction.PrevShape != null)
            {
                if (_shapes.ContainsKey(redoneAction.PrevShape.ShapeId))
                {
                    _shapes[redoneAction.PrevShape.ShapeId] = (_shapes[redoneAction.PrevShape.ShapeId].Shape, false);
                }
            }
            // --- END MODIFIED ---
        }
        // --- END MAJOR CHANGE ---
    }

    public void AddTestShape()
    {
        List<Point> testPoints = new List<Point>
        {
            new Point( 258,  217),
            new Point( 403, 354 )
        };

        var testShape = new EllipseShape(testPoints, Color.Purple, CurrentThickness, CurrentUserId);

        // --- MODIFIED ---
        _shapes.Add(testShape.ShapeId, (testShape, true));
        _stateManager.AddAction(new CanvasAction(CanvasActionType.Create, null, testShape));
        // --- END MODIFIED ---

    }

}
