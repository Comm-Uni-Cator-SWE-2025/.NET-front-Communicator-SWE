/*
 * -----------------------------------------------------------------------------
 *  File: ParticipantViewModelTests.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;
using Communicator.ScreenShare.Tests.Helpers;

namespace Communicator.ScreenShare.Tests.ViewModels.Meeting;

public class ParticipantViewModelTests
{
    private static WriteableBitmap CreateFrame()
        => new(1, 1, 96, 96, PixelFormats.Bgra32, null);

    [Fact]
    public void Constructor_InitializesDefaults_AndValidatesArguments()
    {
        // here i just make sure creating the view model works and nothing is null by mistake
        var user = TestHelpers.CreateUserProfile("user@example.com", "User Name", ParticipantRole.STUDENT);
        var viewModel = new ParticipantViewModel(user);

        Assert.Same(user, viewModel.User);
        Assert.Equal("User Name", viewModel.DisplayName);
        Assert.Equal("U", viewModel.Initial);
        Assert.False(viewModel.HasVideoFrame);
        Assert.False(viewModel.HasScreenFrame);
        Assert.False(viewModel.IsMuted);
        Assert.False(viewModel.IsCameraOn);
        Assert.False(viewModel.IsScreenSharing);
        Assert.False(viewModel.IsHandRaised);
        Assert.Null(viewModel.ProfileImageUri);

        Assert.Throws<ArgumentNullException>(() => new ParticipantViewModel(null!));
    }

    [Fact]
    public void FramePropertiesRaiseNotificationsAndUpdateFlags()
    {
        // this test checks that both video and screen frames raise property change events properly
        var participant = TestHelpers.CreateParticipant("frames@example.com", "Frames");
        var changes = new List<string>();
        participant.PropertyChanged += (_, args) => changes.Add(args.PropertyName!);
        var frame = CreateFrame();

        participant.VideoFrame = frame;
        Assert.Same(frame, participant.VideoFrame);
        Assert.True(participant.HasVideoFrame);
        Assert.Contains(nameof(ParticipantViewModel.VideoFrame), changes);
        Assert.Contains(nameof(ParticipantViewModel.HasVideoFrame), changes);

        var changeCount = changes.Count;
        participant.VideoFrame = frame;
        Assert.Equal(changeCount, changes.Count);

        changes.Clear();
        participant.VideoFrame = null;
        Assert.False(participant.HasVideoFrame);
        Assert.Contains(nameof(ParticipantViewModel.HasVideoFrame), changes);

        changes.Clear();
        participant.ScreenFrame = frame;
        Assert.True(participant.HasScreenFrame);
        Assert.Contains(nameof(ParticipantViewModel.ScreenFrame), changes);
        Assert.Contains(nameof(ParticipantViewModel.HasScreenFrame), changes);

        changes.Clear();
        participant.ScreenFrame = null;
        Assert.False(participant.HasScreenFrame);
        Assert.Contains(nameof(ParticipantViewModel.HasScreenFrame), changes);
    }

    [Fact]
    public void BooleanPropertiesRaiseNotificationsAndIgnoreDuplicates()
    {
        // this one flips every boolean and ensures we only raise events when the value really changes
        var participant = TestHelpers.CreateParticipant("bools@example.com", "Bools");
        var raised = new List<string>();
        participant.PropertyChanged += (_, args) => raised.Add(args.PropertyName!);

        void Toggle(string propertyName, Action setTrue, Action setFalse)
        {
            var before = raised.Count;
            setTrue();
            Assert.True(raised.Count > before);
            Assert.Equal(propertyName, raised.Last());

            before = raised.Count;
            setTrue();
            Assert.Equal(before, raised.Count);

            setFalse();
        }

        Toggle(nameof(ParticipantViewModel.IsMuted), () => participant.IsMuted = true, () => participant.IsMuted = false);
        Toggle(nameof(ParticipantViewModel.IsCameraOn), () => participant.IsCameraOn = true, () => participant.IsCameraOn = false);
        Toggle(nameof(ParticipantViewModel.IsScreenSharing), () => participant.IsScreenSharing = true, () => participant.IsScreenSharing = false);
        Toggle(nameof(ParticipantViewModel.IsHandRaised), () => participant.IsHandRaised = true, () => participant.IsHandRaised = false);
    }

    [Fact]
    public void DisplayInitialAndProfileImageScenariosBehaveAsExpected()
    {
        // this test covers all the display-name and avatar cases so initials always match expectations
        var withDisplayName = new ParticipantViewModel(new UserProfile { Email = "named@example.com", DisplayName = "Named", LogoUrl = new Uri("https://example.com/logo.png") });
        Assert.Equal("Named", withDisplayName.DisplayName);
        Assert.Equal("N", withDisplayName.Initial);
        Assert.Equal(new Uri("https://example.com/logo.png"), withDisplayName.ProfileImageUri);

        var withEmailOnly = new ParticipantViewModel(new UserProfile { Email = "emailonly@example.com", DisplayName = null });
        Assert.Equal("emailonly@example.com", withEmailOnly.DisplayName);
        Assert.Equal("E", withEmailOnly.Initial);
        Assert.Null(withEmailOnly.ProfileImageUri);

        var withNothing = new ParticipantViewModel(new UserProfile { Email = null, DisplayName = null });
        Assert.Equal("Unknown", withNothing.DisplayName);
        Assert.Equal("U", withNothing.Initial);

        var emptyName = new ParticipantViewModel(new UserProfile { Email = null, DisplayName = string.Empty });
        Assert.Equal(string.Empty, emptyName.DisplayName);
        Assert.Equal("?", emptyName.Initial);
    }
}

