using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Communicator.UX.ViewModels;
using Communicator.UX.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Communicator.Core.UX.Models;
using Communicator.Core.UX.Services;

namespace Communicator.UX.Views;

/// <summary>
/// Primary application window that hosts navigation, toast notifications, and custom chrome behaviors.
/// </summary>
public partial class MainView : Window
{
    private readonly IToastService _toastService;

    /// <summary>
    /// Initializes the window, wires up toast handling, and configures chrome behavior.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        // Get ToastService from DI
        _toastService = App.Services.GetRequiredService<IToastService>();

        // Subscribe to toast service events

        _toastService.ToastRequested += OnToastRequested;


        SourceInitialized += OnSourceInitialized;
        StateChanged += (_, _) => UpdateWindowStateVisuals();
        Loaded += (_, _) => UpdateWindowStateVisuals();
    }

    /// <summary>
    /// Displays the profile context menu anchored to the avatar button.
    /// </summary>
    private void OnAvatarButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.ContextMenu == null)
        {
            return;
        }

        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.Placement = PlacementMode.Bottom;
        button.ContextMenu.DataContext = button.DataContext;
        button.ContextMenu.IsOpen = true;
        e.Handled = true;
    }

    /// <summary>
    /// Renders toast notifications and tracks their lifetime within the visual tree.
    /// </summary>
    private void OnToastRequested(object? sender, ToastRequestedEventArgs e)
    {
        Dispatcher.Invoke(() => {
            var toastNotification = new ToastNotification();
            toastNotification.SetToast(e.Message);
            toastNotification.CloseRequested += (s, args) => {
                ToastContainer.Items.Remove(toastNotification);
            };
            ToastContainer.Items.Add(toastNotification);
        });
    }

    /// <summary>
    /// Applies custom chrome preferences when the native window handle becomes available.
    /// </summary>
    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        ApplyWindowChromePreferences();
    }

    /// <summary>
    /// Enables dragging and double-click maximize behavior on the custom title bar.
    /// </summary>
    private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && ResizeMode == ResizeMode.CanResize)
        {
            ToggleWindowState();
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // Ignore drag exceptions triggered while the window is transitioning states.
        }
    }

    /// <summary>
    /// Minimizes the window when the minimize caption button is clicked.
    /// </summary>
    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Toggles between maximized and restored states from the maximize caption button.
    /// </summary>
    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    /// <summary>
    /// Closes the window when the close caption button is clicked.
    /// </summary>
    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Switches window state and refreshes visuals to match the new state.
    /// </summary>
    private void ToggleWindowState()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        UpdateWindowStateVisuals();
    }

    /// <summary>
    /// Updates chrome visuals (corner radius, borders, title bar icons) to match the current window state.
    /// </summary>
    private void UpdateWindowStateVisuals()
    {
        if (ChromeBorder == null || MaximizeButton == null || ContentHost == null)
        {
            return;
        }

        bool isMaximized = WindowState == WindowState.Maximized;

        ChromeBorder.CornerRadius = isMaximized ? new CornerRadius(0) : new CornerRadius(12);
        ChromeBorder.BorderThickness = isMaximized ? new Thickness(0) : new Thickness(1);
        ContentHost.Margin = new Thickness(0);

        MaximizeButton.Content = isMaximized ? "\uE923" : "\uE922";
        MaximizeButton.ToolTip = isMaximized ? "Restore" : "Maximize";
    }

    /// <summary>
    /// Requests rounded corner preference from the Desktop Window Manager when available.
    /// </summary>
    private void ApplyWindowChromePreferences()
    {
        nint handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        try
        {
            DwmWindowCornerPreference preference = DwmWindowCornerPreference.Round;
            const uint attributeSize = sizeof(uint);
            int result = DwmSetWindowAttribute(handle, DwmWindowAttribute.WindowCornerPreference, ref preference, attributeSize);

            // Check HRESULT - 0 indicates success
            if (result != 0)
            {
                // Non-zero HRESULT indicates failure, but we can ignore it since it's cosmetic
                System.Diagnostics.Debug.WriteLine($"DwmSetWindowAttribute returned HRESULT: 0x{result:X}");
            }
        }
        catch (DllNotFoundException)
        {
            // Desktop Window Manager not available; ignore.
        }
        catch (EntryPointNotFoundException)
        {
            // Attribute not supported on this OS version; ignore.
        }
    }

    private enum DwmWindowAttribute
    {
        WindowCornerPreference = 33
    }

    private enum DwmWindowCornerPreference : uint
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, ref DwmWindowCornerPreference pvAttribute, uint cbAttribute);
}

