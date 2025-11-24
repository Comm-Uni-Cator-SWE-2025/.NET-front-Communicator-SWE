using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ScreenShare.UX.Controls
{
    /// <summary>
    /// Event args for visible participants changed event
    /// </summary>
    public class VisibleParticipantsChangedEventArgs : EventArgs
    {
        public HashSet<string> VisibleParticipantIds { get; set; }

        public VisibleParticipantsChangedEventArgs(HashSet<string> visibleParticipantIds)
        {
            VisibleParticipantIds = visibleParticipantIds;
        }
    }

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

        /// <summary>
        /// Event raised when the set of visible participants changes
        /// </summary>
        public event EventHandler<VisibleParticipantsChangedEventArgs>? VisibleParticipantsChanged;

        private ParticipantData? _maximizedParticipant;
        private ObservableCollection<ParticipantData> _thumbnailParticipants = new ObservableCollection<ParticipantData>();
        private HashSet<string> _lastVisibleParticipants = new HashSet<string>();
        private ScrollViewer? _thumbnailScrollViewer;

        public ParticipantsGridControl()
        {
            InitializeComponent();
            this.Loaded += ParticipantsGridControl_Loaded;
            
            // Subscribe to scroll events to recalculate visible participants
            ParticipantsScrollViewer.ScrollChanged += OnScrollChanged;
        }

        private void ParticipantsGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateParticipantGridLayout();
            // Calculate visible participants after initial load
            CalculateVisibleParticipants();
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Recalculate visible participants when scrolling
            CalculateVisibleParticipants();
        }

        /// <summary>
        /// Handler for thumbnail sidebar scroll changes
        /// </summary>
        private void OnThumbnailScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Recalculate visible participants when thumbnail sidebar is scrolled
            System.Diagnostics.Debug.WriteLine(">>> Thumbnail sidebar scrolled - recalculating visible participants");
            CalculateVisibleParticipants();
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
                // Recalculate visible participants when collection changes
                control.CalculateVisibleParticipants();
            }
        }

        private void Participants_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                UpdateParticipantGridLayout();
                CalculateVisibleParticipants();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Calculates which participants are currently visible on the screen
        /// Returns a HashSet of participant IDs that are currently visible
        /// </summary>
        public HashSet<string> CalculateVisibleParticipants()
        {
            var visibleIds = new HashSet<string>();

            System.Diagnostics.Debug.WriteLine("=== CalculateVisibleParticipants Called ===");
            System.Diagnostics.Debug.WriteLine($"Maximized Participant: {_maximizedParticipant?.Username ?? "None"}");

            // If a participant is maximized, check main participant and visible thumbnails
            if (_maximizedParticipant != null)
            {
                System.Diagnostics.Debug.WriteLine("*** MAXIMIZED MODE ***");
                
                // Add maximized participant (always visible in main area)
                if (!string.IsNullOrEmpty(_maximizedParticipant.Id))
                {
                    visibleIds.Add(_maximizedParticipant.Id);
                    System.Diagnostics.Debug.WriteLine($"  - Maximized: {_maximizedParticipant.Username} (ID: {_maximizedParticipant.Id})");
                }

                // Check which thumbnails are actually visible in the sidebar
                System.Diagnostics.Debug.WriteLine($"  - Total Thumbnails: {_thumbnailParticipants.Count}");
                
                // Find the thumbnail ScrollViewer (cached for performance)
                if (_thumbnailScrollViewer == null)
                {
                    _thumbnailScrollViewer = FindThumbnailScrollViewer();
                }

                if (_thumbnailScrollViewer != null)
                {
                    // Get the sidebar viewport rectangle
                    var sidebarViewportRect = new Rect(
                        _thumbnailScrollViewer.HorizontalOffset,
                        _thumbnailScrollViewer.VerticalOffset,
                        _thumbnailScrollViewer.ViewportWidth,
                        _thumbnailScrollViewer.ViewportHeight
                    );

                    System.Diagnostics.Debug.WriteLine($"  - Sidebar Viewport: Y={sidebarViewportRect.Y:F2}, H={sidebarViewportRect.Height:F2}");

                    // Check each thumbnail's visibility
                    int thumbnailIndex = 0;
                    foreach (var thumbnail in _thumbnailParticipants)
                    {
                        if (string.IsNullOrEmpty(thumbnail.Id))
                        {
                            System.Diagnostics.Debug.WriteLine($"  [{thumbnailIndex}] {thumbnail.Username} - SKIPPED (No ID)");
                            thumbnailIndex++;
                            continue;
                        }

                        // Find the thumbnail border in the visual tree
                        var thumbnailBorder = FindThumbnailBorder(thumbnail);
                        if (thumbnailBorder != null)
                        {
                            try
                            {
                                // Get the thumbnail's position relative to the ThumbnailItemsControl
                                var transform = thumbnailBorder.TransformToAncestor(ThumbnailItemsControl);
                                var thumbnailPosition = transform.Transform(new Point(0, 0));
                                
                                var thumbnailRect = new Rect(
                                    thumbnailPosition.X,
                                    thumbnailPosition.Y,
                                    thumbnailBorder.ActualWidth,
                                    thumbnailBorder.ActualHeight
                                );

                                // Check if thumbnail intersects with the sidebar viewport
                                bool isVisible = sidebarViewportRect.IntersectsWith(thumbnailRect);
                                
                                System.Diagnostics.Debug.WriteLine($"  [{thumbnailIndex}] Thumbnail: {thumbnail.Username} (ID: {thumbnail.Id})");
                                System.Diagnostics.Debug.WriteLine($"       Position: Y={thumbnailRect.Y:F2}, H={thumbnailRect.Height:F2}");
                                System.Diagnostics.Debug.WriteLine($"       Visible: {isVisible}");
                                
                                if (isVisible)
                                {
                                    visibleIds.Add(thumbnail.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"  [{thumbnailIndex}] {thumbnail.Username} - ERROR: {ex.Message}");
                                // On error, consider it visible (safer default)
                                visibleIds.Add(thumbnail.Id);
                            }
                        }
                        else
                        {
                            // If border not found, assume visible (might not be rendered yet)
                            System.Diagnostics.Debug.WriteLine($"  [{thumbnailIndex}] {thumbnail.Username} - Border not found, assuming visible");
                            visibleIds.Add(thumbnail.Id);
                        }
                        
                        thumbnailIndex++;
                    }
                }
                else
                {
                    // Fallback: If we can't find the scrollviewer, mark all thumbnails as visible
                    System.Diagnostics.Debug.WriteLine("  - Thumbnail ScrollViewer not found, marking all as visible");
                    foreach (var thumbnail in _thumbnailParticipants)
                    {
                        if (!string.IsNullOrEmpty(thumbnail.Id))
                        {
                            visibleIds.Add(thumbnail.Id);
                            System.Diagnostics.Debug.WriteLine($"  - Thumbnail: {thumbnail.Username} (ID: {thumbnail.Id})");
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("*** GRID MODE ***");
                
                // Normal grid view - check which participants are visible in the scroll viewer
                if (Participants != null && ParticipantsScrollViewer != null)
                {
                    // Get the visible viewport rectangle
                    var viewportRect = new Rect(
                        ParticipantsScrollViewer.HorizontalOffset,
                        ParticipantsScrollViewer.VerticalOffset,
                        ParticipantsScrollViewer.ViewportWidth,
                        ParticipantsScrollViewer.ViewportHeight
                    );

                    System.Diagnostics.Debug.WriteLine($"  - Viewport: X={viewportRect.X:F2}, Y={viewportRect.Y:F2}, W={viewportRect.Width:F2}, H={viewportRect.Height:F2}");
                    System.Diagnostics.Debug.WriteLine($"  - Total Participants: {Participants.Count}");

                    // Check each participant's visibility
                    int participantIndex = 0;
                    foreach (var participant in Participants)
                    {
                        if (string.IsNullOrEmpty(participant.Id))
                        {
                            System.Diagnostics.Debug.WriteLine($"  [{participantIndex}] {participant.Username} - SKIPPED (No ID)");
                            participantIndex++;
                            continue;
                        }

                        // Find the Border element for this participant in the visual tree
                        var border = FindParticipantBorder(participant);
                        if (border != null)
                        {
                            try
                            {
                                // Get the border's position relative to the ItemsControl
                                var transform = border.TransformToAncestor(ParticipantsItemsControl);
                                var borderPosition = transform.Transform(new Point(0, 0));
                                
                                // Create rectangle for the border
                                var borderRect = new Rect(
                                    borderPosition.X,
                                    borderPosition.Y,
                                    border.ActualWidth,
                                    border.ActualHeight
                                );

                                // Check if the border intersects with the visible viewport
                                bool isVisible = viewportRect.IntersectsWith(borderRect);
                                
                                System.Diagnostics.Debug.WriteLine($"  [{participantIndex}] {participant.Username} (ID: {participant.Id})");
                                System.Diagnostics.Debug.WriteLine($"       Border: X={borderRect.X:F2}, Y={borderRect.Y:F2}, W={borderRect.Width:F2}, H={borderRect.Height:F2}");
                                System.Diagnostics.Debug.WriteLine($"       Visible: {isVisible}");
                                
                                if (isVisible)
                                {
                                    visibleIds.Add(participant.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"  [{participantIndex}] {participant.Username} - ERROR: {ex.Message}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  [{participantIndex}] {participant.Username} - Border NOT FOUND in visual tree");
                        }
                        
                        participantIndex++;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  - ParticipantsScrollViewer or Participants is NULL");
                }
            }

            System.Diagnostics.Debug.WriteLine($"*** RESULT: {visibleIds.Count} visible participants ***");
            System.Diagnostics.Debug.WriteLine($"Visible IDs: [{string.Join(", ", visibleIds)}]");

            // Only raise event if the visible participants have changed
            if (!visibleIds.SetEquals(_lastVisibleParticipants))
            {
                System.Diagnostics.Debug.WriteLine(">>> VISIBLE PARTICIPANTS CHANGED - Raising Event <<<");
                _lastVisibleParticipants = new HashSet<string>(visibleIds);
                OnVisibleParticipantsChanged(visibleIds);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(">>> No change in visible participants - Event NOT raised <<<");
            }
            
            System.Diagnostics.Debug.WriteLine("===========================================\n");

            return visibleIds;
        }

        /// <summary>
        /// Finds the ScrollViewer for the thumbnail sidebar
        /// </summary>
        private ScrollViewer? FindThumbnailScrollViewer()
        {
            if (MaximizedView == null || MaximizedView.Visibility != Visibility.Visible)
                return null;

            // Find the ScrollViewer in the MaximizedView (it's in Grid.Column="1", Grid.Row="1")
            var scrollViewers = new List<ScrollViewer>();
            FindScrollViewersInVisualTree(MaximizedView, scrollViewers);

            // Return the one that's not the ParticipantsScrollViewer
            return scrollViewers.FirstOrDefault(sv => sv != ParticipantsScrollViewer);
        }

        /// <summary>
        /// Recursively finds all ScrollViewers in the visual tree
        /// </summary>
        private void FindScrollViewersInVisualTree(DependencyObject parent, List<ScrollViewer> scrollViewers)
        {
            if (parent == null) return;

            if (parent is ScrollViewer sv)
            {
                scrollViewers.Add(sv);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                FindScrollViewersInVisualTree(child, scrollViewers);
            }
        }

        /// <summary>
        /// Finds the Border element for a specific participant in the visual tree
        /// </summary>
        private Border? FindParticipantBorder(ParticipantData participant)
        {
            if (ParticipantsItemsControl == null)
                return null;

            // Iterate through the ItemsControl's visual children
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ParticipantsItemsControl); i++)
            {
                var child = VisualTreeHelper.GetChild(ParticipantsItemsControl, i);
                var border = FindBorderWithDataContext(child, participant);
                if (border != null)
                    return border;
            }

            return null;
        }

        /// <summary>
        /// Finds the Border element for a specific thumbnail in the sidebar
        /// </summary>
        private Border? FindThumbnailBorder(ParticipantData participant)
        {
            if (ThumbnailItemsControl == null)
                return null;

            // Iterate through the ThumbnailItemsControl's visual children
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ThumbnailItemsControl); i++)
            {
                var child = VisualTreeHelper.GetChild(ThumbnailItemsControl, i);
                var border = FindBorderWithDataContext(child, participant);
                if (border != null)
                    return border;
            }

            return null;
        }

        /// <summary>
        /// Recursively searches for a Border with matching DataContext
        /// </summary>
        private Border? FindBorderWithDataContext(DependencyObject element, ParticipantData participant)
        {
            if (element is Border border && border.DataContext == participant)
            {
                return border;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var foundBorder = FindBorderWithDataContext(child, participant);
                if (foundBorder != null)
                    return foundBorder;
            }

            return null;
        }

        /// <summary>
        /// Raises the VisibleParticipantsChanged event
        /// </summary>
        protected virtual void OnVisibleParticipantsChanged(HashSet<string> visibleIds)
        {
            VisibleParticipantsChanged?.Invoke(this, new VisibleParticipantsChangedEventArgs(visibleIds));
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

        private void ParticipantControl_MaximizeRequested(object sender, RoutedEventArgs e)
        {
            if (sender is ParticipantControl control)
            {
                MaximizeParticipant(control.ParticipantData as ParticipantData);
            }
        }

        private void ParticipantBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Find the HoverOverlay within this border
                var overlay = FindVisualChild<Border>(border, "HoverOverlay");
                if (overlay != null)
                {
                    // Animate opacity to 1
                    var animation = new DoubleAnimation
                    {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    overlay.BeginAnimation(Border.OpacityProperty, animation);
                }
            }
        }

        private void ParticipantBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Find the HoverOverlay within this border
                var overlay = FindVisualChild<Border>(border, "HoverOverlay");
                if (overlay != null)
                {
                    // Animate opacity to 0
                    var animation = new DoubleAnimation
                    {
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };
                    overlay.BeginAnimation(Border.OpacityProperty, animation);
                }
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ParticipantData participant)
            {
                MaximizeParticipant(participant);
            }
        }

        private void MaximizeParticipant(ParticipantData? participant)
        {
            if (participant == null || Participants == null) return;

            _maximizedParticipant = participant;

            // Update maximized participant control
            MaximizedParticipantControl.Initial = participant.Initial;
            MaximizedParticipantControl.Username = participant.Username;

            // Populate thumbnails with all other participants
            _thumbnailParticipants.Clear();
            foreach (var p in Participants)
            {
                if (!ReferenceEquals(p, participant))
                {
                    _thumbnailParticipants.Add(p);
                }
            }
            ThumbnailItemsControl.ItemsSource = _thumbnailParticipants;

            // Switch to maximized view
            ParticipantsScrollViewer.Visibility = Visibility.Collapsed;
            MaximizedView.Visibility = Visibility.Visible;

            // Find and subscribe to thumbnail scrollviewer scroll events
            _thumbnailScrollViewer = FindThumbnailScrollViewer();
            if (_thumbnailScrollViewer != null)
            {
                // Unsubscribe first to avoid duplicate subscriptions
                _thumbnailScrollViewer.ScrollChanged -= OnThumbnailScrollChanged;
                _thumbnailScrollViewer.ScrollChanged += OnThumbnailScrollChanged;
                System.Diagnostics.Debug.WriteLine(">>> Subscribed to thumbnail ScrollViewer scroll events");
            }

            // Recalculate visible participants after maximizing
            CalculateVisibleParticipants();
        }

        private void RestoreGridView_Click(object sender, RoutedEventArgs e)
        {
            RestoreGridView();
        }

        private void RestoreGridView()
        {
            // Unsubscribe from thumbnail scrollviewer events
            if (_thumbnailScrollViewer != null)
            {
                _thumbnailScrollViewer.ScrollChanged -= OnThumbnailScrollChanged;
                _thumbnailScrollViewer = null;
                System.Diagnostics.Debug.WriteLine(">>> Unsubscribed from thumbnail ScrollViewer scroll events");
            }

            _maximizedParticipant = null;
            _thumbnailParticipants.Clear();
            ThumbnailItemsControl.ItemsSource = null;

            // Switch back to grid view
            MaximizedView.Visibility = Visibility.Collapsed;
            ParticipantsScrollViewer.Visibility = Visibility.Visible;

            // Recalculate visible participants after restoring grid view
            CalculateVisibleParticipants();
        }

        private void Thumbnail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ParticipantData participant)
            {
                MaximizeParticipant(participant);
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

        private T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && typedChild.Name == name)
                {
                    return typedChild;
                }

                var foundChild = FindVisualChild<T>(child, name);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }
            return null;
        }
    }
}
