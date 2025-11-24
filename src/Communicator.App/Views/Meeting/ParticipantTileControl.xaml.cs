/*
 * -----------------------------------------------------------------------------
 *  File: ParticipantTileControl.xaml.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows;
using System;

namespace Communicator.App.Views.Meeting;

/// <summary>
/// Interaction logic for ParticipantTileControl.xaml
/// </summary>
public sealed partial class ParticipantTileControl : UserControl
{
    public event EventHandler MaximizeRequested;

    public ParticipantTileControl()
    {
        InitializeComponent();
        this.MouseEnter += ParticipantTileControl_MouseEnter;
        this.MouseLeave += ParticipantTileControl_MouseLeave;
    }

    private void ParticipantTileControl_MouseEnter(object sender, MouseEventArgs e)
    {
        var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
        HoverOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    }

    private void ParticipantTileControl_MouseLeave(object sender, MouseEventArgs e)
    {
        var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
        HoverOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        MaximizeRequested?.Invoke(this, EventArgs.Empty);
    }
}


