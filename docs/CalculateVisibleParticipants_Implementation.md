# Calculate Visible Participants Functionality

## Overview
This document describes the implementation of the `CalculateVisibleParticipants` functionality in the ScreenShare.UX application, which tracks which participants are currently visible on the screen.

## Implementation Details

### 1. ParticipantData.cs - Added Unique Identifier
**Location**: `src\ScreenShare.UX\ParticipantData.cs`

**Changes**:
- Added `Id` property to uniquely identify each participant (similar to IP address in the Java implementation)
- This property is a string that can hold IP address, user ID, or any unique identifier

```csharp
public string Id { get; set; }
```

### 2. ParticipantsGridControl.cs - Core Functionality
**Location**: `src\ScreenShare.UX\Controls\ParticipantsGridControl.xaml.cs`

**New Features**:

#### a) Event Definition
```csharp
public event EventHandler<VisibleParticipantsChangedEventArgs>? VisibleParticipantsChanged;
```
- Event raised whenever the set of visible participants changes
- ViewModels can subscribe to this event to be notified of visibility changes

#### b) VisibleParticipantsChangedEventArgs Class
```csharp
public class VisibleParticipantsChangedEventArgs : EventArgs
{
    public HashSet<string> VisibleParticipantIds { get; set; }
}
```
- Contains a HashSet of participant IDs that are currently visible

#### c) CalculateVisibleParticipants() Method
**Purpose**: Calculates which participants are currently visible on the screen

**Logic**:
1. **Maximized Mode**: 
   - If a participant is maximized, adds the maximized participant ID
   - Also adds all thumbnail participant IDs (they are all visible in the sidebar)

2. **Grid Mode**:
   - Gets the viewport rectangle (visible area of ScrollViewer)
   - For each participant, finds its Border element in the visual tree
   - Checks if the participant's Border intersects with the visible viewport
   - Adds intersecting participants to the visible set

3. **Change Detection**:
   - Only raises the `VisibleParticipantsChanged` event if the visible set has changed
   - Prevents unnecessary event firing

**Returns**: `HashSet<string>` of visible participant IDs

#### d) Automatic Recalculation Triggers
The method is automatically called when:
- Control loads (`Loaded` event)
- User scrolls (`ScrollChanged` event)
- Participants collection changes
- User maximizes a participant
- User restores grid view

### 3. MainWindow.xaml.cs - Usage Example
**Location**: `src\ScreenShare.UX\MainWindow.xaml.cs`

**Changes**:
- Participants are now created with unique IDs
- Example: `Id = "participant_1"`, `Id = "main_user"`

**How to Subscribe** (for ViewModels):
```csharp
private void MainWindow_Loaded(object sender, RoutedEventArgs e)
{
    if (ParticipantsGridControl != null)
    {
        ParticipantsGridControl.VisibleParticipantsChanged += OnVisibleParticipantsChanged;
    }
}

private void OnVisibleParticipantsChanged(object? sender, VisibleParticipantsChangedEventArgs e)
{
    // e.VisibleParticipantIds contains HashSet<string> of visible IDs
    // Use this to notify your ViewModel/Model
    // Example: ScreenNVideoModel.UpdateVisibleParticipants(e.VisibleParticipantIds);
}
```

## Key Differences from Java Implementation

| Aspect | Java | C# WPF |
|--------|------|--------|
| Viewport | `scrollPane.getViewport().getViewRect()` | `ScrollViewer.ViewportWidth/Height with Offset` |
| Element Bounds | `panel.getBounds()` | `TransformToAncestor()` and `Rect` |
| Intersection Check | `Rectangle.intersects()` | `Rect.IntersectsWith()` |
| Collection Type | `Set<String>` | `HashSet<string>` |
| Event Model | Direct callback | Event with EventArgs |

## Usage in ViewModel

To use this functionality in your ViewModel layer:

```csharp
// In your ViewModel or Service class
public void InitializeScreenShare(ParticipantsGridControl gridControl)
{
    gridControl.VisibleParticipantsChanged += (sender, e) =>
    {
        // Update your model with visible participants
        UpdateVisibleParticipants(e.VisibleParticipantIds);
    };
}

private void UpdateVisibleParticipants(HashSet<string> visibleIds)
{
    // Send this information to your backend/model
    // Example: NetworkService.NotifyVisibleParticipants(visibleIds);
}
```

## Testing

1. Add multiple participants using the "Add Test User" button
2. Scroll through the grid - visible participants will be calculated
3. Maximize a participant - only maximized and thumbnails will be visible
4. Check Debug output for visible participant IDs

## Performance Considerations

- Calculation only runs when necessary (scroll, collection change, maximize/restore)
- Uses visual tree traversal efficiently
- Change detection prevents unnecessary event firing
- HashSet provides O(1) lookup for visibility checks

## Future Enhancements

1. Add throttling/debouncing for scroll events if performance is an issue
2. Add virtual scrolling for very large participant lists
3. Cache Border references to avoid repeated visual tree searches
4. Add metrics/logging for visibility tracking
