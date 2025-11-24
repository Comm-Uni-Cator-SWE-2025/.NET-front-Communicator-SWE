/*
 * -----------------------------------------------------------------------------
 *  File: VideoSessionViewModelTests.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Communicator.App.Services;
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;
using Communicator.ScreenShare.Tests.Helpers;
using Communicator.ScreenShare.Tests.Mocks;

namespace Communicator.ScreenShare.Tests.ViewModels.Meeting;

public sealed class VideoSessionViewModelTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    private static WriteableBitmap CreateFrame()
        => new(1, 1, 96, 96, PixelFormats.Bgra32, null);

    private static void EnsureApplication()
    {
        if (Application.Current == null)
        {
            new Application();
        }
    }

    [Fact]
    public void ConstructorAndGridLayoutsCoverAllThresholds()
    {
        // this test loops through all grid breakpoints to prove the view model sets columns/rows correctly
        var user = TestHelpers.CreateUserProfile("host@example.com", "Host", ParticipantRole.INSTRUCTOR);
        var expectations = new (int Count, int Columns, int Rows)[]
        {
            (0, 1, 1),
            (1, 1, 1),
            (2, 2, 1),
            (3, 2, 2),
            (5, 3, 2),
            (7, 3, 3),
            (10, 4, 3),
            (16, 4, 4),
            (20, 5, 4)
        };

        foreach (var (count, expectedColumns, expectedRows) in expectations)
        {
            var viewModel = new VideoSessionViewModel(user, TestHelpers.CreateParticipants(count));
            _disposables.Add(viewModel);
            Assert.Equal("Meeting", viewModel.Title);
            Assert.Same(user, viewModel.CurrentUser);
            Assert.Equal(expectedColumns, viewModel.GridColumns);
            Assert.Equal(expectedRows, viewModel.GridRows);
        }

        var participants = new ObservableCollection<ParticipantViewModel>
        {
            TestHelpers.CreateParticipant("regular@example.com", "Regular"),
            TestHelpers.CreateParticipant("sharer@example.com", "Sharer", isScreenSharing: true)
        };
        var sortingViewModel = new VideoSessionViewModel(user, participants);
        _disposables.Add(sortingViewModel);
        Assert.True(sortingViewModel.SortedParticipants.First().IsScreenSharing);
    }

    [Fact]
    public void CollectionChangesAndSortingRespondToParticipantUpdates()
    {
        // i add/remove participants and flip screen sharing to make sure sorting and row counts adjust
        var user = TestHelpers.CreateUserProfile("host@example.com", "Host");
        var observer = TestHelpers.CreateParticipant("observer@example.com", "Observer");
        var sharer = TestHelpers.CreateParticipant("sharer@example.com", "Sharer", isScreenSharing: true);
        var participants = new ObservableCollection<ParticipantViewModel> { observer, sharer };
        var viewModel = new VideoSessionViewModel(user, participants);
        _disposables.Add(viewModel);

        bool rowsChanged = false;
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(VideoSessionViewModel.GridRows))
            {
                rowsChanged = true;
            }
        };

        var newcomer = TestHelpers.CreateParticipant("newcomer@example.com", "Newcomer");
        participants.Add(newcomer);
        Assert.True(rowsChanged);

        participants.Remove(observer);
        sharer.IsScreenSharing = false;
        newcomer.IsScreenSharing = true;
        Assert.Same(newcomer, viewModel.SortedParticipants.First());
    }

    [Fact]
    public void ParticipantCommandFlowHandlesAllViewModesAndParameters()
    {
        // this is the click story: null, wrong type, same person, other person, screen focus etc
        var user = TestHelpers.CreateUserProfile("host@example.com", "Host");
        var focus = TestHelpers.CreateParticipant("focus@example.com", "Focus", isScreenSharing: true);
        var other = TestHelpers.CreateParticipant("other@example.com", "Other");
        var participants = new ObservableCollection<ParticipantViewModel> { focus, other };
        var viewModel = new VideoSessionViewModel(user, participants);
        _disposables.Add(viewModel);

        var notifications = new List<string>();
        viewModel.PropertyChanged += (_, args) => notifications.Add(args.PropertyName!);

        viewModel.ParticipantClickCommand.Execute(null);
        viewModel.ParticipantClickCommand.Execute("unexpected parameter");

        viewModel.ParticipantClickCommand.Execute(focus);
        Assert.Equal(VideoViewMode.VideoFocus, viewModel.ViewMode);
        Assert.Same(focus, viewModel.FocusedParticipant);

        focus.ScreenFrame = CreateFrame();
        viewModel.ParticipantClickCommand.Execute(focus);
        Assert.Equal(VideoViewMode.ScreenFocus, viewModel.ViewMode);

        viewModel.ParticipantClickCommand.Execute(focus);
        Assert.Equal(VideoViewMode.Grid, viewModel.ViewMode);

        viewModel.ParticipantClickCommand.Execute(other);
        Assert.Equal(VideoViewMode.VideoFocus, viewModel.ViewMode);
        viewModel.ParticipantClickCommand.Execute(other);
        Assert.Equal(VideoViewMode.Grid, viewModel.ViewMode);
        Assert.Null(viewModel.FocusedParticipant);

        Assert.Contains(nameof(VideoSessionViewModel.ViewMode), notifications);
        Assert.Contains(nameof(VideoSessionViewModel.FocusedParticipant), notifications);
    }

    [Fact]
    public void FrameProcessingStopShareAndDisposalCoverAllPaths()
    {
        // this test covers RPC frames, invalid payloads, stop share events, and dispose behaviour
        EnsureApplication();

        var user = TestHelpers.CreateUserProfile("host@example.com", "Host");
        var videoParticipant = TestHelpers.CreateParticipant("cam@example.com", "Cam");
        var sharingParticipant = TestHelpers.CreateParticipant("share@example.com", "Share", isScreenSharing: true);
        var ghostParticipant = TestHelpers.CreateParticipant("ghost@example.com", "Ghost");
        var participants = new ObservableCollection<ParticipantViewModel> { videoParticipant, sharingParticipant, ghostParticipant };
        var rpcService = new MockRpcEventService();

        var viewModelWithRpc = new VideoSessionViewModel(user, participants, rpcEventService: rpcService);
        var viewModelWithoutRpc = new VideoSessionViewModel(user, new ObservableCollection<ParticipantViewModel> { TestHelpers.CreateParticipant("solo@example.com", "Solo") });
        _disposables.Add(viewModelWithRpc);
        _disposables.Add(viewModelWithoutRpc);

        viewModelWithRpc.ParticipantClickCommand.Execute(sharingParticipant);

        rpcService.TriggerFrameReceived(RImageTestHelper.CreateSimpleRImageBytes("cam"));
        Assert.NotNull(videoParticipant.VideoFrame);

        rpcService.TriggerFrameReceived(RImageTestHelper.CreateRImageBytes("share", 1, 1));
        Assert.Equal(VideoViewMode.ScreenFocus, viewModelWithRpc.ViewMode);
        Assert.NotNull(sharingParticipant.ScreenFrame);

        rpcService.TriggerFrameReceived(RImageTestHelper.CreateRImageBytes("ghost", 0, 0));
        Assert.Null(ghostParticipant.VideoFrame);

        rpcService.TriggerFrameReceived(new byte[] { 1, 2, 3 });
        rpcService.TriggerStopShareReceived(Array.Empty<byte>());
        rpcService.TriggerStopShareReceived(new byte[] { 42 });

        var method = typeof(VideoSessionViewModel).GetMethod("CreateBitmapSourceFromIntArray", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.Null(method!.Invoke(null, new object[] { Array.Empty<int[]>() }));

        viewModelWithRpc.Dispose();
        viewModelWithRpc.Dispose();
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }
}

