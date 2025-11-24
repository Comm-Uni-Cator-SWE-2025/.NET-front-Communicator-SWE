namespace Communicator.App.ViewModels;

/// <summary>
/// A simple data class to hold client information (hostname and port).
/// This is the .NET equivalent of a tuple(string, port).
/// </summary>
/// <param name="HostName">The IP address.</param>
/// <param name="Port">The PORT.</param>
public record ClientNode(string HostName, int Port)
{
    public override int GetHashCode()
    {
        return string.GetHashCode(HostName + Port, System.StringComparison.Ordinal);
    }
}
