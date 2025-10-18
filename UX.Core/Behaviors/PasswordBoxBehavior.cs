using System.Windows;
using System.Windows.Controls;

namespace UX.Core.Behaviors;

/// <summary>
/// Attached behavior that enables binding to PasswordBox.Password property while maintaining security.
/// PasswordBox doesn't expose Password as a DependencyProperty for security reasons, so we use an attached property.
/// </summary>
public static class PasswordBoxBehavior
{
    private static bool _isUpdating;

    /// <summary>
    /// Attached property to bind the password value from ViewModel to PasswordBox.
    /// </summary>
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxBehavior),
            new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

    /// <summary>
    /// Attached property to enable/disable the password binding behavior.
    /// </summary>
    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxBehavior),
            new PropertyMetadata(false, OnBindPasswordChanged));

    /// <summary>
    /// Gets the bound password value.
    /// </summary>
    public static string GetBoundPassword(DependencyObject d)
    {
        return (string)d.GetValue(BoundPasswordProperty);
    }

    /// <summary>
    /// Sets the bound password value.
    /// </summary>
    public static void SetBoundPassword(DependencyObject d, string value)
    {
        d.SetValue(BoundPasswordProperty, value);
    }

    /// <summary>
    /// Gets whether password binding is enabled.
    /// </summary>
    public static bool GetBindPassword(DependencyObject d)
    {
        return (bool)d.GetValue(BindPasswordProperty);
    }

    /// <summary>
    /// Sets whether password binding is enabled.
    /// </summary>
    public static void SetBindPassword(DependencyObject d, bool value)
    {
        d.SetValue(BindPasswordProperty, value);
    }

    /// <summary>
    /// Called when BindPassword property changes. Attaches/detaches the PasswordChanged event handler.
    /// </summary>
    private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
            return;

        if ((bool)e.OldValue)
        {
            passwordBox.PasswordChanged -= OnPasswordChanged;
        }

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += OnPasswordChanged;
        }
    }

    /// <summary>
    /// Called when BoundPassword property changes in the ViewModel. Updates the PasswordBox.
    /// </summary>
    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox || _isUpdating)
            return;

        _isUpdating = true;
        passwordBox.Password = (string)e.NewValue ?? string.Empty;
        _isUpdating = false;
    }

    /// <summary>
    /// Called when the PasswordBox.Password changes. Updates the bound property in ViewModel.
    /// </summary>
    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox || _isUpdating)
            return;

        _isUpdating = true;
        SetBoundPassword(passwordBox, passwordBox.Password);
        _isUpdating = false;
    }
}
