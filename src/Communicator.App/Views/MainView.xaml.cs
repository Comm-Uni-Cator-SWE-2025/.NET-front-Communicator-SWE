/*
 * -----------------------------------------------------------------------------
 *  File: MainView.xaml.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Communicator.Core.UX.Models;
using Communicator.Core.UX.Services;
using Communicator.App.ViewModels;
using Communicator.App.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Communicator.App.Views;

/// <summary>
/// Primary application window that hosts navigation, toast notifications, and custom chrome behaviors.
/// </summary>
public sealed partial class MainView : Window
{
    private readonly IToastService _toastService;
    private HwndSource? _hwndSource;

    /// <summary>
    /// Initializes the window, wires up toast handling, and configures chrome behavior.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        // Get ToastService from DI
        _toastService = MainApp.Services.GetRequiredService<IToastService>();

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
        _hwndSource = (HwndSource?)PresentationSource.FromVisual(this);
        _hwndSource?.AddHook(WndProc);
        ApplyWindowChromePreferences();
    }

    /// <summary>
    /// Handles Windows messages to fix maximized window positioning.
    /// </summary>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_GETMINMAXINFO = 0x0024;

        if (msg == WM_GETMINMAXINFO)
        {
            // Get the monitor information for the window
            MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);

            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new() {
                    _cbSize = Marshal.SizeOf<MONITORINFO>()
                };

                if (GetMonitorInfo(monitor, ref monitorInfo))
                {
                    RECT rcWorkArea = monitorInfo._rcWork;
                    RECT rcMonitorArea = monitorInfo._rcMonitor;

                    // Set the maximized size to the work area (excluding taskbar)
                    mmi._ptMaxPosition._x = Math.Abs(rcWorkArea._left - rcMonitorArea._left);
                    mmi._ptMaxPosition._y = Math.Abs(rcWorkArea._top - rcMonitorArea._top);
                    mmi._ptMaxSize._x = Math.Abs(rcWorkArea._right - rcWorkArea._left);
                    mmi._ptMaxSize._y = Math.Abs(rcWorkArea._bottom - rcWorkArea._top);

                    Marshal.StructureToPtr(mmi, lParam, true);
                    handled = true;
                }
            }
        }

        return IntPtr.Zero;
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

        if (isMaximized)
        {
            // WM_GETMINMAXINFO handles proper positioning, no need for manual margin
            ChromeBorder.Margin = new Thickness(0);
            ChromeBorder.CornerRadius = new CornerRadius(0);
            ChromeBorder.BorderThickness = new Thickness(0);
        }
        else
        {
            // Normal state - restore rounded corners and border
            ChromeBorder.Margin = new Thickness(0);
            ChromeBorder.CornerRadius = new CornerRadius(12);
            ChromeBorder.BorderThickness = new Thickness(1);
        }

        ContentHost.Margin = new Thickness(0);

        if (MaximizeButton.Content is Communicator.Icons.Icon icon)
        {
            icon.IconName = isMaximized ? "copy" : "square";
        }

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
            const uint AttributeSize = sizeof(uint);
            int result = DwmSetWindowAttribute(handle, DwmWindowAttribute.WindowCornerPreference, ref preference, AttributeSize);

            // Check HRESULT - 0 indicates success
            if (result != 0)
            {
                // Non-zero HRESULT indicates failure, but we can ignore it since it's cosmetic
                System.Diagnostics.Debug.WriteLine($"[MainView] DwmSetWindowAttribute returned HRESULT: 0x{result:X}");
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

    // Windows API for monitor information
    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int _cbSize;
        public RECT _rcMonitor;
        public RECT _rcWork;
        public uint _dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int _left;
        public int _top;
        public int _right;
        public int _bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int _x;
        public int _y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT _ptReserved;
        public POINT _ptMaxSize;
        public POINT _ptMaxPosition;
        public POINT _ptMinTrackSize;
        public POINT _ptMaxTrackSize;
    }
}


