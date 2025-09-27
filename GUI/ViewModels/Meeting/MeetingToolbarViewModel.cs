using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GUI.Core;

namespace GUI.ViewModels.Meeting
{
    public class MeetingToolbarViewModel : ObservableObject
    {
        private MeetingTabViewModel? _selectedTab;

        public MeetingToolbarViewModel(IEnumerable<MeetingTabViewModel> tabs)
        {
            if (tabs == null)
            {
                throw new ArgumentNullException(nameof(tabs));
            }

            Tabs = new ObservableCollection<MeetingTabViewModel>(tabs);
            _selectedTab = Tabs.FirstOrDefault();
        }

        public ObservableCollection<MeetingTabViewModel> Tabs { get; }

        public MeetingTabViewModel? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    SelectedTabChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<MeetingTabViewModel?>? SelectedTabChanged;
    }
}
