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

using CanvasApp.DataModel;
using CanvasApp.Services;

using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace CanvasApp.ViewModel;

public class CanvasViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum DrawingMode { Select, FreeHand, StraightLine, Rectangle, EllipseShape, TriangleShape, Regularize }
    private DrawingMode _currentMode = DrawingMode.FreeHand;
    public DrawingMode CurrentMode
    {
        get => _currentMode;
        set {
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

    // --- NEW ---
    private bool _isMovingShape = false;
    /// <summary>
    /// Public getter to allow the View to check if a move is in progress.
    /// </summary>
    public bool IsMovingShape => _isMovingShape;
    /// <summary>
    /// Stores the mouse position where a shape move began.
    /// </summary>
    private Point _moveStartPoint;
    /// <summary>
    /// Stores the state of a shape *before* a move begins.
    /// This is used to create the 'PrevShape' for the undo action.
    /// </summary>
    private IShape? _originalShapeForMove;
    /// <summary>
    /// Stores the bounds of the canvas to constrain shape movement.
    /// </summary>
    public Rectangle CanvasBounds { get; set; }
    // --- END NEW ---

    // --- MAJOR CHANGE ---
    // Changed from ObservableCollection<IShape> to a Dictionary.
    // The dictionary stores the shape and its visibility, as you requested.
    public Dictionary<string, (IShape Shape, bool IsVisible)> _shapes = new();

    // --- MAJOR CHANGE ---
    // The StateManager now works with CanvasActions.
    private readonly StateManager _stateManager = new();
    // --- NEW ---
    /// <summary>
    /// Stores the state of a shape *before* a modification (like a color
    /// change or slider drag) begins. This is used to create the 'PrevShape'
    /// for the undo action.
    /// </summary>
    private IShape? _originalShapeForUndo = null;
    // --- END NEW ---
    // --- MODIFIED ---
    private Color _currentColor = Color.Red;
    public Color CurrentColor
    {
        get => _currentColor;
        set {
            if (_currentColor == value) { return; }
            _currentColor = value;
            OnPropertyChanged();

            // --- NEW MODIFY LOGIC FOR COLOR ---
            if (SelectedShape != null && SelectedShape.Color != value)
            {
                // Store the original shape if this is the first change in a sequence.
                _originalShapeForUndo ??= SelectedShape;

                // Create the new shape
                IShape newShape = SelectedShape.WithUpdates(value, null);

                // Update the dictionary
                _shapes[newShape.ShapeId] = (newShape, true);

                // --- FIX 1: THE FEEDBACK LOOP ---
                // Do not call the public 'SelectedShape' setter.
                // Update the *backing field* directly to prevent the feedback loop
                // and notify the UI that the selection *property* has changed
                // (even though it's technically the same object ID).
                _selectedShape = newShape;
                OnPropertyChanged(nameof(SelectedShape));
                // --- END FIX 1 ---
            }
            // --- END NEW ---
        }
    }
    public string CurrentUserId { get; set; } = "user_default"; // <-- ADD THIS (mock ID for now)
    // --- MODIFIED ---
    private double _currentThickness = 2.0;
    public double CurrentThickness
    {
        get => _currentThickness;
        set {
            if (_currentThickness == value) { return; }
            _currentThickness = value;
            OnPropertyChanged();

            // --- NEW MODIFY LOGIC FOR THICKNESS ---
            if (SelectedShape != null && SelectedShape.Thickness != value)
            {
                // Store the original shape if this is the first change in a sequence.
                _originalShapeForUndo ??= SelectedShape;

                // Create the new shape
                IShape newShape = SelectedShape.WithUpdates(null, value);

                // Update the dictionary
                _shapes[newShape.ShapeId] = (newShape, true);


                // --- FIX 1: THE FEEDBACK LOOP ---
                // Do not call the public 'SelectedShape' setter.
                // Update the *backing field* directly.
                _selectedShape = newShape;
                OnPropertyChanged(nameof(SelectedShape));
                // --- END FIX 1 ---
            }
            // --- END NEW ---
        }
    }
    // --- END MODIFIED ---
    private IShape? _selectedShape;
    public IShape? SelectedShape
    {
        get => _selectedShape;
        set {
            if (_selectedShape != value)
            {
                // --- NEW ---
                // If we are deselecting a shape, commit any pending modification.
                if (_selectedShape != null)
                {
                    CommitModification();
                }
                // --- END NEW ---
                _selectedShape = value;
                OnPropertyChanged(); // Notify the View to update the selection box
                                     // --- FIX 2: UPDATE UI CONTROLS ---
                                     // When a new shape is selected, update the UI controls (slider/color) to match it.
                if (_selectedShape != null)
                {
                    // Set backing fields *directly* to prevent feedback loop
                    if (_currentColor != _selectedShape.Color)
                    {
                        _currentColor = _selectedShape.Color;
                        OnPropertyChanged(nameof(CurrentColor));
                    }
                    if (_currentThickness != _selectedShape.Thickness)
                    {
                        _currentThickness = _selectedShape.Thickness;
                        OnPropertyChanged(nameof(CurrentThickness));
                    }
                }
                // --- END FIX 2 ---
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

    // --- NEW METHOD ---
    /// <summary>
    /// Commits any pending modification to the undo/redo stack.
    /// This is called when a modification gesture (like a color click
    /// or slider drag) is completed.
    /// </summary>
    public void CommitModification()
    {
        // If we have a stored "before" shape and a "current" shape,
        // and they are different, create a Modify action.
        if (_originalShapeForUndo != null && SelectedShape != null &&
            _originalShapeForUndo.ShapeId == SelectedShape.ShapeId)
        {
            // Check if anything actually changed
            if (_originalShapeForUndo.Color != SelectedShape.Color ||
                _originalShapeForUndo.Thickness != SelectedShape.Thickness)
            {
                var action = new CanvasAction(CanvasActionType.Modify, _originalShapeForUndo, SelectedShape);
                _stateManager.AddAction(action);
            }
        }

        // Always clear the "before" shape after committing.
        _originalShapeForUndo = null;
    }
    // --- END NEW ---

    public void SelectShapeAt(Point point)
    {
        // --- NEW ---
        // Commit any pending modification before selecting a new shape.
        CommitModification();
        // --- END NEW ---

        SelectedShape = null;

        // --- FIX 3: Corrected loop syntax ---
        // The dictionary's value is a tuple: (IShape Shape, bool IsVisible)
        foreach ((IShape Shape, bool IsVisible) item in _shapes.Values.Reverse())
        {
            if (item.IsVisible && item.Shape.IsHit(point))
            {
                SelectedShape = item.Shape;
                return;
            }
        }
        // --- END FIX 3 ---
    }
    public void StartTracking(Point point)
    {
        // --- NEW ---
        // Commit any pending modification before drawing.
        CommitModification();
        // --- END NEW ---

        LastCreatedShape = null;
        // --- MODIFIED ---
        if (CurrentMode == DrawingMode.Select)
        {
            // Handle selection and potential move
            _isTracking = false;
            SelectShapeAt(point); // This selects the shape

            // Check if the click was ON the newly selected shape
            if (SelectedShape != null && SelectedShape.IsHit(point))
            {
                // Start a move operation
                _isMovingShape = true;
                _moveStartPoint = point;
                _originalShapeForMove = SelectedShape; // Store pre-move state
            }
            else
            {
                _isMovingShape = false;
            }
            return;
        }

        // --- NEW: Regularize tool handling (create a constant shape at click)
        if (CurrentMode == DrawingMode.Regularize)
        {
            _isTracking = false;
            SelectedShape = null;

            // Create a fixed-size rectangle centered at the click point
            int width = 40;
            int height = 30;
            var topLeft = new Point(point.X - width / 2, point.Y - height / 2);
            var bottomRight = new Point(point.X + width / 2, point.Y + height / 2);
            var pts = new List<Point> { topLeft, bottomRight };

            var constantShape = new RectangleShape(pts, CurrentColor, CurrentThickness, CurrentUserId);

            // Add to dictionary and push a Create action
            _shapes.Add(constantShape.ShapeId, (constantShape, true));
            _stateManager.AddAction(new CanvasAction(CanvasActionType.Create, null, constantShape));

            SelectedShape = constantShape;
            LastCreatedShape = constantShape;

            return;
        }
        // --- END NEW ---

        // --- If not Select, proceed with drawing ---
        _isTracking = true;
        _isMovingShape = false; // --- ADDED ---
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
        // --- NEW: Handle shape moving ---
        if (_isMovingShape && SelectedShape != null && _originalShapeForMove != null)
        {
            // Calculate delta from the *start* of the move
            Point offset = new Point(point.X - _moveStartPoint.X, point.Y - _moveStartPoint.Y);

            // Get a new, moved shape from the *original* shape
            IShape movedShape = _originalShapeForMove.WithMove(offset, CanvasBounds);

            // Update the dictionary
            _shapes[movedShape.ShapeId] = (movedShape, true);

            // Update the backing field and notify
            _selectedShape = movedShape;
            OnPropertyChanged(nameof(SelectedShape));
        }
        // --- END NEW ---
        else if (_isTracking && _trackedPoints.Count > 0)
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
        // --- NEW: Handle end of shape move ---
        if (_isMovingShape)
        {
            _isMovingShape = false;

            // Check if a move actually happened
            if (_originalShapeForMove != null && SelectedShape != null &&
                _originalShapeForMove.ShapeId == SelectedShape.ShapeId &&
                !_originalShapeForMove.Points.SequenceEqual(SelectedShape.Points))
            {
                // Create the Modify action
                var action = new CanvasAction(CanvasActionType.Modify, _originalShapeForMove, SelectedShape);
                _stateManager.AddAction(action);
            }

            _originalShapeForMove = null;
            return; // --- ADDED: Stop processing
        }
        // --- END NEW ---

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
            if (_trackedPoints.Count < 2) // --- ADDED: Prevent single-point "shapes"
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
        get {
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
        // --- NEW ---
        // Commit any pending modification before drawing.
        CommitModification();
        // --- END NEW ---
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
        // --- NEW ---
        // Commit any pending modification before drawing.
        CommitModification();
        // --- END NEW ---
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
            // --- NEW ---
            // --- FIX 4: YOUR SAFETY CHECK ---
            else if (undoneAction.ActionType == CanvasActionType.Modify && undoneAction.PrevShape != null && undoneAction.NewShape != null)
            {
                string shapeId = undoneAction.PrevShape.ShapeId;
                if (_shapes.ContainsKey(shapeId))
                {
                    // This is your requested safety check:
                    // Only undo if the *current* shape on canvas is the one from the action's "NewShape".
                    if (_shapes[shapeId].Shape == undoneAction.NewShape)
                    {
                        _shapes[shapeId] = (undoneAction.PrevShape, true);
                        SelectedShape = undoneAction.PrevShape;
                    }
                    else
                    {
                        // The check failed. This means the shape changed *again* after this action.
                        // We can't safely undo. For now, we do nothing.
                        // We must also move the state manager back to where it was,
                        // otherwise we have a "desync"
                        _stateManager.Redo(); // Undo the undo
                    }
                }
            }
            // --- END FIX 4 ---
            // --- END NEW ---
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
            // --- FIX 4 (Redo Safety Check) ---
            else if (redoneAction.ActionType == CanvasActionType.Modify && redoneAction.NewShape != null && redoneAction.PrevShape != null)
            {
                string shapeId = redoneAction.NewShape.ShapeId;
                if (_shapes.ContainsKey(shapeId))
                {
                    // Safety check for Redo:
                    // Only redo if the *current* shape is the one from the action's "PrevShape".
                    if (_shapes[shapeId].Shape == redoneAction.PrevShape)
                    {
                        _shapes[shapeId] = (redoneAction.NewShape, true);
                        SelectedShape = redoneAction.NewShape;
                    }
                    else
                    {
                        // The check failed.
                        _stateManager.Undo(); // Undo the redo
                    }
                }
            }
            // --- END FIX 4 ---

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

    // --- NEW METHOD ---
    /// <summary>
    /// Regularizes the selected shape by replacing it with a small rectangle.
    /// This is typically used to simplify a shape to a basic form.
    /// </summary>
    /// <param name="width">The width of the regularized shape.</param>
    /// <param name="height">The height of the regularized shape.</param>
    public void RegularizeSelectedShape(int width = 1, int height = 1)
    {
        if (SelectedShape == null)
        {
            return;
        }

        // Store previous shape
        IShape prev = SelectedShape;

        // Create a very small replacement centered on the previous shape's bounding box
        IShape replacement = AiService.SmallReplacement(prev, CurrentUserId, width, height);

        // Update dictionary (replace the shape)
        if (_shapes.ContainsKey(prev.ShapeId))
        {
            _shapes[prev.ShapeId] = (replacement, true);

            // Push Modify action to state manager
            _stateManager.AddAction(new CanvasAction(CanvasActionType.Modify, prev, replacement));

            // Select the replacement so UI updates
            SelectedShape = replacement;
        }
    }
    // --- END NEW METHOD ---
}
