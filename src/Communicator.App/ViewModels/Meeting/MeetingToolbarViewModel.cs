/*
 * -----------------------------------------------------------------------------
 *  File: MeetingToolbarViewModel.cs
 *  Owner: Pramodh Sai
 *  Roll Number : 112201029
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Communicator.Core.UX;

namespace Communicator.App.ViewModels.Meeting;

/// <summary>
/// Event arguments for selected tab changes.
/// </summary>
public sealed class TabChangedEventArgs : EventArgs
{
    public MeetingTabViewModel? Tab { get; }

    public TabChangedEventArgs(MeetingTabViewModel? tab)
    {
        Tab = tab;
    }
}

/// <summary>
/// Represents the meeting tab strip, tracking available tabs and the currently selected one.
/// </summary>
public sealed class MeetingToolbarViewModel : ObservableObject
{
    private MeetingTabViewModel? _selectedTab;

    /// <summary>
    /// Initializes the toolbar with a predefined set of tabs.
    /// </summary>
    public MeetingToolbarViewModel(IEnumerable<MeetingTabViewModel> tabs)
    {
        ArgumentNullException.ThrowIfNull(tabs);

        Tabs = new ObservableCollection<MeetingTabViewModel>(tabs);
        _selectedTab = Tabs.FirstOrDefault();
    }

    public ObservableCollection<MeetingTabViewModel> Tabs { get; }

    public MeetingTabViewModel? SelectedTab
    {
        get => _selectedTab;
        set {
            if (SetProperty(ref _selectedTab, value))
            {
                SelectedTabChanged?.Invoke(this, new TabChangedEventArgs(value));
            }
        }
    }

    /// <summary>
    /// Raised whenever the selected tab changes so the shell can update navigation stacks.
    /// </summary>
    public event EventHandler<TabChangedEventArgs>? SelectedTabChanged;
}



