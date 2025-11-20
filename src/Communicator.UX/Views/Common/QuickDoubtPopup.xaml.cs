/*
 * -----------------------------------------------------------------------------
 *  File: QuickDoubtPopup.xaml.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Communicator.UX.Views;

/// <summary>
/// Visual control for displaying Quick Doubt messages at the top-center of the meeting view.
/// Can be dismissed by clicking the close button or clicking outside the popup.
/// </summary>
public sealed partial class QuickDoubtPopup : UserControl
{
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Sender name property for data binding
    /// </summary>
    public string SenderName
    {
        get => (string)GetValue(SenderNameProperty);
        set => SetValue(SenderNameProperty, value);
    }

    public static readonly DependencyProperty SenderNameProperty =
        DependencyProperty.Register("SenderName", typeof(string), typeof(QuickDoubtPopup), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Message property for data binding
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register("Message", typeof(string), typeof(QuickDoubtPopup), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Initializes the Quick Doubt popup.
    /// </summary>
    public QuickDoubtPopup()
    {
        InitializeComponent();
        // Don't set DataContext - it will be inherited from parent or set via bindings
    }

    /// <summary>
    /// Begins the entrance animation once the control is visible.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimateIn();
    }

    /// <summary>
    /// Slides the popup in from the left.
    /// </summary>
    private void AnimateIn()
    {
        var slideIn = new DoubleAnimation {
            From = -100,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(350),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeIn = new DoubleAnimation {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        var translateTransform = new System.Windows.Media.TranslateTransform();
        RenderTransform = translateTransform;

        translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
        BeginAnimation(OpacityProperty, fadeIn);
    }

    /// <summary>
    /// Slides and fades the popup out, notifying listeners when complete.
    /// </summary>
    private void AnimateOut()
    {
        var slideOut = new DoubleAnimation {
            To = -100,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var fadeOut = new DoubleAnimation {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(250)
        };

        slideOut.Completed += (s, e) => {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        };

        if (RenderTransform is System.Windows.Media.TranslateTransform translateTransform)
        {
            translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
        }
        BeginAnimation(OpacityProperty, fadeOut);
    }

    /// <summary>
    /// Immediately dismisses the popup when the close button is clicked.
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AnimateOut();
    }
}


