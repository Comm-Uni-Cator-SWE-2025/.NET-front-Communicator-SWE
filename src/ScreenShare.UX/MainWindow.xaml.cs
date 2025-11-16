using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ScreenShare.UX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ParticipantData> Participants { get; set; }
        private int participantCounter = 1; // Counter for generating participant names

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize with the main user
            Participants = new ObservableCollection<ParticipantData>();
            Participants.Add(new ParticipantData 
            { 
                Initial = "Y", 
                Username = "You", 
                DisplayName = "You", 
                IsMainUser = true 
            });
            
            // Listen to collection changes to update grid layout
            Participants.CollectionChanged += Participants_CollectionChanged;
            
            this.DataContext = this;
            
            // Update grid layout after window is loaded
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateParticipantGridLayout();
        }

        private void Participants_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Update the grid layout when participants are added or removed
            Dispatcher.InvokeAsync(() => UpdateParticipantGridLayout(), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateParticipantGridLayout()
        {
            // Find the UniformGrid in the visual tree
            var uniformGrid = FindVisualChild<UniformGrid>(ParticipantsItemsControl);
            if (uniformGrid != null)
            {
                int participantCount = Participants.Count;
                
                // Dynamic layout based on participant count
                // 1 participant: Full screen (1×1)
                // 2 participants: Split horizontally (1×2)
                // 3-4 participants: 2×2 grid
                // 5+ participants: 2×2 grid with scrolling
                
                if (participantCount == 1)
                {
                    // Single participant: Full screen
                    uniformGrid.Rows = 1;
                    uniformGrid.Columns = 1;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else if (participantCount == 2)
                {
                    // Two participants: Split into 2 columns, 1 row
                    uniformGrid.Rows = 1;
                    uniformGrid.Columns = 2;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else if (participantCount >= 3 && participantCount <= 4)
                {
                    // 3-4 participants: 2×2 grid (one empty slot for 3 participants)
                    uniformGrid.Rows = 2;
                    uniformGrid.Columns = 2;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else // participantCount > 4
                {
                    // More than 4 participants: 2×2 grid with scrolling
                    uniformGrid.Rows = 0; // Auto-calculate rows based on columns
                    uniformGrid.Columns = 2;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
            }
        }

        // Helper method to find child elements in visual tree
        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var foundChild = FindVisualChild<T>(child);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }
            return null;
        }

        // Test button to add participants
        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            // Generate participant data
            string initial = GetInitialForParticipant(participantCounter);
            string username = $"username{participantCounter}";
            string displayName = $"displayname{participantCounter}";
            
            Participants.Add(new ParticipantData 
            { 
                Initial = initial, 
                Username = username, 
                DisplayName = displayName,
                IsMainUser = false 
            });
            
            participantCounter++;
        }

        // Remove participant button click handler
        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ParticipantData participant)
            {
                // Don't allow removing the main user
                if (!participant.IsMainUser)
                {
                    Participants.Remove(participant);
                }
            }
        }

        // Helper method to generate initials for participants
        private string GetInitialForParticipant(int number)
        {
            // Generate initials: U, V, W, X, Y, Z, A, B, C, etc.
            char[] letters = "UVWXYZABCDEFGHIJKLMNOPQRST".ToCharArray();
            int index = (number - 1) % letters.Length;
            return letters[index].ToString();
        }
    }

    public class ParticipantData : INotifyPropertyChanged
    {
        private string _initial = string.Empty;
        private string _username = string.Empty;
        private string _displayName = string.Empty;
        private bool _isMainUser;

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
