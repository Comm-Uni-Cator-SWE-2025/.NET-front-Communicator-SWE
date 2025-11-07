/**
 * A simple data class to hold client information (hostname and port).
 * This is the Java equivalent of a tuple(string, port).
 * @param hostName The IP address.
 * @param port The PORT.
 */
namespace Networking;
public record ClientNode(string HostName, int Port);
