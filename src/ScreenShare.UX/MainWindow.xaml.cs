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
            Participants.Add(new ParticipantData {
                Id = "main_user",
                Initial = "Y",
                Username = "You",
                DisplayName = "You",
                IsMainUser = true
            });

            this.DataContext = this;

            // Subscribe to visible participants changed event
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("\n######## MainWindow Loaded - Subscribing to VisibleParticipantsChanged ########\n");
            
            // Subscribe to the ParticipantsGridControl's VisibleParticipantsChanged event
            if (ParticipantsGridControl != null)
            {
                ParticipantsGridControl.VisibleParticipantsChanged += OnVisibleParticipantsChanged;
                System.Diagnostics.Debug.WriteLine("✓ Successfully subscribed to VisibleParticipantsChanged event");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✗ ParticipantsGridControl is NULL - cannot subscribe");
            }
            
            System.Diagnostics.Debug.WriteLine("###############################################################\n");
        }

        /// <summary>
        /// Handler for visible participants changed event
        /// This can be used by ViewModels to update the backend about which participants are visible
        /// </summary>
        private void OnVisibleParticipantsChanged(object? sender, Controls.VisibleParticipantsChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("\n");
            System.Diagnostics.Debug.WriteLine("████████████████████████████████████████████████████████████");
            System.Diagnostics.Debug.WriteLine("█  EVENT RECEIVED: VisibleParticipantsChanged             █");
            System.Diagnostics.Debug.WriteLine("████████████████████████████████████████████████████████████");
            System.Diagnostics.Debug.WriteLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
            System.Diagnostics.Debug.WriteLine($"Visible Participants Count: {e.VisibleParticipantIds.Count}");
            System.Diagnostics.Debug.WriteLine($"Visible Participant IDs:");
            
            foreach (var id in e.VisibleParticipantIds)
            {
                var participant = Participants.FirstOrDefault(p => p.Id == id);
                if (participant != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  ✓ {id} → {participant.Username}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  ? {id} → [Unknown participant]");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("████████████████████████████████████████████████████████████\n");

            // TODO: This is where you would notify your ViewModel/Model
            // Example: ScreenNVideoModel.UpdateVisibleParticipants(e.VisibleParticipantIds);
        }

        // Test button to add participants
        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            // Generate participant data
            string initial = GetInitialForParticipant(participantCounter);
            string username = $"username{participantCounter}";
            string displayName = $"displayname{participantCounter}";
            string id = $"participant_{participantCounter}"; // Unique ID for each participant

            Participants.Add(new ParticipantData {
                Id = id,
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
}
