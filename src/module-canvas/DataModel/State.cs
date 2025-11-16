namespace CanvasApp.DataModel;

/// <summary>
/// Represents a single undo-able/redo-able action on the canvas.
/// This replaces the old 'State' class which stored the entire canvas snapshot.
/// </summary>
public class CanvasAction
{
    /// <summary>
    /// The type of action that was performed (e.g., Create, Modify, Delete).
    /// </summary>
    public CanvasActionType ActionType { get; }

    /// <summary>
    /// The state of the shape *before* the action.
    /// - For Create: null
    /// - For Modify: The shape before modification.
    /// - For Delete: The shape being deleted.
    /// </summary>
    public IShape? PrevShape { get; }

    /// <summary>
    /// The state of the shape *after* the action.
    /// - For Create: The new shape.
    /// - For Modify: The shape after modification.
    /// - For Delete: null
    /// </summary>
    public IShape? NewShape { get; }

    public CanvasAction(CanvasActionType actionType, IShape? prevShape, IShape? newShape)
    {
        ActionType = actionType;
        PrevShape = prevShape;
        NewShape = newShape;
    }
}
