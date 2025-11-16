namespace Canvas.DataModel;

/// <summary>
/// Internal node for the doubly-linked list, now holding a CanvasAction.
/// </summary>
public class ActionNode
{
    public CanvasAction Action { get; }
    public ActionNode? Prev { get; set; }
    public ActionNode? Next { get; set; }

    public ActionNode(CanvasAction action)
    {
        Action = action;
    }
}

/// <summary>
/// Manages the undo/redo stack using a list of actions (Command Pattern).
/// </summary>
public class StateManager
{
    private ActionNode? _current;

    public StateManager()
    {
        // --- MODIFIED ---
        // Start with an 'Initial' action node, representing the empty canvas.
        _current = new ActionNode(new CanvasAction(CanvasActionType.Initial, null, null));
        // --- END MODIFIED ---
    }

    /// <summary>
    /// Adds a new action to the manager, cutting off the old redo chain.
    /// </summary>
    public void AddAction(CanvasAction action)
    {
        var node = new ActionNode(action);

        if (_current != null)
        {
            // cut off redo chain
            _current.Next = null;
            node.Prev = _current;
            _current.Next = node;
        }

        _current = node;
    }

    /// <summary>
    /// Moves the state backward, returning the action that was *undone*.
    /// </summary>
    public CanvasAction? Undo()
    {
        if (_current?.Prev != null)
        {
            CanvasAction actionToUndo = _current.Action;
            _current = _current.Prev;
            return actionToUndo; // Return the action that was just reversed
        }
        return null; // At the beginning
    }

    /// <summary>
    /// Moves the state forward, returning the action that was *redone*.
    /// </summary>
    public CanvasAction? Redo()
    {
        if (_current?.Next != null)
        {
            _current = _current.Next;
            return _current.Action; // Return the action that was just re-applied
        }
        return null; // At the end
    }

    /// <summary>
    /// Gets the current action on the stack.
    /// </summary>
    public CanvasAction? CurrentAction => _current?.Action;
}
