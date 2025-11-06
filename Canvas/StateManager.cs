using System.Text;


namespace CanvasDataModel;

public class StateNode
{
    public State State { get; }
    public StateNode? Prev { get; set; }
    public StateNode? Next { get; set; }

    public StateNode(State state)
    {
        State = state;
    }
}

public class StateManager
{
    private StateNode? _current;

    public StateManager()
    {
        // Start with a null/empty state (clear canvas)
        AddState(new State(new List<IShape>()));
    }
    public void AddState(State state)
    {
        var node = new StateNode(state);

        if (_current != null)
        {
            // cut off redo chain
            _current.Next = null;
            node.Prev = _current;
            _current.Next = node;
        }

        _current = node;
    }

    public State? Undo()
    {
        if (_current?.Prev != null)
        {
            _current = _current.Prev;
            return _current.State;
        }
        return null;
    }

    public State? Redo()
    {
        if (_current?.Next != null)
        {
            _current = _current.Next;
            return _current.State;
        }
        return null;
    }

    public State? Current => _current?.State;
}
