/*
 * -----------------------------------------------------------------------------
 *  File: VideoSessionView.xaml.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Communicator.App.ViewModels.Meeting;
using System.Collections.Generic;
using System.Linq;

namespace Communicator.App.Views.Meeting;

/// <summary>
/// Provides the main meeting content area where participants interact.
/// </summary>
public sealed partial class VideoSessionView : UserControl
{
    /// <summary>
    /// Initializes session UI from its XAML definition.
    /// </summary>
    public VideoSessionView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        CalculateVisibleParticipants();
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        CalculateVisibleParticipants();
    }

    private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        CalculateVisibleParticipants();
    }

    private void ParticipantTileControl_MaximizeRequested(object sender, System.EventArgs e)
    {
        if (sender is ParticipantTileControl tile && tile.DataContext is ParticipantViewModel participant)
        {
            if (DataContext is VideoSessionViewModel viewModel)
            {
                viewModel.MaximizeParticipant(participant);
            }
        }
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is VideoSessionViewModel viewModel)
        {
            viewModel.RestoreGridView();
        }
    }

    private void CalculateVisibleParticipants()
    {
        if (DataContext is not VideoSessionViewModel viewModel)
        {
            return;
        }

        if (viewModel.IsMaximized)
        {
            return; // In maximized mode, visibility logic might be different or simplified
        }

        ScrollViewer scrollViewer = MainScrollViewer;
        if (scrollViewer == null)
        {
            return;
        }

        ItemsControl? itemsControl = FindVisualChild<ItemsControl>(scrollViewer);
        if (itemsControl == null)
        {
            return;
        }

        List<string> visibleParticipants = new List<string>();
        Rect viewport = new Rect(0, 0, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);

        foreach (object item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is not FrameworkElement container)
            {
                continue;
            }

            GeneralTransform transform = container.TransformToAncestor(scrollViewer);
            Rect itemRect = transform.TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight));

            if (viewport.IntersectsWith(itemRect))
            {
                if (item is ParticipantViewModel participant && !string.IsNullOrEmpty(participant.User.Email))
                {
                    visibleParticipants.Add(participant.User.Email);
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"[MeetingVideoSessionView] Visible participants calculated: {string.Join(", ", visibleParticipants)}");

        viewModel.UpdateVisibleParticipants(visibleParticipants);
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            T? result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}



