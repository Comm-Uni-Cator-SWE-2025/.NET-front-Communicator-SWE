using MeetAnalyticsApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MeetAnalyticsApp.ViewModels
{
    /// <summary>
    /// ViewModel that exposes meeting statistics and announcements to the view.
    /// Currently uses static values; replace with service integration when needed.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private MeetStats _meetStats = new()
        {
            UsersPresent = 120,
            UsersLoggedOut = 15,
            PreviousSummary = "The previous meeting was very productive and tasks were assigned."
        };

        /// <summary>
        /// Holds the current meeting statistics for display in the UI.
        /// </summary>
        public MeetStats MeetStats
        {
            get => _meetStats;
            set
            {
                _meetStats = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Collection of meeting messages or announcements.
        /// </summary>
        public ObservableCollection<string> Messages { get; } = new()
        {
            "Welcome to the meeting",
            "Design update will be shared soon",
            "Team is progressing well"
        };

        /// <summary>
        /// Initializes the ViewModel with placeholder data.
        /// </summary>
        public MainViewModel()
        {
            // Static / hardcoded sample data only.
        }

        #region INotifyPropertyChanged

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises a property changed event to update bindings.
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
