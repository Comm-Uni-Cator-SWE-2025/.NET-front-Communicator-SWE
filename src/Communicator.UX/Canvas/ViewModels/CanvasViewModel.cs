// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * -----------------------------------------------------------------------------
 *  File: CanvasViewModel.cs
 *  Owner: Pranitha Muluguru
 *  Roll Number : 112201004
 *  Module : Canvas
 *
 * -----------------------------------------------------------------------------
 */
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Communicator.Canvas;
using Communicator.Controller.Serialization;
using Communicator.Core.RPC;
using Communicator.Core.UX.Services;
using Microsoft.Win32;

namespace Communicator.UX.Canvas.ViewModels;

/// <summary>
/// The core logic for the Canvas application. 
/// Implements the **MVVM (Model-View-ViewModel) Pattern**.
/// Manages the state of shapes, handles user input logic, and coordinates with the StateManager.
/// Also acts as the **Observer** subject via INotifyPropertyChanged.
/// </summary>
public class CanvasViewModel : INotifyPropertyChanged
{
    protected readonly IRPC? Rpc;
    protected readonly IRpcEventService? RpcEventService;

    public CanvasViewModel(IRPC rpc, IRpcEventService rpcEventService)
    {
        Rpc = rpc;
        RpcEventService = rpcEventService;

        if (RpcEventService != null)
        {
            RpcEventService.CanvasUpdateReceived += OnCanvasUpdateReceived;
        }
    }

    private void OnCanvasUpdateReceived(object? sender, RpcDataEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[CanvasViewModel] OnCanvasUpdateReceived called");
        System.Diagnostics.Debug.WriteLine(DataSerializer.Deserialize<string>(e.Data.ToArray()));
        ReceiveData(e.Data.ToArray());
    }

    public virtual void ReceiveData(byte[] data)
    {
        // Base implementation does nothing or can be overridden
    }

    // --- Properties ---

    /// <summary>
    /// Event triggered when a property changes, notifying the UI to update.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Event triggered when the visual canvas needs to be repainted.
    /// </summary>
    public event Action? RequestRedraw;

    // --- Enumerations ---

    /// <summary>
    /// Enumeration for the current interaction tool selected by the user.
    /// </summary>
    public enum DrawingMode
    {
        Select,
        FreeHand,
        StraightLine,
        Rectangle,
        EllipseShape,
        TriangleShape
    }

    // --- Private Fields ---

    /// <summary>
    /// Backing field for the current drawing mode.
    /// </summary>
    private DrawingMode _currentMode = DrawingMode.FreeHand;

    /// <summary>
    /// Stores the list of points tracked during a mouse drag operation.
    /// </summary>
    private List<Point> _trackedPoints = new();

    /// <summary>
    /// Indicates if the user is currently dragging/moving an existing shape.
    /// </summary>
    private bool _isMovingShape = false;

    /// <summary>
    /// The coordinate where a move operation began.
    /// </summary>
    private Point _moveStartPoint;

    /// <summary>
    /// A snapshot of the shape before it began moving, used for delta calculations.
    /// </summary>
    private IShape? _originalShapeForMove;

    /// <summary>
    /// Backing field for the currently selected color.
    /// </summary>
    private Color _currentColor = Color.Black;

    /// <summary>
    /// Backing field for the currently selected stroke thickness.
    /// </summary>
    private double _currentThickness = 2.0;

    /// <summary>
    /// Backing field for the selected shape.
    /// </summary>
    protected IShape? _selectedShape;

    /// <summary>
    /// Backing field for the analysis result text.
    /// </summary>
    private string _analysisResult = "Ready to analyze...";

    /// <summary>
    /// Backing field to toggle the visibility of the analysis side panel.
    /// </summary>
    private bool _isAnalysisVisible = false;

    // --- Public Properties & Fields ---

    /// <summary>
    /// Indicates if the mouse is currently being tracked (button held down).
    /// </summary>
    public bool _isTracking = false;

    /// <summary>
    /// Public accessor to check if a shape is being moved.
    /// </summary>
    public bool IsMovingShape => _isMovingShape;

    /// <summary>
    /// Represents the physical boundaries of the canvas area.
    /// </summary>
    public Rectangle CanvasBounds { get; set; }

    /// <summary>
    /// The main dictionary storing all shapes on the canvas, keyed by Shape ID.
    /// </summary>
    public Dictionary<string, IShape> _shapes = new();

