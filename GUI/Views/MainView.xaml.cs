using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls.Primitives;
using GUI.Models;
using GUI.ViewModels;

namespace GUI.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        
        // Initialize Toast Container
        if (DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.ToastContainerViewModel = new ToastContainerViewModel(App.ToastService);
        }
        
        // Subscribe to toast service events
        App.ToastService.ToastRequested += OnToastRequested;
        
        SourceInitialized += OnSourceInitialized;
        StateChanged += (_, _) => UpdateWindowStateVisuals();
        Loaded += (_, _) => UpdateWindowStateVisuals();
    }

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

    private void OnToastRequested(ToastMessage toast)
    {
        Dispatcher.Invoke(() =>
        {
            var toastNotification = new ToastNotification();
            toastNotification.SetToast(toast);
            toastNotification.CloseRequested += (s, args) =>
            {
                ToastContainer.Items.Remove(toastNotification);
            };
            ToastContainer.Items.Add(toastNotification);
        });
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        ApplyWindowChromePreferences();
    }

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

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleWindowState()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        UpdateWindowStateVisuals();
    }

    private void UpdateWindowStateVisuals()
    {
        if (ChromeBorder == null || MaximizeButton == null || ContentHost == null)
        {
            return;
        }

        bool isMaximized = WindowState == WindowState.Maximized;

        ChromeBorder.CornerRadius = isMaximized  ? new CornerRadius(0) : new CornerRadius(12);
        ChromeBorder.BorderThickness = isMaximized ? new Thickness(0) : new Thickness(1);
        ContentHost.Margin = new Thickness(0);

        MaximizeButton.Content = isMaximized ? "\uE923" : "\uE922";
        MaximizeButton.ToolTip = isMaximized ? "Restore" : "Maximize";
    }

    private void ApplyWindowChromePreferences()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        try
        {
            var preference = DwmWindowCornerPreference.Round;
            const uint attributeSize = sizeof(uint);
            DwmSetWindowAttribute(handle, DwmWindowAttribute.WindowCornerPreference, ref preference, attributeSize);
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

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, ref DwmWindowCornerPreference pvAttribute, uint cbAttribute);
}