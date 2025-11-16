using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Controls
{
    /// <summary>
    /// Interaction logic for ParticipantsListControl.xaml
    /// </summary>
    public partial class ParticipantsListControl : UserControl
    {
        public static readonly DependencyProperty ParticipantsProperty =
            DependencyProperty.Register("Participants", typeof(ObservableCollection<ParticipantData>), 
                typeof(ParticipantsListControl), new PropertyMetadata(null, OnParticipantsChanged));

        public ObservableCollection<ParticipantData> Participants
        {
            get { return (ObservableCollection<ParticipantData>)GetValue(ParticipantsProperty); }
            set { SetValue(ParticipantsProperty, value); }
        }

        public event RoutedEventHandler? AddParticipantClick;
        public event RoutedEventHandler? RemoveParticipantClick;

        public ParticipantsListControl()
        {
            InitializeComponent();
            AddTestUserButton.Click += AddTestUserButton_Click;
        }

        private static void OnParticipantsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticipantsListControl;
            if (control != null)
            {
                control.ParticipantsItemsControl.ItemsSource = e.NewValue as ObservableCollection<ParticipantData>;
                
                if (e.OldValue is ObservableCollection<ParticipantData> oldCollection)
                {
                    oldCollection.CollectionChanged -= control.Participants_CollectionChanged;
                }

                if (e.NewValue is ObservableCollection<ParticipantData> newCollection)
                {
                    newCollection.CollectionChanged += control.Participants_CollectionChanged;
                }

                control.UpdateParticipantCount();
            }
        }

        private void Participants_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateParticipantCount();
        }

        private void UpdateParticipantCount()
        {
            if (Participants != null)
            {
                ParticipantCountTextBlock.Text = $"Participants ( {Participants.Count} )";
            }
        }

        private void AddTestUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddParticipantClick?.Invoke(this, e);
        }

        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            RemoveParticipantClick?.Invoke(sender, e);
        }
    }
}
