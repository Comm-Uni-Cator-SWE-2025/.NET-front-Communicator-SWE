using System.Text.Json.Serialization;

namespace Communicator.Controller.Meeting;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionMode
{
    TEST,
    CLASS
}
