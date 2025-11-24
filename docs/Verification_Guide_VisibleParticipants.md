# Verification Guide: CalculateVisibleParticipants Functionality

## How to Verify the Implementation

### Step 1: Run the Application in Debug Mode
1. Start the ScreenShare.UX application in **Debug mode** (F5 in Visual Studio)
2. Make sure the **Output** window is visible (View ? Output or Ctrl+Alt+O)
3. Select **Debug** from the "Show output from" dropdown

### Step 2: Initial Load Test
**What to expect:**
When the application loads, you should see output like:

```
######## MainWindow Loaded - Subscribing to VisibleParticipantsChanged ########
? Successfully subscribed to VisibleParticipantsChanged event
###############################################################

=== CalculateVisibleParticipants Called ===
Maximized Participant: None
*** GRID MODE ***
  - Viewport: X=0.00, Y=0.00, W=XXX.XX, H=XXX.XX
  - Total Participants: 1
  [0] You (ID: main_user)
       Border: X=0.00, Y=0.00, W=XXX.XX, H=XXX.XX
       Visible: True
*** RESULT: 1 visible participants ***
Visible IDs: [main_user]
>>> VISIBLE PARTICIPANTS CHANGED - Raising Event <<<
===========================================

????????????????????????????????????????????????????????????
?  EVENT RECEIVED: VisibleParticipantsChanged             ?
????????????????????????????????????????????????????????????
Timestamp: HH:mm:ss.fff
Visible Participants Count: 1
Visible Participant IDs:
  ? main_user ? You
????????????????????????????????????????????????????????????
```

### Step 3: Add Participants Test
Click the "Add Test User" button several times.

**What to expect:**
- Each time you add a participant, you should see the calculation output
- Initially (with 1-4 participants), all should be visible
- Example output:
```
=== CalculateVisibleParticipants Called ===
*** GRID MODE ***
  - Total Participants: 3
  [0] You (ID: main_user)
       Visible: True
  [1] username1 (ID: participant_1)
       Visible: True
  [2] username2 (ID: participant_2)
       Visible: True
*** RESULT: 3 visible participants ***
```

### Step 4: Scroll Test (with 5+ participants)
1. Add 5 or more participants (scrollbar will appear)
2. Scroll up and down in the participant grid

**What to expect:**
- As you scroll, you'll see the calculation output showing which participants are visible
- Participants scrolled out of view will show `Visible: False`
- The event will fire when visible set changes
```
=== CalculateVisibleParticipants Called ===
*** GRID MODE ***
  - Viewport: X=0.00, Y=150.00, W=750.00, H=400.00
  - Total Participants: 6
  [0] You (ID: main_user)
       Border: X=0.00, Y=0.00, W=375.00, H=200.00
       Visible: False  ? Scrolled out of view
  [1] username1 (ID: participant_1)
       Visible: False
  [2] username2 (ID: participant_2)
       Visible: True   ? In viewport
  [3] username3 (ID: participant_3)
       Visible: True
```

###Step 5: Maximize Test
1. Hover over any participant (black bar appears at bottom)
2. Click the maximize icon (plus sign)

**What to expect:**
```
=== CalculateVisibleParticipants Called ===
Maximized Participant: username1
*** MAXIMIZED MODE ***
  - Maximized: username1 (ID: participant_1)
  - Thumbnails count: 5
  - Thumbnail: You (ID: main_user)
  - Thumbnail: username2 (ID: participant_2)
  - Thumbnail: username3 (ID: participant_3)
  - Thumbnail: username4 (ID: participant_4)
  - Thumbnail: username5 (ID: participant_5)
*** RESULT: 6 visible participants ***
Visible IDs: [participant_1, main_user, participant_2, participant_3, participant_4, participant_5]
```

### Step 6: Thumbnail Click Test
While in maximized mode, click on different thumbnails in the sidebar.

**What to expect:**
- Each thumbnail click triggers the calculation
- The maximized participant changes
- All participants remain visible (maximized + all thumbnails)

### Step 7: Restore Grid View Test
Click the "Restore Grid View" button.

**What to expect:**
- Returns to grid mode
- Calculation runs again
- Shows which participants are visible in the restored grid

## Success Criteria

? **Functionality is working if:**
1. Initial load shows the first participant as visible
2. Adding participants triggers recalculation
3. Scrolling shows different participants becoming visible/hidden
4. Maximize mode shows all participants as visible (maximized + thumbnails)
5. Thumbnail clicks update the visible set correctly
6. Restore grid view returns to normal visibility detection
7. Event is raised only when visible set actually changes
8. The `VisibleParticipantsChanged` event is received in MainWindow

## Common Issues to Watch For

? **Issue**: No output in Debug window
**Solution**: Make sure you're running in Debug mode and Output window is set to "Debug"

? **Issue**: "Border NOT FOUND in visual tree"
**Solution**: This might happen initially before the visual tree is fully loaded. Should resolve after a moment.

? **Issue**: All participants always show as visible even when scrolling
**Solution**: Check that viewport rectangle and border rectangle calculations are correct

? **Issue**: Event never fires
**Solution**: Make sure MainWindow_Loaded is being called and subscription is successful

## Next Steps for Integration

Once verified, the ViewModel can use this functionality:

```csharp
// In your ViewModel
ParticipantsGridControl.VisibleParticipantsChanged += (sender, e) =>
{
    // Update your model/backend
    ScreenNVideoModel.Instance.UpdateVisibleParticipants(e.VisibleParticipantIds);
};
```

Or manually query at any time:
```csharp
HashSet<string> visibleIds = ParticipantsGridControl.CalculateVisibleParticipants();
```
