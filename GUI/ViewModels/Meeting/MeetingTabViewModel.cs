using GUI.Core;

namespace GUI.ViewModels.Meeting
{
    public class MeetingTabViewModel : ObservableObject
    {
        private string _header;

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

        public object ContentViewModel { get; }
    }
}
