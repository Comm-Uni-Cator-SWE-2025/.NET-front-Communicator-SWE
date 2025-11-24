using System.ComponentModel;

namespace ScreenShare.UX
{
    public class ParticipantData : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _initial = string.Empty;
        private string _username = string.Empty;
        private string _displayName = string.Empty;
        private bool _isMainUser;

        /// <summary>
        /// Unique identifier for the participant (e.g., IP address, user ID)
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Initial
        {
            get => _initial;
            set
            {
                if (_initial != value)
                {
                    _initial = value;
                    OnPropertyChanged(nameof(Initial));
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public bool IsMainUser
        {
            get => _isMainUser;
            set
            {
                if (_isMainUser != value)
                {
                    _isMainUser = value;
                    OnPropertyChanged(nameof(IsMainUser));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
