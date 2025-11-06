using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UX.Core;

/// <summary>
/// Provides INotifyPropertyChanged support for derived view models.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets the backing field to the new value and raises PropertyChanged when the value differs.
    /// Returns true when a change was applied so callers can short-circuit additional work.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Notifies listeners that a property value changed. CallerMemberName ensures callers can omit the property name.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