    /// <summary>
    /// Manages the Undo/Redo stacks.
    /// </summary>
    protected readonly StateManager StateManager = new();

    /// <summary>
    /// Used to store the state of a shape before modification for Undo purposes.
    /// </summary>
    public IShape? _originalShapeForUndo = null;

    /// <summary>
    /// The ID of the current user (e.g., "Client_1234" or "Host_Admin").
    /// </summary>
    public string CurrentUserId { get; set; } = "user_default";

    /// <summary>
    /// Collection of shapes meant to be displayed transiently (e.g., remote user drawing).
    /// </summary>
    public List<IShape> GhostShapes { get; } = new();

    /// <summary>
    /// Indicates if this instance is the Host (Server) or Client.
    /// </summary>
    public virtual bool IsHost => false;

    /// <summary>
    /// Gets or sets the current drawing tool.
    /// </summary>
    public DrawingMode CurrentMode
    {
        get => _currentMode;
        set {
            if (_currentMode != value)
            {
                _currentMode = value;
                OnPropertyChanged();
                // Deselect any shape when switching drawing tools to avoid accidental edits
                if (_currentMode != DrawingMode.Select)
                {
                    SelectedShape = null;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the color used for new shapes or the selected shape.
    /// </summary>
    public Color CurrentColor
    {
        get => _currentColor;
        set {
            if (_currentColor == value)
            {
                return;
            }

            _currentColor = value;
            OnPropertyChanged();

            if (SelectedShape != null && SelectedShape.Color != value)
            {
                _originalShapeForUndo ??= SelectedShape;
                IShape newShape = SelectedShape.WithUpdates(value, null, CurrentUserId);
                _selectedShape = newShape;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }
    }

    /// <summary>
    /// Gets or sets the line thickness for new shapes or the selected shape.
    /// </summary>
    public double CurrentThickness
    {
        get => _currentThickness;
        set {
            if (_currentThickness == value)
            {
                return;
            }

            _currentThickness = value;
            OnPropertyChanged();

            if (SelectedShape != null && SelectedShape.Thickness != value)
            {
                _originalShapeForUndo ??= SelectedShape;
                IShape newShape = SelectedShape.WithUpdates(null, value, CurrentUserId);
                _selectedShape = newShape;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }
    }

    /// <summary>
    /// The currently selected shape for editing.
    /// </summary>
    public IShape? SelectedShape
    {
        get => _selectedShape;
        set {
            if (_selectedShape != value)
            {
                // If we were editing a previous shape, ensure changes are committed
                if (_selectedShape != null)
                {
                    CommitModification();
                }
                _selectedShape = value;
                OnPropertyChanged();

                // Sync UI controls to the newly selected shape's properties
                if (_selectedShape != null)
                {
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
            }
        }
    }

    /// <summary>
    /// The most recently created shape, exposed for reference.
    /// </summary>
    public IShape? LastCreatedShape { get; private set; }

    /// <summary>
    /// Textual result from the AI analysis service.
    /// </summary>
    public string AnalysisResult
    {
        get => _analysisResult;
        set {
            _analysisResult = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Controls the visibility of the AI analysis panel in the UI.
    /// </summary>
    public bool IsAnalysisVisible
    {
        get => _isAnalysisVisible;
        set {
            _isAnalysisVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Generates a temporary shape for previewing while drawing.
    /// </summary>
    public IShape? CurrentPreviewShape
    {
        get {
            if (!_isTracking || _trackedPoints.Count < 2 || CurrentMode == DrawingMode.Select)
            {
                return null;
            }

            ShapeType? type = CurrentMode switch {
                DrawingMode.FreeHand => ShapeType.FREEHAND,
                DrawingMode.StraightLine => ShapeType.LINE,
                DrawingMode.Rectangle => ShapeType.RECTANGLE,
                DrawingMode.EllipseShape => ShapeType.ELLIPSE,
                DrawingMode.TriangleShape => ShapeType.TRIANGLE,
                _ => null
            };

            if (type.HasValue)
            {
                // Factory Pattern
                return ShapeFactory.CreateShape(type.Value, _trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            }
            return null;
        }
    }

    // --- Methods ---

    /// <summary>
    /// Helper method to invoke the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Helper method to invoke the RequestRedraw event.
    /// </summary>
    protected void RaiseRequestRedraw()
    {
        RequestRedraw?.Invoke();
    }

    /// <summary>
    /// Updates the local dictionary with a new/modified shape.
    /// Also handles synchronization if the updated shape was currently selected.
    /// </summary>
    /// <param name="shape">The shape object to update or add.</param>
    protected void UpdateShapeFromNetwork(IShape shape)
    {
        _shapes[shape.ShapeId] = shape;

        if (_selectedShape != null && _selectedShape.ShapeId == shape.ShapeId)
        {
            _selectedShape = shape;
            OnPropertyChanged(nameof(SelectedShape));
            _originalShapeForUndo = null; // Reset baseline

            if (_currentThickness != shape.Thickness)
            {
                _currentThickness = shape.Thickness;
                OnPropertyChanged(nameof(CurrentThickness));
            }
            if (_currentColor != shape.Color)
            {
                _currentColor = shape.Color;
                OnPropertyChanged(nameof(CurrentColor));
            }
        }
    }

    /// <summary>
    /// Template Method for processing actions. 
    /// Derived classes (Client/Host) override this to add networking logic.
    /// </summary>
    /// <param name="action">The action to process.</param>
    protected virtual void ProcessAction(CanvasAction action)
    {
        ApplyActionLocally(action);
    }

    /// <summary>
    /// Finalizes a modification (e.g., release mouse after drag, release slider).
    /// Creates a 'Modify' action for the Undo stack.
    /// </summary>
    public virtual void CommitModification()
    {
        if (_originalShapeForUndo != null && SelectedShape != null &&
            _originalShapeForUndo.ShapeId == SelectedShape.ShapeId)
        {
            if (_originalShapeForUndo.Color != SelectedShape.Color ||
                _originalShapeForUndo.Thickness != SelectedShape.Thickness)
            {
                CanvasAction action = new CanvasAction(CanvasActionType.Modify, _originalShapeForUndo, SelectedShape);
                ProcessAction(action);
            }
        }
        _originalShapeForUndo = null;
    }

    /// <summary>
    /// Performs a soft delete on the selected shape.
    /// </summary>
    public virtual void DeleteSelectedShape()
    {
        CommitModification();
        if (SelectedShape == null)
        {
            return;
        }

        IShape deletedShape = SelectedShape.WithDelete(CurrentUserId);

        CanvasAction deleteAction = new CanvasAction(
            CanvasActionType.Delete,
            SelectedShape,
            deletedShape
        );

        ProcessAction(deleteAction);
        SelectedShape = null;
    }

    /// <summary>
    /// Handles the completion of a mouse interaction (drawing or dropping).
    /// </summary>
    public void StopTracking()
    {
        if (_isMovingShape)
        {
            _isMovingShape = false;
            if (_originalShapeForMove != null && SelectedShape != null &&
                _originalShapeForMove.ShapeId == SelectedShape.ShapeId &&
                !_originalShapeForMove.Points.SequenceEqual(SelectedShape.Points))
            {
                // Create Modify Action for the move
                CanvasAction action = new CanvasAction(CanvasActionType.Modify, _originalShapeForMove, SelectedShape);
                ProcessAction(action);
            }
            _originalShapeForMove = null;
            return;
        }

        if (CurrentMode == DrawingMode.Select || !_isTracking)
        {
            _isTracking = false;
            return;
        }

        _isTracking = false;
        if (_trackedPoints.Count == 0)
        {
            return;
        }

        ShapeType? typeToCreate = CurrentMode switch {
            DrawingMode.FreeHand => ShapeType.FREEHAND,
            DrawingMode.StraightLine => ShapeType.LINE,
            DrawingMode.Rectangle => ShapeType.RECTANGLE,
            DrawingMode.EllipseShape => ShapeType.ELLIPSE,
            DrawingMode.TriangleShape => ShapeType.TRIANGLE,
            _ => null
        };

        if (typeToCreate.HasValue && _trackedPoints.Count >= 2)
        {
            IShape newShape = ShapeFactory.CreateShape(
                typeToCreate.Value,
                _trackedPoints,
                CurrentColor,
                CurrentThickness,
                CurrentUserId
            );

            CanvasAction action = new CanvasAction(CanvasActionType.Create, null, newShape);
            ProcessAction(action);
            LastCreatedShape = newShape;
        }
    }

    /// <summary>
    /// Initiates tracking when the mouse is pressed.
    /// </summary>
    /// <param name="point">The coordinate where tracking started.</param>
    public void StartTracking(Point point)
    {
        CommitModification();
        LastCreatedShape = null;

        if (CurrentMode == DrawingMode.Select)
        {
            _isTracking = false;
            SelectShapeAt(point);

            if (SelectedShape != null && SelectedShape.IsHit(point))
            {
                _isMovingShape = true;
                _moveStartPoint = point;
                _originalShapeForMove = SelectedShape;
            }
            return;
        }

        _isTracking = true;
        _isMovingShape = false;
        SelectedShape = null;

        _trackedPoints.Clear();
        _trackedPoints.Add(point);
        if (CurrentMode != DrawingMode.FreeHand)
        {
            _trackedPoints.Add(point);
        }
    }

    /// <summary>
    /// Attempts to select a shape at a specific coordinate.
    /// </summary>
    /// <param name="point">The coordinate to test for a hit.</param>
    public void SelectShapeAt(Point point)
    {
        CommitModification();
        SelectedShape = null;
        if (_shapes == null)
        {
            return;
        }
        foreach (IShape shape in _shapes.Values.Reverse())
        {
            if (!shape.IsDeleted && shape.IsHit(point))
            {
                SelectedShape = shape;
                return;
            }
        }
    }

    /// <summary>
    /// Updates tracking data as the mouse moves.
    /// </summary>
    /// <param name="point">The current mouse coordinate.</param>
    public void TrackPoint(Point point)
    {
        int x = point.X;
        int y = point.Y;

        if (CanvasBounds.Width > 0 && CanvasBounds.Height > 0)
        {
            x = Math.Max(CanvasBounds.Left, Math.Min(x, CanvasBounds.Right));
            y = Math.Max(CanvasBounds.Top, Math.Min(y, CanvasBounds.Bottom));
        }
        Point clampedPoint = new Point(x, y);

        if (_isMovingShape && SelectedShape != null && _originalShapeForMove != null)
        {
            Point offset = new Point(clampedPoint.X - _moveStartPoint.X, clampedPoint.Y - _moveStartPoint.Y);
            IShape movedShape = _originalShapeForMove.WithMove(offset, CanvasBounds, CurrentUserId);
            UpdateShapeFromNetwork(movedShape);
            RaiseRequestRedraw();
        }
        else if (_isTracking && _trackedPoints.Count > 0)
        {
            if (CurrentMode == DrawingMode.FreeHand)
            {
                _trackedPoints.Add(clampedPoint);
            }
            else
            {
                _trackedPoints[1] = clampedPoint;
            }
        }
    }

    /// <summary>
    /// Undoes the last action in the state manager.
    /// </summary>
    public virtual void Undo()
    {
        CommitModification();
        SelectedShape = null;
        CanvasAction? undoneAction = StateManager.Undo();
        if (undoneAction != null)
        {
            SyncDictionaryFromAction(undoneAction, true);
            RaiseRequestRedraw();
        }
    }

    /// <summary>
    /// Redoes the last undone action in the state manager.
    /// </summary>
    public virtual void Redo()
    {
        SelectedShape = null;
        CanvasAction? redoneAction = StateManager.Redo();
        if (redoneAction != null)
        {
            SyncDictionaryFromAction(redoneAction, false);
            RaiseRequestRedraw();
        }
    }

    /// <summary>
    /// Applies an action to the local state manager and dictionary.
    /// </summary>
    /// <param name="action">The action to apply.</param>
    protected void ApplyActionLocally(CanvasAction action)
    {
        StateManager.AddAction(action);
        if (action.NewShape != null)
        {
            UpdateShapeFromNetwork(action.NewShape);
        }
        RaiseRequestRedraw();
    }

    /// <summary>
    /// Updates the shapes dictionary based on an Undo or Redo action.
    /// </summary>
    /// <param name="action">The action being processed.</param>
    /// <param name="isUndo">True if undoing, False if redoing.</param>
    protected void SyncDictionaryFromAction(CanvasAction action, bool isUndo)
    {
        IShape? shapeToApply = isUndo ? action.PrevShape : action.NewShape;

        if (action.ActionType == CanvasActionType.Create)
        {
            if (isUndo)
            {
                if (action.NewShape != null)
                {
                    UpdateShapeFromNetwork(action.NewShape.WithDelete("system"));
                }
            }
            else
            {
                if (action.NewShape != null)
                {
                    UpdateShapeFromNetwork(action.NewShape);
                }
            }
        }
        else if (shapeToApply != null)
        {
            UpdateShapeFromNetwork(shapeToApply);
        }
    }

    /// <summary>
    /// Uses the processing service to regularize the selected shape (make it geometrically perfect).
    /// </summary>
    public async void RegularizeSelectedShape()
    {
        if (SelectedShape == null) return;
        CommitModification(); // Ensure any pending edits are saved first

        string inputJson = CanvasSerializer.SerializeShapeManual(SelectedShape);

        try
        {
            System.Diagnostics.Debug.WriteLine("[CanvasViewModel] RegularizeSelectedShape called with input: ");
            byte[] response = await Rpc.Call("canvas:regularize", DataSerializer.Serialize(inputJson));
            System.Diagnostics.Debug.WriteLine("[CanvasViewModel] RegularizeSelectedShape received response: ");
            string outputJson = DataSerializer.Deserialize<string>(response);

            IShape? regularizedShape = CanvasSerializer.DeserializeShapeManual(outputJson);
            System.Diagnostics.Debug.WriteLine("[CanvasViewModel] Regularized shape deserialized.");

            if (regularizedShape != null)
            {
                CanvasAction action = new CanvasAction(CanvasActionType.Modify, SelectedShape, regularizedShape);
                ProcessAction(action);
                SelectedShape = regularizedShape; // Keep selected
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Canvas] Regularize failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Triggers the analysis of a canvas image path.
    /// </summary>
    /// <param name="imagePath">File path to the image to analyze.</param>
    public async void PerformAnalysis(string imagePath)
    {
        // Toggle visibility automatically
        IsAnalysisVisible = true;
        AnalysisResult = "Analyzing...";
        try
        {
            byte[] response = await Rpc.Call("canvas:describe", Encoding.UTF8.GetBytes(imagePath));
            string result = Encoding.UTF8.GetString(response);
            Console.WriteLine($"[CanvasViewModel] Analysis result received. {result}");
            AnalysisResult = result;
        }
        catch (Exception ex)
        {
            AnalysisResult = $"Analysis failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Helper method to generate the inverse of an action (for networking undo).
    /// </summary>
    /// <param name="original">The original action.</param>
    /// <param name="userId">The ID of the user performing the inverse.</param>
    /// <returns>A new CanvasAction representing the inverse operation.</returns>
    protected CanvasAction GetInverseAction(CanvasAction original, string userId)
    {
        switch (original.ActionType)
        {
            case CanvasActionType.Create:
                IShape? shapeToDelete = original.NewShape;
                IShape? deletedShape = shapeToDelete?.WithDelete(userId);
                return new CanvasAction(original.ActionId, CanvasActionType.Delete, shapeToDelete, deletedShape);
            case CanvasActionType.Delete:
                return new CanvasAction(original.ActionId, CanvasActionType.Resurrect, original.NewShape, original.PrevShape);
            case CanvasActionType.Modify:
                return new CanvasAction(original.ActionId, CanvasActionType.Modify, original.NewShape, original.PrevShape);
            case CanvasActionType.Resurrect:
                IShape? shapeToKill = original.NewShape;
                IShape? killedShape = shapeToKill?.WithDelete(userId);
                return new CanvasAction(original.ActionId, CanvasActionType.Delete, original.NewShape, killedShape);
            default:
                return original;
        }
    }

    /// <summary>
    /// Saves the current shapes dictionary to a JSON file.
    /// </summary>
    public void SaveShapes()
    {
        if (_shapes == null)
        {
            return;
        }

        SaveFileDialog saveDialog = new SaveFileDialog {
            Filter = "Canvas JSON (*.json)|*.json",
            FileName = "canvas_shapes.json"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                string json = CanvasSerializer.SerializeShapesDictionary(_shapes);
                File.WriteAllText(saveDialog.FileName, json);
                Console.WriteLine($"[Host] Shapes saved to {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Host] Save failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Loads the canvas state from a JSON dictionary string.
    /// </summary>
    /// <param name="jsonDictionary">The JSON string representing the dictionary of shapes.</param>
    public void ApplyRestore(string jsonDictionary)
    {
        try
        {
            Dictionary<string, IShape> loadedShapes = CanvasSerializer.DeserializeShapesDictionary(jsonDictionary);
            if (loadedShapes != null)
            {
                _shapes = loadedShapes;
                SelectedShape = null;
                StateManager.ImportState(new SerializedActionStack());
                RaiseRequestRedraw();
                Console.WriteLine("[Canvas] State Restored.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Canvas] Restore failed: {ex.Message}");
        }
    }
}
