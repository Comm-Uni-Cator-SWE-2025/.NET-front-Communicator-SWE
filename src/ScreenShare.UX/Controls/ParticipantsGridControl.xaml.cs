using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WpfApp1.Controls
{
    /// <summary>
    /// Interaction logic for ParticipantsGridControl.xaml
    /// </summary>
    public partial class ParticipantsGridControl : UserControl
    {
        public static readonly DependencyProperty ParticipantsProperty =
            DependencyProperty.Register("Participants", typeof(ObservableCollection<ParticipantData>), 
                typeof(ParticipantsGridControl), new PropertyMetadata(null, OnParticipantsChanged));

        public ObservableCollection<ParticipantData> Participants
        {
            get { return (ObservableCollection<ParticipantData>)GetValue(ParticipantsProperty); }
            set { SetValue(ParticipantsProperty, value); }
        }

        public ParticipantsGridControl()
        {
            InitializeComponent();
            this.Loaded += ParticipantsGridControl_Loaded;
        }

        private void ParticipantsGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateParticipantGridLayout();
        }

        private static void OnParticipantsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticipantsGridControl;
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

                control.UpdateParticipantGridLayout();
            }
        }

        private void Participants_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() => UpdateParticipantGridLayout(), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateParticipantGridLayout()
        {
            if (Participants == null) return;

            var uniformGrid = FindVisualChild<UniformGrid>(ParticipantsItemsControl);
            if (uniformGrid != null)
            {
                int participantCount = Participants.Count;
                
                if (participantCount == 1)
                {
                    uniformGrid.Rows = 1;
                    uniformGrid.Columns = 1;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else if (participantCount == 2)
                {
                    uniformGrid.Rows = 1;
                    uniformGrid.Columns = 2;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else if (participantCount >= 3 && participantCount <= 4)
                {
                    uniformGrid.Rows = 2;
                    uniformGrid.Columns = 2;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else
                {
                    uniformGrid.Rows = 0;
                    uniformGrid.Columns = 2;
                    ParticipantsScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
            }
        }

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
    }
}
