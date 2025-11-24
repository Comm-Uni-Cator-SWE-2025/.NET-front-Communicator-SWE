# Fix for Maximized Mode Thumbnail Visibility

## Issue

Currently, in maximized mode, ALL thumbnails are marked as visible, even if they're scrolled out of view in the sidebar.

## Solution

We need to check if each thumbnail is within the sidebar's ScrollViewer viewport, similar to how we check grid mode visibility.

## Implementation

### Step 1: Update CalculateVisibleParticipants Method

Replace the maximized mode thumbnail section with viewport-aware checking:

```csharp
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
    System.Diagnostics.Debug.WriteLine($"  - Thumbnails count: {_thumbnailParticipants.Count}");
    
    // Find the thumbnail ScrollViewer
    var thumbnailScrollViewer = FindVisualChild<ScrollViewer>(MaximizedView);
    if (thumbnailScrollViewer != null && thumbnailScrollViewer != ParticipantsScrollViewer)
    {
        // Get the sidebar viewport rectangle
        var sidebarViewportRect = new Rect(
            thumbnailScrollViewer.HorizontalOffset,
            thumbnailScrollViewer.VerticalOffset,
            thumbnailScrollViewer.ViewportWidth,
            thumbnailScrollViewer.ViewportHeight
        );

        System.Diagnostics.Debug.WriteLine($"  - Sidebar Viewport: Y={sidebarViewportRect.Y:F2}, H={sidebarViewportRect.Height:F2}");

        // Check each thumbnail's visibility
        int thumbnailIndex = 0;
        foreach (var thumbnail in _thumbnailParticipants)
        {
            if (string.IsNullOrEmpty(thumbnail.Id))
            {
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
                    
                    System.Diagnostics.Debug.WriteLine($"  [{thumbnailIndex}] Thumbnail: {thumbnail.Username}");
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
```

### Step 2: Add Helper Method to Find Thumbnail Borders

```csharp
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
```

### Step 3: Subscribe to Thumbnail ScrollViewer Changes

Add this to the MaximizeParticipant method:

```csharp
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

    // NEW: Subscribe to thumbnail scrollviewer scroll events
    var thumbnailScrollViewer = FindVisualChild<ScrollViewer>(MaximizedView);
    if (thumbnailScrollViewer != null && thumbnailScrollViewer != ParticipantsScrollViewer)
    {
        // Unsubscribe first to avoid duplicate subscriptions
        thumbnailScrollViewer.ScrollChanged -= OnThumbnailScrollChanged;
        thumbnailScrollViewer.ScrollChanged += OnThumbnailScrollChanged;
    }

    // Recalculate visible participants after maximizing
    CalculateVisibleParticipants();
}

/// <summary>
/// Handler for thumbnail sidebar scroll changes
/// </summary>
private void OnThumbnailScrollChanged(object sender, ScrollChangedEventArgs e)
{
    // Recalculate visible participants when thumbnail sidebar is scrolled
    CalculateVisibleParticipants();
}
```

## Benefits of This Fix

1. ? **Accurate Tracking**: Only thumbnails actually visible in sidebar are reported
2. ? **Scroll Responsive**: Updates when user scrolls the thumbnail sidebar
3. ? **Consistent Logic**: Uses same intersection algorithm as grid mode
4. ? **Robust**: Has fallback behavior if scrollviewer not found
5. ? **Debuggable**: Detailed logging shows exactly which thumbnails are visible

## Testing the Fix

1. Add 10+ participants
2. Maximize one participant
3. Check output - should show maximized participant + only visible thumbnails
4. Scroll thumbnail sidebar down
5. Check output - should update to show different thumbnails as visible
6. Verify event only fires when visible set changes

## Alternative: Simpler Approach

If you want to keep the current behavior (all thumbnails always visible in maximized mode), that's also valid depending on your use case:

- **Keep current**: All thumbnails are "visible" because they're in the sidebar (even if scrolled)
- **Use this fix**: Only report thumbnails that are actually on-screen in the viewport

The choice depends on your backend requirements:
- If backend needs to stream video for all sidebar participants ? keep current
- If backend should only stream for what user can actually see ? use this fix

## Recommendation

Implement this fix to be consistent with the Java reference code behavior and provide accurate "what the user can actually see" tracking.
