using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsApp.Models;
/// <summary>
/// Represents a single AI-generated message.
/// </summary>
public class AIMessageData
{    /// <summary>
     /// The time this message was received.
     /// </summary>
    public DateTime Time { get; set; }
    /// <summary>
    /// The actual message content.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
