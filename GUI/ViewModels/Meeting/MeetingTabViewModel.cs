using GUI.Core;

namespace GUI.ViewModels.Meeting
{
    /// <summary>
    /// Simple holder for a meeting tab header and its corresponding page view model.
    /// </summary>
    public class MeetingTabViewModel : ObservableObject
    {
        private string _header;

        /// <summary>
        /// Creates a meeting tab with the label to display and the view model it hosts.
        /// </summary>
        public MeetingTabViewModel(string header, object contentViewModel)
        {
            _header = header;
            ContentViewModel = contentViewModel;
        }

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// The view model rendered when this tab is active.
        /// </summary>
        public object ContentViewModel { get; }
    }
}
