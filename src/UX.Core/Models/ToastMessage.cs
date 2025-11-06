using System;

namespace UX.Core.Models;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastMessage
{
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; }
    public int Duration { get; set; } = 3000;
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public ToastMessage()
    {
    }

    public ToastMessage(string message, ToastType type, int duration = 3000)
    {
        Message = message;
        Type = type;
        Duration = duration;
    }
}
