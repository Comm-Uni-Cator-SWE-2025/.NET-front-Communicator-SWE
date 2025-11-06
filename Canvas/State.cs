using System.Collections.ObjectModel;
using System.Text;


namespace CanvasDataModel;

public class State
{
    private ObservableCollection<IShape> _shapes;

    public List<IShape> Shapes { get; }

    public State(List<IShape> shapes)
    {
        // copy so we freeze this moment
        Shapes = new List<IShape>(shapes);
    }

    public State(ObservableCollection<IShape> shapes)
    {
        Shapes = new List<IShape>(shapes);
    }
}
